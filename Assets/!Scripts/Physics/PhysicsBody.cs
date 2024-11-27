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

    public bool SimulateFriction => m_simulateFriction;
    [SerializeField] private bool m_simulateFriction = true;
    public float KineticFriction => m_kineticFriction;
    [SerializeField, Range(0, 1)] private float m_kineticFriction = 0.5f;
    public float StaticFriction => m_staticFriction;
    [SerializeField, Range(0, 1)] private float m_staticFriction = 0.5f;

    [SerializeField, ReadOnly] private Vector3 m_velocity = Vector3.zero;
    [SerializeField, ReadOnly] protected PhysicsComponentBase[] physicsComponents;
    private float m_sleepValue = 0.15f;

    private CollisionComponent m_collisionComponent;
    private bool m_collisionsEnabled = false;

    private Vector3 m_positionBeforeUnintersection = Vector3.zero;

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
        Vector3 netForce = Vector3.zero;

        foreach (var physicsComponent in physicsComponents)
        {
            netForce += physicsComponent.GetForce(m_velocity, m_collisionComponent.DisplacementThisFrame);
        }

        m_velocity += (netForce / m_mass) * PhysicsBodyUpdateSystem.TimeStep;
        transform.position += m_velocity;
    }

    public void Unintersect()
    {
        if (m_collisionsEnabled)
        {
            Vector3 resultingVelocity = m_collisionComponent.ResolveCollisions(m_velocity, out Vector3 resolvedPosition);
            transform.position = resolvedPosition;
            Debug.Log("set position");
            m_velocity = resultingVelocity;
        }
    }

    public void OverrideVelocity(Vector3 velocity) => m_velocity = velocity;

    protected virtual void OnDestroy()
    {
        PhysicsManager.RemoveFromLoop(this);
    }
}
