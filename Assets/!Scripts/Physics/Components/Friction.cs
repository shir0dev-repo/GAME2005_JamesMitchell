using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Gravity))]
public class Friction : PhysicsComponentBase
{
    public Vector3 m_normalForce = Vector3.zero, m_frictionForce = Vector3.zero, m_gravityForce = Vector3.zero;
    public float FrictionCoefficient => m_frictionCoefficient;
    [SerializeField, Range(0, 1)] private float m_frictionCoefficient = 0.5f;

    public Vector3 ActiveFriction => m_activeFriction;
    [SerializeField, ReadOnly] private Vector3 m_activeFriction = Vector3.zero;

    private Gravity m_gravity;
    private CollisionComponent m_collisionComponent;
    protected override void Awake()
    {
        base.Awake();
        m_gravity = GetComponent<Gravity>();
        m_collisionComponent = GetComponent<CollisionComponent>();
    }

    public override Vector3 GetForce(Vector3 initial, Vector3 collisionDisplacement)
    {
        
        m_gravityForce = m_gravity.GForce;
        float g = m_gravityForce.magnitude;
        m_frictionForce = Vector3.zero;
        
        if (m_collisionComponent.CurrentCollisions.Count <= 0)
        {
            m_normalForce = Vector3.zero;
            return Vector3.zero;
        }

        m_normalForce = -m_gravityForce;
        float nDotG = float.MaxValue;
        Vector3 normGravity = m_gravityForce.normalized;

        foreach (CollisionData cd in m_collisionComponent.CurrentCollisions)
        {
            // find the collider with the collision normal pointing the most away from gravity
            float d = Vector3.Dot(cd.CollisionNormal, m_gravityForce);
            if (d < nDotG)
            {
                nDotG = d;
                m_normalForce = cd.CollisionNormal;
            }
        }

        Vector3 m2 = m_normalForce.normalized * nDotG;
        Vector3 frictionDirection = (m2 - m_gravityForce).normalized;
        float frictionMagnitude = Vector3.Dot(m_gravityForce, -frictionDirection);
        float fDotN = Vector3.Dot(-frictionDirection, m2);
        if (fDotN == 1)
            m_frictionForce = Vector3.zero;
        else if (frictionDirection != Vector3.zero) 
            m_frictionForce = frictionDirection * frictionMagnitude;

        m_normalForce.Normalize();
        return m_frictionForce * PhysicsBodyUpdateSystem.TimeStep;
    }

    public void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + m_normalForce.normalized);

        Gizmos.color = new Color(1.0f, 0.65f, 0, 1); // orange i guess
        Gizmos.DrawLine(transform.position, transform.position + m_frictionForce.normalized);

        Gizmos.color = new Color(1, 0, 1, 1); // purple
        Gizmos.DrawLine(transform.position, transform.position + m_gravityForce.normalized);
    }
}
