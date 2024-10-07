using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsBody : MonoBehaviour
{
    [SerializeField] protected float m_mass = 1f;
    public float Mass => m_mass;

    [SerializeField, Range(0, 1.5f)] protected float m_dragCoefficient = 1f;
    public float Drag => m_dragCoefficient;

    [SerializeField, ReadOnly] protected Vector3 m_velocity = Vector3.zero;
    [SerializeField] protected PhysicsComponentBase[] physicsComponents;

    [SerializeField] protected ProductionLine<Vector3> m_velocityLine;

    protected void Awake()
    {
        m_velocityLine = new ProductionLine<Vector3>(m_velocity);
        foreach (var physicsComponent in physicsComponents)
            m_velocityLine.AppendOperation(physicsComponent.Modify);
    }

    protected virtual void Start()
    {
        PhysicsManager.AddToLoop(this);
    }

    public void Move()
    {
        m_velocity = m_velocityLine.GetResult();
        m_velocityLine.SetInitial(m_velocity);
        transform.position += m_velocity * PhysicsManager.Instance.DeltaTime;

    }

    protected virtual void OnDestroy()
    {
        PhysicsManager.RemoveFromLoop(this);
    }
}
