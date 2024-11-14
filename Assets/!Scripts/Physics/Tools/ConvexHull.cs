using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class ConvexHull
{
    private float m_area;
    private PlaneAxis m_axes;

    private float m_normalTolerance = 0.003f;

    public Vector3[] Vertices { get; set; }
    public Vector2[] Hull { get; private set; }
    public float Area { get => m_area; }
    public PlaneAxis Axes { get => m_axes; }


    public ConvexHull(Vector3 projectionNormal, Vector3[] vertices)
    {
        Vertices = vertices;
        m_axes = new PlaneAxis(projectionNormal);
        CalculateHull();
        m_area = CalculateArea();
    }

    /// <summary>
    ///     Calculates the area of a 2D convex hull.<br/>
    ///     <seealso href="https://artofproblemsolving.com/wiki/index.php/Shoelace_Theorem"/>
    /// </summary>
    /// <param name="hullPoints">A collection of hull points, ordered counter-clockwise</param>
    /// <returns>The area of the hull.</returns>
    private float CalculateArea()
    {
        Vector2[] hullPoints = Hull;
        float area = 0f;
        int size = hullPoints.Length;
        for (int i = 0; i < size; i++)
        {
            area += (hullPoints[i].x * hullPoints[(i + 1) % size].y) - (hullPoints[(i + 1) % size].x * hullPoints[i].y);
        }

        area += (hullPoints[size - 1].x * hullPoints[0].y) - (hullPoints[0].x * hullPoints[size - 1].y);

        area = Mathf.Abs(area * 0.5f);
        m_area = area;
        return area;
    }

    /// <summary>
    ///     Projects a collection of points onto a plane, returning their local coordinates.
    /// </summary>
    /// <param name="points">Collection to project</param>
    /// <param name="inNormal">Normal of the plane</param>
    /// <returns>A collection of points projected onto plane defined by <paramref name="inNormal"/>.</returns>
    private static List<Vector2> ProjectPointsToPlaneSpace(IEnumerable<Vector3> points, Vector3 inNormal)
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
    private static List<Vector2> ProjectPointsToPlaneSpace(IEnumerable<Vector3> points, Vector3 inNormal, out PlaneAxis axis)
    {
        List<Vector2> result = new List<Vector2>();
        axis = new PlaneAxis(inNormal);

        foreach (Vector3 point in points)
        {
            Vector2 projectedLocal = axis.Project(point);

            if (!result.Contains(projectedLocal))
            {
                result.Add(projectedLocal);
            }
        }
        return result;
    }

    private void CalculateHull()
    {
        Vector3 normal = m_axes.Normal;

        // all points are in "plane space" (x,y) relative to origin of plane
        List<Vector2> projectedPoints = ProjectPointsToPlaneSpace(Vertices, normal);

        Vector2 initial = new(Mathf.Infinity, Mathf.Infinity);
        foreach (Vector2 point in projectedPoints)
        {
            if (point.y < initial.y) initial = point;
            else if (point.y == initial.y && point.x < initial.x) initial = point;
            else if (point.y == initial.y && point.x == initial.x) initial = point;
            //else if (point.y == initial.y && point.x == initial.x && point.z < initial.z) initial = point;
        }

        projectedPoints.Remove(initial);

        projectedPoints = projectedPoints.OrderBy(p =>
        {
            Vector2 delta = (p - initial).normalized;
            float angle = Mathf.Atan2(delta.y, delta.x);

            return angle;
        }).ToList();

        Stack<Vector2> hull = new Stack<Vector2>();
        hull.Push(initial);

        foreach (Vector2 point in projectedPoints)
        {
            if (hull.Count > 1 && PointOrientation(hull.ToList()[1], hull.Peek(), point) < 0)
                hull.Pop();

            hull.Push(point);
        }
        Hull = hull.ToArray();
        Hull.Reverse();

        CalculateArea();
    }

    public void CalculateHull(Vector3 inNormal)
    {
        if (Vector3.Dot(inNormal, m_axes.Normal) > m_normalTolerance)
            m_axes = new PlaneAxis(inNormal);
        CalculateHull();
        
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
    private static float PointOrientation(Vector2 first, Vector2 current, Vector2 next)
    {
        float z = (current.x - first.x) * (next.y - first.y) -
            (current.y - first.y) * (next.x - first.x); // z-component of the cross product
        if (z == 0) return 0;
        else return (z < 0) ? -1 : 1;
    }
}
