using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Callbacks;
using UnityEngine;

public class PhysicsManager : Singleton<PhysicsManager>
{
    [SerializeField] private float m_deltaTime = 0.02f;
    public float DeltaTime => m_deltaTime;

    [SerializeField] private Vector3 m_gravity = Vector3.down * 9.81f;
    public Vector3 Gravity => m_gravity;

    private readonly static List<PhysicsBody> m_kinematicActors = new List<PhysicsBody>();
    private readonly static List<PhysicsBody> m_staticActors = new List<PhysicsBody>();
    public static event EventHandler<float> OnPhysicsUpdate;
    public static event EventHandler<PhysicsBody> OnObjectAdded;

    private void FixedUpdate()
    {
        foreach (PhysicsBody pb in m_kinematicActors)
            pb.Move();

        OnPhysicsUpdate?.Invoke(this, m_deltaTime);
    }

    public static void AddToLoop(PhysicsBody pb) 
    {
        if (pb.SimulationMode == SimulationMode.Static)
        {
            if (!m_staticActors.Contains(pb))
            {
                m_staticActors.Add(pb);
                OnObjectAdded?.Invoke(Instance, pb);
            }
        }
        else if (pb.SimulationMode == SimulationMode.Kinematic)
        {
            if (!m_kinematicActors.Contains(pb))
            {
                m_kinematicActors.Add(pb);
                OnObjectAdded?.Invoke(Instance, pb);
            }
        }
    }
    public static void RemoveFromLoop(PhysicsBody pb) {  m_kinematicActors.Remove(pb); }
}
