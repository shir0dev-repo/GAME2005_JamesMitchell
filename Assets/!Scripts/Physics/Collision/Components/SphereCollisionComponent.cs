using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereCollisionComponent : CollisionComponent
{
    public override ColliderType Type => ColliderType.Sphere;
    public override bool IsKinematic => m_isKinematic;
    [SerializeField] protected bool m_isKinematic = true;
    public override VelocityMode VelocityMode => m_velocityMode;
    [SerializeField] protected VelocityMode m_velocityMode = VelocityMode.Reflect;

    public float Radius => m_radius;
    [SerializeField] private float m_radius = 0.5f;

    /// <summary>Used for debugging the CSA and drawing it with gizmos.</summary>
    private Vector3 m_lastKnownNormal = Vector3.zero;

    public override float CrossSectionalArea(Vector3 inNormal)
    {
        m_lastKnownNormal = inNormal.normalized;
        return Mathf.PI * Mathf.Pow(m_radius, 2);
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        Gizmos.color = Color.green;
        
        if (m_lastKnownNormal != Vector3.zero)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(Center, Center + m_lastKnownNormal);
        }
    }
}
