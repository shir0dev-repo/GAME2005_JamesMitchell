using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICollisionVolume : IPartitionable
{
    ColliderType Type { get; }
    bool CurrentlyColliding { get; set; }
    Stack<ICollisionVolume> CurrentCollisions { get; }

    bool IsColliding(ICollisionVolume other)
    {
        return Collisions.IsColliding(this, other);    
    }

    Vector3 GetResponseVector(ICollisionVolume other)
    {
        return Collisions.GetResponse(this, other);
    }
}

