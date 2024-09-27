using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SineMovement : MonoBehaviour
{
    [SerializeField, ReadOnly] private float m_timeElapsed = 0;
    [SerializeField] private float m_deltaTime = 0.02f;
    [SerializeField] private float m_frequency = 2, m_amplitude = 1f;

    public void FixedUpdate()
    {
        Move();
        m_timeElapsed += m_deltaTime;
    }

    private void Move()
    {
        Vector3 position = new Vector3();
        position.x = transform.position.x + (-Mathf.Sin(m_timeElapsed * m_frequency)) * m_frequency * m_amplitude * m_deltaTime;
        position.y = transform.position.y + Mathf.Cos(m_timeElapsed * m_frequency) * m_frequency * m_amplitude * m_deltaTime;

        transform.position = position;

    }
}
