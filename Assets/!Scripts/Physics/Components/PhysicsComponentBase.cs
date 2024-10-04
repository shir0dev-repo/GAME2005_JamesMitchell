using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PhysicsBody))]
public abstract class PhysicsComponentBase : MonoBehaviour, IPhysicsComponent
{
    public abstract Vector3 Modify(Vector3 initial);
    [SerializeField] protected PhysicsBody m_body;

    protected virtual void Awake()
    {
        m_body = GetComponent<PhysicsBody>();
    }
}
