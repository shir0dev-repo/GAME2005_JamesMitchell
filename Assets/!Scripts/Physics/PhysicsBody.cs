using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SimulationMode { Static, Kinematic }

public class PhysicsBody : MonoBehaviour
{
    public SimulationMode SimulationMode => m_simulationMode;
    [SerializeField] private SimulationMode m_simulationMode = SimulationMode.Kinematic;

    public float Mass => m_mass;
    [SerializeField, Min(0.5f)] protected float m_mass = 1f;

    [SerializeField, ReadOnly] private Vector3 m_velocity = Vector3.zero;
    [SerializeField, ReadOnly] protected PhysicsComponentBase[] physicsComponents;
    
    private CollisionComponent m_collisionComponent;
    private bool m_collisionsEnabled = false;

    private void Awake()
    {
        physicsComponents = GetComponents<PhysicsComponentBase>();

        m_collisionsEnabled = TryGetComponent(out m_collisionComponent);
    }

    private void Start()
    {
        PhysicsManager.AddToLoop(this);
    }

    public void Move()
    {
        if (m_collisionsEnabled)
            m_collisionComponent.ResolveCollisionsDirect(ref m_velocity);

        Vector3 netForce = Vector3.zero;

        foreach (var physicsComponent in physicsComponents)
        {
            netForce += physicsComponent.GetForce(m_velocity);
        }
        
        m_velocity += netForce / m_mass * PhysicsBodyUpdateSystem.TimeStep;

        transform.position += m_velocity;
    }

    public void OverrideVelocity(Vector3 velocity) => m_velocity = velocity;
    
    protected virtual void OnDestroy()
    {
        PhysicsManager.RemoveFromLoop(this);
    }
}
