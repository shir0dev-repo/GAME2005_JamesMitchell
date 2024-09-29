using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        Array.Fill(m_points, m_rotation * m_center);
        // create 8 axis aligned points
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
        Vector3 normal = Vector3.Normalize(inNormal);

        List<Vector3> projectedPoints = ProjectPoints(m_points, normal);

        Vector3 p0 = projectedPoints[0];

        foreach (Vector3 p in projectedPoints)
        {
            if (p.y < p0.y || (p.y == p0.y && p.x < p0.x)) p0 = p;
        }
        Debug.DrawLine(m_center, p0, Color.red);
        projectedPoints.RemoveAll(p => p == p0);
        Debug.Log(p0);
        Vector3 axis = Vector3.Cross(p0, normal).normalized;
        projectedPoints = projectedPoints
            .Distinct()
            .OrderBy(p => p.x)/*{
                float a = Vector3.SignedAngle(p0, p, normal);
                ;

                return a;
            })*/
            .ToList();

        foreach (Vector3 p in projectedPoints) Debug.Log(p);
        Debug.Log(projectedPoints.Count);
        
        Stack<Vector3> hull = new Stack<Vector3>();
        hull.Push(p0);

        foreach (Vector3 point in projectedPoints)
        {
            while (hull.Count > 1 && Orientation(PeekTwice(hull), hull.Peek(), point, normal) <= 0)
                hull.Pop();
            
            hull.Push(point);
        }
        List<Vector3> hullList = hull.ToList();
        Debug.Log(hullList.Count);
        for (int i = 0; i < hullList.Count - 1; i++)
        {
            Debug.DrawLine(m_center + hullList[i], m_center + hullList[i + 1], Color.blue);
        }

        Debug.DrawLine(m_center + hullList[^1], m_center + hullList[0], Color.blue);

        return ShoelaceTheorem(hullList);
    }

    private float ShoelaceTheorem(List<Vector3> hullList)
    {
        float area = 0;
        for (int i = 0; i < hullList.Count - 1; i++)
        {
            area += (hullList[i].y * hullList[i + 1].z) + (hullList[i].x * hullList[i + 1].y) + (hullList[i + 1].x * hullList[i].z) -
                    ((hullList[i + 1].x * hullList[i].y) + (hullList[i + 1].y * hullList[i].z) + (hullList[i].x * hullList[i + 1].z));
        }
        
        area += (hullList[^1].y * hullList[0].z) + (hullList[^1].x * hullList[0].y) + (hullList[0].x * hullList[^1].z) -
                    ((hullList[0].x * hullList[^1].y) + (hullList[0].y * hullList[^1].z) + (hullList[^1].x * hullList[0].z));

        area = Mathf.Abs(area) * 0.5f;
        Debug.Log(area);
        
        return area;
    }

    private Vector3 PeekTwice(Stack<Vector3> stack) { return stack.ToList()[stack.Count - 2]; }

    private List<Vector3> ProjectPoints(Vector3[] points, Vector3 normal)
    {
        List<Vector3> result = new List<Vector3>();

        for (int i = 0; i < points.Length; i++)
        {
            Vector3 projected = points[i] - Vector3.Dot(points[i], normal) * normal;
            Debug.DrawLine(points[i], projected, Color.green);
            result.Add(projected);
        }

        return result;
    }

    private float Orientation(Vector3 p, Vector3 q, Vector3 r, Vector3 normal)
    {
        Debug.DrawLine(p, q, Color.magenta);
        Debug.DrawLine(p, r, Color.magenta);
        float a = Vector3.SignedAngle(p + q, p + r, normal);

        Debug.Log(a);

        return a;
    }
}
