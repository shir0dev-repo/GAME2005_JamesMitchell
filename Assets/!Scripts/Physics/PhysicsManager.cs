using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.LowLevel;
using System.Timers;

public class PhysicsManager : Singleton<PhysicsManager>
{
    public static void MarkForUpdate() => m_shouldUpdate = true;
    private static bool m_shouldUpdate;
    
    public float DeltaTime => PhysicsBodyUpdateSystem.TimeStep;

    [SerializeField] private Vector3 m_gravity = Vector3.down * 9.81f;
    public Vector3 Gravity => m_gravity;

    private readonly static List<PhysicsBody> m_actors = new List<PhysicsBody>();
    public static event EventHandler<float> OnPhysicsUpdate;
    public static event EventHandler<PhysicsBody> OnObjectAdded;

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
        if (!m_actors.Contains(pb))
        {
            m_actors.Add(pb);
            OnObjectAdded?.Invoke(Instance, pb);
        }
    }
    public static void RemoveFromLoop(PhysicsBody pb) { m_actors.Remove(pb); }

    protected override void OnApplicationQuit()
    {
        PhysicsBodyUpdateSystem.OnMarkForUpdate -= MarkForUpdate;
        base.OnApplicationQuit();
    }
}
