using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CubeVolume : PhysicsVolume
{
    [SerializeField] private Vector3 m_halfExtents = Vector3.one * 0.5f;
    public Vector3 HalfExtents => m_halfExtents;
    
    private Vector3[] m_points = new Vector3[8];
    public Vector3[] Points => m_points;

    private Vector3 m_lastKnownNormal = Vector3.zero;
    private Vector3[] m_projectedPoints = null;
    private void OnEnable()
    {
        PhysicsManager.OnPhysicsUpdate += CalculateShape;
    }

    private void OnDisable()
    {
        PhysicsManager.OnPhysicsUpdate -= CalculateShape;
    }

    private void CalculateShape(object physicsManager, float dt) => CalculateShape();
    private void CalculateShape()
    {
        Quaternion rotation = Rotation;
        
        m_points[0] = rotation * new Vector3(m_halfExtents.x, -m_halfExtents.y, m_halfExtents.z); // rbf
        m_points[1] = rotation * new Vector3(-m_halfExtents.x, -m_halfExtents.y, m_halfExtents.z); // lbf
        m_points[2] = rotation * new Vector3(m_halfExtents.x, m_halfExtents.y, m_halfExtents.z); // rtf
        m_points[3] = rotation * new Vector3(-m_halfExtents.x, m_halfExtents.y, m_halfExtents.z); // ltf
        m_points[4] = rotation * new Vector3(m_halfExtents.x, -m_halfExtents.y, -m_halfExtents.z); // rbb
        m_points[5] = rotation * new Vector3(-m_halfExtents.x, -m_halfExtents.y, -m_halfExtents.z); // lbb
        m_points[6] = rotation * new Vector3(m_halfExtents.x, m_halfExtents.y, -m_halfExtents.z); // rtb
        m_points[7] = rotation * new Vector3(-m_halfExtents.x, m_halfExtents.y, -m_halfExtents.z); // ltb
    }
    
    public override float CrossSectionalArea(Vector3 inNormal)
    {
        m_lastKnownNormal = inNormal.normalized;

        // all points are in "plane space" (x,y) relative to origin of plane
        List<Vector3> projectedPoints = ProjectPointsToPlaneSpace(m_points, m_lastKnownNormal);

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
            if (hull.Count > 1 && PointOrientation(hull.ToList()[1], hull.Peek(), point) < 0)
                hull.Pop();

            hull.Push(point);
        }
        m_projectedPoints = hull.ToArray();
        m_projectedPoints.Reverse();

        return ShoelaceArea(m_projectedPoints);
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        Gizmos.color = Color.green;
        Gizmos.matrix = transform.localToWorldMatrix;
        
        Vector3 scale = transform.localScale;
        transform.localScale = m_halfExtents;
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
        transform.localScale = scale;
        
        Gizmos.matrix = Matrix4x4.identity;

        Gizmos.color = Color.blue;
        if (m_lastKnownNormal != Vector3.zero)
        {
            Gizmos.DrawLine(Center, Center + m_lastKnownNormal);

            if (m_projectedPoints != null)
            {
                PlaneAxis p = new PlaneAxis(m_lastKnownNormal);
                Vector3[] projWS = new Vector3[m_projectedPoints.Length];
                for (int i = 0; i < projWS.Length; i++)
                {
                    projWS[i] = p.ToWorldSpace(Center, m_projectedPoints[i]); 
                }

                Gizmos.DrawLineStrip(new(projWS), true);
            }
        }
    }
}
