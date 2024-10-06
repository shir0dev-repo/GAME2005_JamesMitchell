using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereVolume : PhysicsVolume
{
    [SerializeField] private float m_radius = 0.5f;
    public float Radius => m_radius;

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
        //Gizmos.DrawWireSphere(Center, m_radius);
        
        if (m_lastKnownNormal != Vector3.zero)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(Center, Center + m_lastKnownNormal);
        }
    }
}
