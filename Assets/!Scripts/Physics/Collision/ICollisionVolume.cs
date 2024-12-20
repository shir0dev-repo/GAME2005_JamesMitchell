using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICollisionVolume : IPartitionable
{
    ColliderType Type { get; }
    VelocityMode VelocityMode { get; }
    Stack<ICollisionVolume> CurrentCollisions { get; }
    bool CurrentlyColliding { get; set; }
    bool IsKinematic { get; }

    bool IsColliding(ICollisionVolume other)
    {
        return Collisions.IsColliding(this, other);    
    }
    Vector3 GetCollisionResponse(ref Vector3 velocity, ICollisionVolume other)
    {
        return Collisions.GetResponse(ref velocity, this, other);
    }
}

