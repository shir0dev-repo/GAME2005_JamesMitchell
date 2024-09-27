using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsManager : Singleton<PhysicsManager>
{
    [SerializeField] private float m_deltaTime = 0.02f;
    public float DeltaTime => m_deltaTime;

    [SerializeField] private Vector3 m_gravity = Vector3.down * 9.81f;

    private static List<PhysicsBody> m_actors = new List<PhysicsBody>();

    private void FixedUpdate()
    {
        foreach (PhysicsBody pb in m_actors)
            pb.Move();
    }

    public static void AddToLoop(PhysicsBody pb) 
    {
        if (!m_actors.Contains(pb))
            m_actors.Add(pb); 
    }
    public static void RemoveFromLoop(PhysicsBody pb) {  m_actors.Remove(pb); }
}
