using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PhysicsVolume
{
    private Vector3 m_halfExtents;
    public Vector3 HalfExtents => m_halfExtents;

    private Vector3 m_center;
    public Vector3 Center => m_center;

    private Quaternion m_rotation;
    public Quaternion Rotation => m_rotation;

    private Vector3[] m_points = new Vector3[8];
    public Vector3[] Points => m_points;

    public PhysicsVolume(Vector3 center, Quaternion rotation, Vector3 halfExtents)
    {
        m_center = center;
        m_rotation = rotation;
        m_halfExtents = halfExtents;
        CalculateShape();
    }

    public void CalculateShape()
    {
        m_points[0] = m_rotation * new Vector3(m_halfExtents.x, -m_halfExtents.y, m_halfExtents.z); // rbf
        m_points[1] = m_rotation * new Vector3(-m_halfExtents.x, -m_halfExtents.y, m_halfExtents.z); // lbf
        m_points[2] = m_rotation * new Vector3(m_halfExtents.x, m_halfExtents.y, m_halfExtents.z); // rtf
        m_points[3] = m_rotation * new Vector3(-m_halfExtents.x, m_halfExtents.y, m_halfExtents.z); // ltf
        m_points[4] = m_rotation * new Vector3(m_halfExtents.x, -m_halfExtents.y, -m_halfExtents.z); // rbb
        m_points[5] = m_rotation * new Vector3(-m_halfExtents.x, -m_halfExtents.y, -m_halfExtents.z); // lbb
        m_points[6] = m_rotation * new Vector3(m_halfExtents.x, m_halfExtents.y, -m_halfExtents.z); // rtb
        m_points[7] = m_rotation * new Vector3(-m_halfExtents.x, m_halfExtents.y, -m_halfExtents.z); // ltb
    }

    public void UpdatePositionAndRotation(Vector3 center, Quaternion rotation)
    {
        m_center = center;
        m_rotation = rotation;
        CalculateShape();
    }

    public float CrossSectionalArea(Vector3 inNormal)
    {

        inNormal = inNormal.normalized;
        Debug.DrawLine(m_center, m_center + inNormal, Color.green);
        // all points are in "plane space" (x,y) relative to origin of plane
        List<Vector3> projectedPoints = ProjectPointsToPlaneSpace(m_points, inNormal, out (Vector3 tangent, Vector3 bitangent) axes);

        Vector3 initial = new(Mathf.Infinity, Mathf.Infinity);
        foreach (Vector3 point in projectedPoints)
        {
            if (point.y < initial.y) initial = point;
            else if (point.y == initial.y && point.x < initial.x) initial = point;
            else if (point.y == initial.y && point.x == initial.x && point.z < initial.z) initial = point;
        }

        projectedPoints.Remove(initial);

        projectedPoints = projectedPoints.OrderBy(p =>
        {
            Vector2 delta = (p - initial).normalized;
            float angle = Mathf.Atan2(delta.y, delta.x);

            return angle;
        }).ToList();

        Stack<Vector3> hull = new Stack<Vector3>();
        hull.Push(initial);

        foreach (Vector3 point in projectedPoints)
        {
            if (hull.Count > 1 && Orientation(hull.ToList()[1], hull.Peek(), point) < 0)
                hull.Pop();

            hull.Push(point);
        }
        List<Vector3> convexHull = hull.ToList();
        convexHull.Reverse();
        List<Vector3> hullWS = new List<Vector3>();
        hullWS.AddRange(convexHull);



        for (int i = 0; i < hullWS.Count; i++)
        {
            hullWS[i] = m_center + (hullWS[i].x * axes.tangent) + (hullWS[i].y * axes.bitangent);
        }

        for (int i = 0; i < hullWS.Count; i++)
        {
            Debug.DrawLine(hullWS[i], hullWS[(i + 1) % hullWS.Count], Color.blue);
        }

        Debug.DrawLine(hullWS[^1], hullWS[0], Color.blue);

        return ShoelaceArea(convexHull);
    }

    private float ShoelaceArea(List<Vector3> pointsCCW)
    {
        float area = 0f;
        int size = pointsCCW.Count;
        for (int i = 0; i < size; i++)
        {
            area += (pointsCCW[i].x * pointsCCW[(i + 1) % size].y) - (pointsCCW[(i + 1) % size].x * pointsCCW[i].y);
        }

        area += (pointsCCW[size - 1].x * pointsCCW[0].y) - (pointsCCW[0].x * pointsCCW[size - 1].y);

        area = Mathf.Abs(area * 0.5f);
        return area;
    }

    private List<Vector3> ProjectPointsToPlaneSpace(Vector3[] m_points, Vector3 inNormal, out (Vector3 tangent, Vector3 bitangent) axes)
    {
        List<Vector3> result = new List<Vector3>();

        if (Mathf.Abs(inNormal.x) > Mathf.Abs(inNormal.y) && Mathf.Abs(inNormal.x) > Mathf.Abs(inNormal.z))
        {
            axes.tangent = Vector3.Cross(inNormal, Vector3.up).normalized;
            axes.bitangent = Vector3.Cross(inNormal, axes.tangent).normalized;
        }
        else if (Mathf.Abs(inNormal.y) > Mathf.Abs(inNormal.x) && Mathf.Abs(inNormal.y) > Mathf.Abs(inNormal.z))
        {
            axes.tangent = Vector3.Cross(inNormal, Vector3.forward);
            axes.bitangent = Vector3.Cross(inNormal, axes.tangent).normalized;
        }
        else
        {
            axes.tangent = Vector3.Cross(inNormal, Vector3.up).normalized;
            axes.bitangent = Vector3.Cross(inNormal, axes.tangent).normalized;
        }

        foreach (Vector3 point in m_points)
        {
            Vector3 projectedLocal = new Vector3()
            {
                x = Vector3.Dot(point, axes.tangent),
                y = Vector3.Dot(point, axes.bitangent),
                z = 0
            };

            if (!result.Contains(projectedLocal))
            {
                result.Add(projectedLocal);
            }
        }
        return result;
    }

    private float Orientation(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        return (p2.x - p1.x) * (p3.y - p1.y) - (p2.y - p1.y) * (p3.x - p1.x); // z-component of the cross product
        
    }
}
