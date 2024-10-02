using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gravity : PhysicsComponentBase
{
    [SerializeField] float m_gravityScale = 1;
    [SerializeField] private Vector3 m_gravity = 9.81f * Vector3.down;

    public override Vector3 Modify(Vector3 initial)
    {
        return initial + m_gravityScale * PhysicsManager.Instance.DeltaTime * m_gravity;
    }
}
