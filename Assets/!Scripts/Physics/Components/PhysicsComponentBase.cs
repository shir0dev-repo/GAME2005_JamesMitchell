using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PhysicsBody))]
public abstract class PhysicsComponentBase : MonoBehaviour, IPhysicsComponent
{
    protected PhysicsBody m_body;
    public abstract Vector3 GetForce(Vector3 initial, Vector3 collisionDisplacement);

    protected virtual void Awake()
    {
        m_body = GetComponent<PhysicsBody>();
    }
}
