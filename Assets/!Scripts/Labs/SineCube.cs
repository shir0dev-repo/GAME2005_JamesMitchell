using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SineCube : Physibody
{
    [SerializeField] private float m_frequency = 2, m_amplitude = 0.5f;

    [SerializeField, ReadOnly] private float m_timeElapsed = 0;

    protected override void FixedUpdate()
    {
        Vector3 newPos = new Vector3();
        newPos.x = transform.position.x + (-Mathf.Sin(m_timeElapsed * m_frequency)) * m_frequency * m_amplitude * PhysiManager.DeltaTime;
        newPos.y = transform.position.y + Mathf.Cos(m_timeElapsed * m_frequency) * m_frequency * m_amplitude * PhysiManager.DeltaTime;
        SetVelocity(newPos);

        transform.position = m_velocity;

        m_timeElapsed += PhysiManager.DeltaTime;
    }
}
