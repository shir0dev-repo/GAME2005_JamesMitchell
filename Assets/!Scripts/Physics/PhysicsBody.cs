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

    public float Roughness() => m_material.KineticFrictionCoefficient;
    public float StaticFriction() => m_material.StaticFrictionThreshold;
    public float Restitution() => m_material.Restitution;

    public Vector3 Velocity => m_velocity;
    [SerializeField, ReadOnly] private Vector3 m_velocity = Vector3.zero;
    [SerializeField, ReadOnly] private Vector3 m_positionLastFrame = Vector3.zero;
    [SerializeField, ReadOnly] private Vector3 m_summatedForces;

    private Friction m_friction;

    protected PhysicsComponentBase[] m_physicsComponents;

    [SerializeField] private PhysicsMaterial m_material;

    private CollisionComponent m_collisionComponent;
    private bool m_collisionsEnabled = false;

    private void Awake()
    {
        List<PhysicsComponentBase> comps = new List<PhysicsComponentBase>(GetComponents<PhysicsComponentBase>());
        m_physicsComponents = comps.OrderBy(pc => (int) pc.ForceApplicationMode).ToArray();

        m_collisionsEnabled = TryGetComponent(out m_collisionComponent);
        m_friction = GetComponent<Friction>();
    }

    private void Start()
    {
        PhysicsManager.AddToLoop(this);
    }

    public void Move()
    {
        Debug.Log("moving");
        
        m_positionLastFrame = transform.position;
        m_summatedForces = Vector3.zero;
        
        for (int i = 0; i < m_physicsComponents.Length; i++)
        {
            m_summatedForces += m_physicsComponents[i].GetForce(m_velocity, m_collisionComponent.DisplacementThisFrame);
        }

        Vector3 acceleration = m_summatedForces / m_mass;
        m_velocity += acceleration * PhysicsBodyUpdateSystem.TimeStep;
        transform.position += m_velocity * PhysicsBodyUpdateSystem.TimeStep;
    }

    public void Unintersect()
    {
        if (m_collisionsEnabled)
        {
            Debug.Log("unintersecting");
            
            transform.position = m_collisionComponent.ResolveCollisions(transform.position, m_velocity, out m_velocity);
            /*Vector3 friction = m_friction.GetForce(m_velocity, Vector3.zero) * PhysicsBodyUpdateSystem.TimeStep;
            m_velocity += friction;
            transform.position += friction * PhysicsBodyUpdateSystem.TimeStep;
            if (m_velocity.magnitude < 0.001f)
                m_velocity = Vector3.zero;*/
        }
    }

    public void ApplyPostCollisionVelocity()
    {
        
    }

    public void OverrideVelocity(Vector3 velocity) => m_velocity = velocity;

    protected virtual void OnDestroy()
    {
        PhysicsManager.RemoveFromLoop(this);
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        Gizmos.color = Color.red;

        Gizmos.DrawLine(transform.position, transform.position + m_velocity.normalized);
    }
}
