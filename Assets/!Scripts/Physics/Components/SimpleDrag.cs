using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleDrag : PhysicsComponentBase
{
    public float Coefficient => m_coefficient;
    [SerializeField, Range(0.1f, 1.5f)] private float m_coefficient = 0.75f;

    public override Vector3 GetForce(Vector3 initial)
    {
        return initial + PhysicsManager.Instance.Gravity / m_coefficient * PhysicsBodyUpdateSystem.TimeStep;
    }
}
