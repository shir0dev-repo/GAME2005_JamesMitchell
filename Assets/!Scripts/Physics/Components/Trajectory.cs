using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Trajectory : PhysicsComponentBase
{
    bool m_firstFrame = true;

    private Vector3 m_startingVelocity = Vector3.zero;
    public void InitParams(float initialAngleDegrees, float initialSpeed, float lifetime)
    {
        float initialAngleRadians = Mathf.Deg2Rad * initialAngleDegrees;

        m_startingVelocity.x = initialSpeed * Mathf.Cos(initialAngleRadians);
        m_startingVelocity.y = initialSpeed * Mathf.Sin(initialAngleRadians);

        Destroy(gameObject, lifetime);
    }

    public override Vector3 GetForce(Vector3 initial, Vector3 collisionDisplacement)
    {
        if (m_firstFrame)
        {
            initial = m_startingVelocity;
            m_firstFrame = false;
        }
        
        transform.forward = initial.normalized;
        return initial;
    }
}
