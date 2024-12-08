using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum ApplicationMode { BeforeCollision, AfterCollision };

[RequireComponent(typeof(PhysicsBody))]
public abstract class PhysicsComponentBase : MonoBehaviour, IPhysicsComponent
{

    protected PhysicsBody m_body;
    public abstract Vector3 GetForce(Vector3 initial);
    public ApplicationMode ForceApplicationMode => m_applicationMode;
    [SerializeField] private ApplicationMode m_applicationMode = ApplicationMode.BeforeCollision;

    protected virtual void Awake()
    {
        m_body = GetComponent<PhysicsBody>();
    }
}
