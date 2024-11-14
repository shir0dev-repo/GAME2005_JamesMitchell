using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsBody : MonoBehaviour
{
    [SerializeField] protected float m_mass = 1f;
    public float Mass => m_mass;

    [SerializeField, Range(0.5f, 1000f)] protected float m_mass = 1f;

    public float Drag => m_dragCoefficient;
    [SerializeField, Range(0.01f, 1.5f)] protected float m_dragCoefficient = 1f;
>>>>>>> Stashed changes

    [SerializeField, ReadOnly] protected Vector3 m_velocity = Vector3.zero;
    [SerializeField] protected PhysicsComponentBase[] physicsComponents;
    
    private CollisionComponent m_collisionComponent;
    private bool m_collisionsEnabled = false;

<<<<<<< Updated upstream
    [SerializeField] protected ProductionLine<Vector3> m_velocityLine;

    protected void Awake()
=======
    private void Awake()
>>>>>>> Stashed changes
    {
        m_collisionsEnabled = TryGetComponent(out m_collisionComponent);
    }

    private void Start()
    {
        PhysicsManager.AddToLoop(this);
    }

    public void Move()
    {
<<<<<<< Updated upstream
        m_velocity = m_velocityLine.GetResult();
        m_velocityLine.SetInitial(m_velocity);
        transform.position += m_velocity * PhysicsManager.Instance.DeltaTime;
=======
        if (m_simulationMode == SimulationMode.Static) return;

        if (m_collisionsEnabled)
            m_collisionComponent.ResolveCollisionsDirect(ref m_velocity);

        Vector3 netForce = Vector3.zero;
        foreach (var physicsComponent in physicsComponents)
        {
            netForce += physicsComponent.GetForce(m_velocity);
        }

        m_velocityLastFrame = m_velocity;
        m_velocity += netForce / m_mass * PhysicsManager.Instance.DeltaTime;
>>>>>>> Stashed changes

        transform.position += m_velocity;
    }

<<<<<<< Updated upstream
=======
    public void OverrideVelocity(Vector3 velocity) => m_velocity = velocity;
    

>>>>>>> Stashed changes
    protected virtual void OnDestroy()
    {
        PhysicsManager.RemoveFromLoop(this);
    }
}
