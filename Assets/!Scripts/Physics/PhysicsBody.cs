using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsBody : MonoBehaviour
{
    [SerializeField] protected float m_mass = 1f;
    public float Mass => m_mass;

    [SerializeField] protected float m_dragCoefficient = 1f;
    public float Drag => m_dragCoefficient;

    [SerializeField, ReadOnly] protected Vector3 m_velocity = Vector3.zero;
    [SerializeField] private PhysicsComponent[] physicsComponents;

    private int frameCount = 0;

    protected virtual void Start()
    {
        PhysicsManager.AddToLoop(this);
    }

    public void Move()
    {
        foreach (var physicsComponent in physicsComponents)
        {
            //m_velocity += physicsComponent.ApplyToObject(m_velocity);
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
