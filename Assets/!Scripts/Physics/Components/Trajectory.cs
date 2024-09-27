using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Gravity))]
public class Trajectory : PhysicsComponent
{
    private Vector3 m_startingVelocity = Vector3.zero;
    private float m_timeSinceLaunch = 0;

    [SerializeField] private Gravity m_gravity;

    private void Awake()
    {
        Destroy(gameObject, 3f);
    }

    public void InitParams(float initialAngleDegrees, float initialSpeed)
    {
        float initialAngleRadians = Mathf.Deg2Rad * initialAngleDegrees;

        m_startingVelocity.x = initialSpeed * Mathf.Cos(initialAngleRadians);
        m_startingVelocity.y = initialSpeed * Mathf.Sin(initialAngleRadians);
    }

    public override Vector3 ApplyToObject(ref Vector3 initial)
    {
        initial.x = m_startingVelocity.x * m_timeSinceLaunch;
        initial.y = m_startingVelocity.y * m_timeSinceLaunch + (0.5f * m_gravity.GravityForce.y * Mathf.Pow(m_timeSinceLaunch, 2));

        m_timeSinceLaunch += PhysicsManager.Instance.DeltaTime;
        return initial;
    }
}
