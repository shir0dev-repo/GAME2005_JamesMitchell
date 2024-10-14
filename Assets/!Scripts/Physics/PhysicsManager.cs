using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsManager : Singleton<PhysicsManager>
{
    [SerializeField] private float m_deltaTime = 0.02f;
    public float DeltaTime => m_deltaTime;
    [SerializeField, ReadOnly] private float m_timeSinceStartup = 0;

    [SerializeField] private Vector3 m_gravity = Vector3.down * 9.81f;
    public Vector3 Gravity => m_gravity;

    private readonly static List<PhysicsBody> m_actors = new List<PhysicsBody>();
    public static event EventHandler<float> OnPhysicsUpdate;
    public static event EventHandler<PhysicsBody> OnObjectAdded;

    private void Update()
    {
        m_timeSinceStartup += Time.deltaTime;
    }

    private void FixedUpdate()
    {
        foreach (PhysicsBody pb in m_actors)
            pb.Move();
        OnPhysicsUpdate?.Invoke(this, m_deltaTime);
    }

    public static void AddToLoop(PhysicsBody pb) 
    {
        if (!m_actors.Contains(pb))
        {
            m_actors.Add(pb); 
            OnObjectAdded?.Invoke(Instance, pb);
        }
    }
    public static void RemoveFromLoop(PhysicsBody pb) {  m_actors.Remove(pb); }
}
