using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereCollisionVolume : PhysicsComponentBase, ICollisionVolume
{
    [SerializeField] private float m_radius = 0.5f;

    public VolumeType Type => VolumeType.Sphere;

    public Vector3 CurrentPartitionOrigin { get; set; }
    public Transform Transform => transform;

    bool ICollisionVolume.CollideWithSphere(SphereCollisionVolume other)
    {
        if (other is not SphereCollisionVolume otherAsSphere) return false;

        float distance = (otherAsSphere.transform.position - transform.position).magnitude;
        return distance < m_radius + otherAsSphere.m_radius;
    }

    bool ICollisionVolume.CollideWithPlane(PlaneCollisionVolume other)
    {
        throw new UnimplementedCollisionException();
    }

    bool ICollisionVolume.CollideWithHalfspace(HalfspaceCollisionVolume other)
    {
        throw new UnimplementedCollisionException();
    }

    public override Vector3 Modify(Vector3 initial)
    {
        /*
        The plan is to use the existing system to allow for the collision object to "react" to collisions,
        and essentially use the last point in the production line to "push out" from the collision object.
        For now, just return the initial velocity at this point to avoid modifying it.
        */

        return initial;
    }
}
