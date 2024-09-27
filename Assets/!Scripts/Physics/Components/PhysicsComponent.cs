using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PhysicsComponent : MonoBehaviour
{
    [SerializeField] protected Vector3 m_velocity = Vector3.zero;

    public abstract Vector3 ApplyToObject(ref Vector3 initial);
}
