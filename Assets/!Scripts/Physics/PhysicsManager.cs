using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.LowLevel;
using System.Timers;
using Unity.VisualScripting;

public class PhysicsManager : InjectedSystem<PhysicsManager>
{
    public Vector3 Gravity => m_gravity;
    [SerializeField] private Vector3 m_gravity = Vector3.down * 9.81f;

    private readonly static List<PhysicsBody> m_actors = new List<PhysicsBody>();
    public static event EventHandler<float> OnPhysicsUpdate;

    protected override void OnPreCollisionUpdate()
    {
        foreach (PhysicsBody pb in m_actors)
            pb.Move();

        OnPhysicsUpdate?.Invoke(Instance, PhysicsBodyUpdateSystem.TimeStep);
    }

    protected override void OnUnintersectionUpdate()
    {
        foreach (PhysicsBody pb in m_actors)
            pb.Unintersect();
    }

    protected override void OnPostCollisionUpdate()
    {
        Debug.Log("adjusting velocity");

        foreach (PhysicsBody pb in m_actors)
            pb.ApplyPostCollisionVelocity();
    }

    public static void AddToLoop(PhysicsBody pb)
    {
        if (pb.SimulationMode == SimulationMode.Kinematic && !m_actors.Contains(pb))
        {
            m_actors.Add(pb);
        }
    }

    public static void RemoveFromLoop(PhysicsBody pb) { m_actors.Remove(pb); }
}
