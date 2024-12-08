using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gravity : PhysicsComponentBase
{
    [SerializeField] float m_gravityScale = 1;
    [SerializeField] private Vector3 m_gravity = 9.81f * Vector3.down;
    [SerializeField, ReadOnly] Vector3 m_effectiveForce = Vector3.zero;
    public Vector3 GForce => m_gravity * m_gravityScale;

    public override Vector3 GetForce(Vector3 initial)
    {
        m_effectiveForce = m_gravityScale * m_body.Mass * m_gravity;
        return m_effectiveForce;
    }
    
}
