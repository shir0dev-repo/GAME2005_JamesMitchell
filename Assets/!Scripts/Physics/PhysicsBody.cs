using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SimulationMode { Static, Kinematic }

public class PhysicsBody : MonoBehaviour
{
    public SimulationMode SimulationMode => m_simulationMode;
    [SerializeField] private SimulationMode m_simulationMode = SimulationMode.Kinematic;

    public float Mass => m_mass;
    [SerializeField, Range(0.5f, 1000f)] protected float m_mass = 1f;

    public float Drag => m_dragCoefficient;
    [SerializeField, Range(0.01f, 1.5f)] protected float m_dragCoefficient = 1f;

    [SerializeField, ReadOnly] protected Vector3 m_velocity = Vector3.zero;
    [SerializeField] protected PhysicsComponentBase[] physicsComponents;
    
    private CollisionComponent m_collisionComponent;
    private bool m_collisionsEnabled = false;

    [SerializeField] protected ProductionLine<Vector3> m_velocityLine;

    private void Awake()
    {
        m_collisionsEnabled = TryGetComponent(out m_collisionComponent);
    }

    private void Start()
    {
        PhysicsManager.AddToLoop(this);
    }

    public void Move()
    {
        if (m_simulationMode == SimulationMode.Static) return;

        if (m_collisionsEnabled)
            m_collisionComponent.ResolveCollisionsDirect(ref m_velocity);

        Vector3 netForce = Vector3.zero;
        foreach (var physicsComponent in physicsComponents)
        {
            netForce += physicsComponent.GetForce(m_velocity);
        }

        m_velocity += netForce / m_mass * PhysicsManager.Instance.DeltaTime;

        transform.position += m_velocity;
    }

    public void OverrideVelocity(Vector3 velocity) => m_velocity = velocity;
    
    protected virtual void OnDestroy()
    {
        PhysicsManager.RemoveFromLoop(this);
    }
}
