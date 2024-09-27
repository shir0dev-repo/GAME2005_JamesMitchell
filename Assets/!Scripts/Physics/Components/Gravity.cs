using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gravity : PhysicsComponent
{
    [SerializeField] float m_gravityScale = 1;

    public Vector3 GravityForce => m_velocity * m_gravityScale;

    public override Vector3 ApplyToObject(ref Vector3 initial)
    {
        initial += m_gravityScale * PhysicsManager.Instance.DeltaTime * m_velocity;
        return initial;
    }
}
