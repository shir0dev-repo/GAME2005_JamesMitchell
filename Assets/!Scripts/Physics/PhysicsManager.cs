using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.LowLevel;
using System.Timers;
using Unity.VisualScripting;

public class PhysicsManager : Singleton<PhysicsManager>
{
    public static void MarkForUpdate() => m_shouldUpdate = true;
    private static bool m_shouldUpdate;
    
    [SerializeField] private Vector3 m_gravity = Vector3.down * 9.81f;

    private readonly static List<PhysicsBody> m_actors = new List<PhysicsBody>();
    public static event EventHandler<float> OnPhysicsUpdate;

    private void Start()
    {
        PhysicsBodyUpdateSystem.OnMarkForUpdate += MarkForUpdate;
    }

    private static void PhysicsManagerUpdateInjected()
    {
        if (m_shouldUpdate)
        {
            m_shouldUpdate = false;
            foreach (PhysicsBody pb in m_actors)
                pb.Move();

            OnPhysicsUpdate?.Invoke(Instance, PhysicsBodyUpdateSystem.TimeStep);
        }
    }

    public static void AddToLoop(PhysicsBody pb)
    {
        if (pb.SimulationMode == SimulationMode.Kinematic && !m_actors.Contains(pb))
        {
            m_actors.Add(pb);
        }
    }

    public static void RemoveFromLoop(PhysicsBody pb) { m_actors.Remove(pb); }

    protected override void OnApplicationQuit()
    {
        PhysicsBodyUpdateSystem.OnMarkForUpdate -= MarkForUpdate;
        base.OnApplicationQuit();
    }
}
