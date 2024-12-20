using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AABBCollisionVolume : PhysicsComponentBase, ICollisionVolume, IPhysicsVolume
{
    public ColliderType Type => ColliderType.AABB;
    public VelocityMode VelocityMode => m_velocityMode;
    [SerializeField] private VelocityMode m_velocityMode;

    public bool IsKinematic { get; private set; }

    public bool CurrentlyColliding { get; set; }
    public Stack<ICollisionVolume> CurrentCollisions { get => m_currentCollisions; }
    [SerializeField] private Stack<ICollisionVolume> m_currentCollisions = new();

    public Vector3 CurrentPartitionOrigin { get; set; }
    public Transform Transform { get => transform; }
    

    public Vector3 Center { get => transform.position; }
    public Quaternion Rotation { get => transform.rotation; }

    [SerializeField] private Vector3 m_halfExtents = Vector3.one * 0.5f;
    private readonly Vector3[] m_points = new Vector3[8];

    public Vector3 HalfExtents => m_halfExtents;
    public Vector3[] Points => m_points;

    

    private Vector3 m_lastKnownNormal = Vector3.zero;
    private ConvexHull m_projectedHull;

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

        m_points[0] = rotation * new Vector3( m_halfExtents.x, -m_halfExtents.y,  m_halfExtents.z); // rbf
        m_points[1] = rotation * new Vector3(-m_halfExtents.x, -m_halfExtents.y,  m_halfExtents.z); // lbf
        m_points[2] = rotation * new Vector3( m_halfExtents.x,  m_halfExtents.y,  m_halfExtents.z); // rtf
        m_points[3] = rotation * new Vector3(-m_halfExtents.x,  m_halfExtents.y,  m_halfExtents.z); // ltf
        m_points[4] = rotation * new Vector3( m_halfExtents.x, -m_halfExtents.y, -m_halfExtents.z); // rbb
        m_points[5] = rotation * new Vector3(-m_halfExtents.x, -m_halfExtents.y, -m_halfExtents.z); // lbb
        m_points[6] = rotation * new Vector3( m_halfExtents.x,  m_halfExtents.y, -m_halfExtents.z); // rtb
        m_points[7] = rotation * new Vector3(-m_halfExtents.x,  m_halfExtents.y, -m_halfExtents.z); // ltb
    }
    
    public float CrossSectionalArea(Vector3 inNormal)
    {
        m_lastKnownNormal = inNormal;

        if (m_projectedHull == null)
        {
            m_projectedHull = new ConvexHull(m_lastKnownNormal, m_points);
        }
        else
        {
            m_projectedHull.CalculateHull(m_lastKnownNormal);
        }

        return m_projectedHull.Area;
    }

    public override Vector3 Modify(Vector3 initial)
    {
        return initial;
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        Gizmos.color = Color.red;
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

            if (m_projectedHull != null)
            {
                Gizmos.color = Color.green;
                PlaneAxis p = m_projectedHull.Axes;
                Vector3[] projWS = new Vector3[m_projectedHull.Hull.Length];
                for (int i = 0; i < projWS.Length; i++)
                {
                    projWS[i] = p.ToWorldSpace(Center, m_projectedHull.Hull[i]);
                }

                Gizmos.DrawLineStrip(new(projWS), true);
            }
        }
    }
}
