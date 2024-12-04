using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Gravity))]
public class Friction : PhysicsComponentBase
{
    public Vector3 m_normalForce = Vector3.zero, m_frictionForce = Vector3.zero, m_gravityForce = Vector3.zero;

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
        Debug.Log("applying");
        m_gravityForce = m_gravity.GForce;// * m_body.Mass;
        m_frictionForce = Vector3.zero;

        if (m_collisionComponent.CurrentCollisions.Count <= 0)
        {
            m_normalForce = Vector3.zero;
            return Vector3.zero;
        }


        m_normalForce = -m_gravityForce;
        float nDotG = float.MaxValue;

        CollisionData selectedCollision = m_collisionComponent.CurrentCollisions[0];

        foreach (CollisionData cd in m_collisionComponent.CurrentCollisions)
        {
            // find the collider with the collision normal pointing the most away from gravity
            float d = Vector3.Dot(cd.CollisionNormal, m_gravityForce);
            if (d < nDotG)
            {
                selectedCollision = cd;
                nDotG = d;
                m_normalForce = -Vector3.Project(m_gravityForce, cd.CollisionNormal);
            }
        }

        float sf = GetStaticFriction(selectedCollision);
        Vector3 relativeVelocity = initial - selectedCollision.Other.GetBody().Velocity;

        Vector3 tangentialVelocity = relativeVelocity - Vector3.Dot(relativeVelocity, m_normalForce) * m_normalForce;
        float tangentialSpeed = tangentialVelocity.magnitude;
        Vector3 tangentialDirection = tangentialVelocity.normalized;

        if (tangentialSpeed <= sf)
        {
            Debug.Log("static");
            m_frictionForce = -tangentialVelocity * m_normalForce.magnitude * sf;
        }
        else 
        {
            Debug.Log("kinetic");
            m_frictionForce = tangentialDirection * m_normalForce.magnitude * -GetKineticFriction(selectedCollision);
        }

        return m_frictionForce;
    }

    bool DidSurpassStaticFriction(Vector3 relVelocity, Vector3 normalForce, float staticFriction)
    {
        Vector3 frictionForce = normalForce * staticFriction;
        Vector3 perpendicular = relVelocity - Vector3.Project(relVelocity, normalForce);

        float vDotN = Vector3.Dot(relVelocity, frictionForce);
        return perpendicular.magnitude <= frictionForce.magnitude;
    }

    private float GetStaticFriction(CollisionData data)
    {
        float f = data.Other.GetBody().StaticFriction();
        return (f + m_body.StaticFriction()) / 2.0f;
    }

    private float GetKineticFriction(CollisionData data)
    {
        float roughnessOther = data.Other.GetBody().Roughness();
        if (m_body.Roughness() == 0 && roughnessOther == 0)
            return 0;

        float product = 2 * m_body.Roughness() * roughnessOther;
        return Mathf.Clamp(product / (m_body.Roughness() + roughnessOther), 0.01f, 1);
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
