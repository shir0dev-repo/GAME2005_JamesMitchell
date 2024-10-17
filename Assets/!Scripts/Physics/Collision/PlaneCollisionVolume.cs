
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneCollisionVolume : PhysicsComponentBase, ICollisionVolume
{
    private PlaneAxis m_planeAxes = new(Vector3.up);

    public VolumeType Type => VolumeType.Plane;

    public Vector3 CurrentPartitionOrigin { get; set; }
    public Transform Transform => transform;

    bool ICollisionVolume.CollideWithHalfspace(HalfspaceCollisionVolume other)
    {
        throw new System.NotImplementedException();
    }

    bool ICollisionVolume.CollideWithPlane(PlaneCollisionVolume other)
    {
        throw new System.NotImplementedException();
    }

    bool ICollisionVolume.CollideWithSphere(SphereCollisionVolume other)
    {
        throw new System.NotImplementedException();
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
