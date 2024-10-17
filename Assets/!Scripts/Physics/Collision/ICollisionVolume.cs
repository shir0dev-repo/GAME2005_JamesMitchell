using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICollisionVolume : IPartitionable
{
    VolumeType Type { get; }

    public bool IsColliding(ICollisionVolume other)
    {
        return other.Type switch
        {
            VolumeType.Sphere => CollideWithSphere(other as SphereCollisionVolume),
            VolumeType.Plane => CollideWithPlane(other as PlaneCollisionVolume),
            VolumeType.Halfspace => CollideWithHalfspace(other as HalfspaceCollisionVolume),
            _ => throw new UnimplementedCollisionException("Undefined collision type!")
        };
    }

    protected abstract bool CollideWithSphere(SphereCollisionVolume other);
    protected abstract bool CollideWithPlane(PlaneCollisionVolume other);
    protected abstract bool CollideWithHalfspace(HalfspaceCollisionVolume other);
}

