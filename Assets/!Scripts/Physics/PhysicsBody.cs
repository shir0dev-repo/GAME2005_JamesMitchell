using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SimulationMode { Kinematic, Static };

public class PhysicsBody : MonoBehaviour
{
    public SimulationMode SimulationMode => m_simulationMode;
    public float Mass => m_mass;
    public float Drag => m_dragCoefficient;

    [SerializeField] protected SimulationMode m_simulationMode;
    [SerializeField] protected float m_mass = 1f;
    [SerializeField, Range(0, 1.5f)] protected float m_dragCoefficient = 1f;

    [SerializeField, ReadOnly] protected Vector3 m_velocity = Vector3.zero;
    [SerializeField] protected PhysicsComponentBase[] physicsComponents;

    protected ProductionLine<Vector3> m_velocityLine;

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
        if (m_simulationMode == SimulationMode.Static) return;

        m_velocity = m_velocityLine.GetResult();
        m_velocityLine.SetInitial(m_velocity);
        transform.position += m_velocity * PhysicsManager.Instance.DeltaTime;

    }

    [ContextMenu("Add Random Force")]
    public void AddRandomForce()
    {
        Vector3 rand = new(Random.Range(0, 1f), Random.Range(0, 1f), Random.Range(0, 1f));
        rand.Normalize();
        m_velocityLine.SetInitial(rand);
    }

    protected virtual void OnDestroy()
    {
        PhysicsManager.RemoveFromLoop(this);
    }
}
