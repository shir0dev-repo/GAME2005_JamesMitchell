using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Gravity))]
public class Trajectory : PhysicsComponentBase
{
    private Vector3 m_startingVelocity = Vector3.zero;
    private float m_timeSinceLaunch = 0;

    [SerializeField] private Gravity m_gravity;

    public void InitParams(float initialAngleDegrees, float initialSpeed, float lifetime)
    {
        float initialAngleRadians = Mathf.Deg2Rad * initialAngleDegrees;

        m_startingVelocity.x = initialSpeed * Mathf.Cos(initialAngleRadians);
        m_startingVelocity.y = initialSpeed * Mathf.Sin(initialAngleRadians);

        Destroy(gameObject, lifetime);
    }

    public override Vector3 Modify(Vector3 initial)
    {
        if (m_timeSinceLaunch == 0)
            initial = m_startingVelocity;
        else
            initial.x += m_startingVelocity.x * m_timeSinceLaunch * PhysicsManager.Instance.DeltaTime;

        transform.forward = initial.normalized;
        m_timeSinceLaunch += PhysicsManager.Instance.DeltaTime;
        return initial;
    }
}
