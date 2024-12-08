using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum SimulationMode { Static, Kinematic }

public class PhysicsBody : MonoBehaviour
{
    public SimulationMode SimulationMode => m_simulationMode;
    [SerializeField] private SimulationMode m_simulationMode = SimulationMode.Kinematic;

    public float Mass => m_mass;
    [SerializeField, Min(0.5f)] protected float m_mass = 1f;

    public Vector3 Velocity => m_velocity;
    [SerializeField, ReadOnly] private Vector3 m_velocity = Vector3.zero;
    [SerializeField, ReadOnly] private Vector3 m_positionLastFrame = Vector3.zero;
    [SerializeField, ReadOnly] private Vector3 m_summatedForces;

    protected PhysicsComponentBase[] m_physicsComponents;

    private CollisionComponent m_collisionComponent;
    private bool m_collisionsEnabled = false;

    [SerializeField] private bool m_useDebugging = false;

    private void Awake()
    {
        List<PhysicsComponentBase> comps = new List<PhysicsComponentBase>(GetComponents<PhysicsComponentBase>());
        m_physicsComponents = comps.OrderBy(pc => (int)pc.ForceApplicationMode).ToArray();

        if (m_collisionsEnabled = TryGetComponent(out m_collisionComponent))
        {
            
        }
    }

    private void Start()
    {
        PhysicsManager.AddToLoop(this);
        if (m_collisionsEnabled)
            m_mass = m_collisionComponent.Material.Density() * m_collisionComponent.Volume();
    }

    void Reset()
    {
        m_velocity = Vector3.zero;
        m_summatedForces = Vector3.zero;
    }

    public void Move()
    {
        m_positionLastFrame = transform.position;
        m_summatedForces = Vector3.zero;

        for (int i = 0; i < m_physicsComponents.Length; i++)
        {
            m_summatedForces += m_physicsComponents[i].GetForce(m_velocity);
        }

        m_velocity += (m_summatedForces / m_mass) * PhysicsBodyUpdateSystem.TimeStep;
        if (m_collisionsEnabled)
            m_collisionComponent.TheoreticalPosition = transform.position + (m_velocity * PhysicsBodyUpdateSystem.TimeStep);
    }

    public void PostCollision()
    {
        transform.position += m_velocity * PhysicsBodyUpdateSystem.TimeStep;
    }
    
    public void AddImpulse(Vector3 impulse, float otherMass)
    {
        m_summatedForces += (impulse * otherMass) / PhysicsBodyUpdateSystem.TimeStep;
        m_velocity += impulse * otherMass;
    }

    public void AddImpulseUnscaledTime(Vector3 unscaledImpulse)
    {
        m_velocity += (unscaledImpulse / m_mass) * PhysicsBodyUpdateSystem.TimeStep;
    }

    [ContextMenu("Apply Random Force")]
    public void ApplyRandomForce()
    {
        Vector3 dir = new Vector3(Random.Range(0f, 1.0f), Random.Range(0f, 1.0f), Random.Range(0f, 1.0f)).normalized;
        AddImpulse(dir, Random.Range(1f, 25f));
    }

    protected virtual void OnDestroy()
    {
        PhysicsManager.RemoveFromLoop(this);
    }

    private void OnDrawGizmos()
    {
        if (!m_useDebugging) return;
        else if (!Application.isPlaying) return;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + m_velocity);
    }
}
