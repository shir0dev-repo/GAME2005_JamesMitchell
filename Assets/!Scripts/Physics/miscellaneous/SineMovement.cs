using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SineMovement : MonoBehaviour
{
    [SerializeField, ReadOnly] private float m_timeElapsed = 0;
    [SerializeField] private float m_frequency = 2, m_amplitude = 1f;

    private void FixedUpdate()
    {
        Move();
    }

    public void Move()
    {
        float dt = PhysicsBodyUpdateSystem.TimeStep;

        Vector3 position = new();
        position.x = transform.position.x + (-Mathf.Sin(m_timeElapsed * m_frequency)) * m_frequency * m_amplitude * dt;
        position.y = transform.position.y + Mathf.Cos(m_timeElapsed * m_frequency) * m_frequency * m_amplitude * dt;

        transform.position = position;

        m_timeElapsed += dt;
    }
}
