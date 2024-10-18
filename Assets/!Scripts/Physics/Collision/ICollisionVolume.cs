using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICollisionVolume : IPartitionable
{
    VolumeType Type { get; }
    bool CurrentlyColliding { get; set; }
    bool IsColliding(ICollisionVolume other)
    {
        return Collisions.IsColliding(this, other);
    }
}

