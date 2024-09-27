using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsBody : MonoBehaviour
{
    [SerializeField] protected float m_mass = 1f;
    [SerializeField] protected float m_dragCoefficient = 1f;
    [SerializeField, ReadOnly] protected Vector3 m_velocity = Vector3.zero;
    [SerializeField] private PhysicsComponent[] physicsComponents;

    protected virtual void Start()
    {
        PhysicsManager.AddToLoop(this);
    }

    public void Move()
    {
        foreach (var physicsComponent in physicsComponents)
        {
            physicsComponent.ApplyToObject(ref m_velocity);
        }

        transform.position += m_velocity * PhysicsManager.Instance.DeltaTime;

    }

    protected void Accelerate(Vector3 acceleration)
    {

    }

    protected virtual void OnDestroy()
    {
        PhysicsManager.RemoveFromLoop(this);
    }
}
