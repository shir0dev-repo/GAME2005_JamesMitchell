using System.Collections.Generic;
using UnityEngine;

public abstract class PhysicsVolume : MonoBehaviour
{
    public Vector3 Center => transform.position;
    public Quaternion Rotation => transform.rotation;

    /// <summary>
    ///     Calculates the cross sectional area of a shape perpendicular to <paramref name="inNormal"/>.
    /// </summary>
    /// <param name="inNormal">The normal of the cross section</param>
    /// <returns>
    ///     The area formed by the cross sectional projection of the shape onto the plane
    ///     defined by <paramref name="inNormal"/>.
    /// </returns>
    public abstract float CrossSectionalArea(Vector3 inNormal);

    /// <summary>
    ///     Calculates the area of a 2D convex hull.<br/>
    ///     <seealso href="https://artofproblemsolving.com/wiki/index.php/Shoelace_Theorem"/>
    /// </summary>
    /// <param name="hullPoints">A collection of hull points, ordered counter-clockwise</param>
    /// <returns>The area of the hull.</returns>
    protected static float ShoelaceArea(Vector3[] hullPoints)
    {
        float area = 0f;
        int size = hullPoints.Length;
        for (int i = 0; i < size; i++)
        {
            area += (hullPoints[i].x * hullPoints[(i + 1) % size].y) - (hullPoints[(i + 1) % size].x * hullPoints[i].y);
        }

        area += (hullPoints[size - 1].x * hullPoints[0].y) - (hullPoints[0].x * hullPoints[size - 1].y);

        area = Mathf.Abs(area * 0.5f);
        return area;
    }

    /// <summary>
    ///     Projects a collection of points onto a plane, returning their local coordinates.
    /// </summary>
    /// <param name="points">Collection to project</param>
    /// <param name="inNormal">Normal of the plane</param>
    /// <returns>A collection of points projected onto plane defined by <paramref name="inNormal"/>.</returns>
    protected static List<Vector3> ProjectPointsToPlaneSpace(IEnumerable<Vector3> points, Vector3 inNormal)
    {
        return ProjectPointsToPlaneSpace(points, inNormal, out _);
    }

    /// <summary>
    ///     Projects a collection of points onto a plane, returning their local coordinates.
    /// </summary>
    /// <param name="points">Collection to project</param>
    /// <param name="inNormal">Normal of the plane</param>
    /// <param name="axis">Three axis-aligned vectors of the plane</param>
    /// <returns>A collection of points projected onto plane defined by <paramref name="inNormal"/>.</returns>
    protected static List<Vector3> ProjectPointsToPlaneSpace(IEnumerable<Vector3> points, Vector3 inNormal, out PlaneAxis axis) 
    {
        List<Vector3> result = new List<Vector3>();
        axis = new PlaneAxis(inNormal);

        foreach (Vector3 point in points)
        {
            Vector3 projectedLocal = axis.Project(point);

            if (!result.Contains(projectedLocal))
            {
                result.Add(projectedLocal);
            }
        }
        return result;
    }

    /// <summary>
    ///     Calculates the orientation between three points (2D), 
    ///     <paramref name="first"/>, <paramref name="current"/>, and <paramref name="next"/>.
    /// </summary>
    /// <returns>
    ///     -1 if they are clockwise<br/>
    ///     0 if they are colinear <br/>
    ///     1 if they are counter-clockwise
    /// </returns>
    protected static float PointOrientation(Vector2 first, Vector2 current, Vector2 next)
    {
        float z = (current.x - first.x) * (next.y - first.y) - 
            (current.y - first.y) * (next.x - first.x); // z-component of the cross product
        if (z == 0) return 0;
        else return (z < 0) ? -1 : 1;
    }
}
