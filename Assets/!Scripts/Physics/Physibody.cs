using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Physibody : MonoBehaviour
{
    [SerializeField] protected float m_gravityScale = 1;
    [SerializeField] protected float m_mass = 1;
    [SerializeField] protected float m_dragCoefficient = 1;

    protected Vector3 m_velocity = Vector3.zero;

    /// <summary>
    /// Prepend all implementation-specific calculations before calling the base of this method.
    /// </summary>
    protected virtual void FixedUpdate()
    {
        
    }

    public void Accelerate(Vector3 acceleration)
    {
        m_velocity += acceleration;
    }

    public void SetVelocity(Vector3 velocity) => m_velocity = velocity; 
}
