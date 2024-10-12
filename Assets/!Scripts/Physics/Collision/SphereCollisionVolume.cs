using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereCollisionVolume : CollisionVolume
{
    [SerializeField] private float m_radius = 0.5f;
    public override bool IsColliding(CollisionVolume other)
    {
        if (other is not SphereCollisionVolume otherAsSphere) return false;

        float distance = (otherAsSphere.transform.position - transform.position).magnitude;
        return distance < m_radius + otherAsSphere.m_radius;
    }
}
