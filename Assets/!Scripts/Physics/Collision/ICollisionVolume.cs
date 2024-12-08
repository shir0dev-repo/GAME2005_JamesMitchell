using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICollisionVolume : IPartitionable
{
    ColliderType Type { get; }
    VelocityMode VelocityMode { get; }

    Vector3 TheoreticalPosition { get; set; }

    float SkinWidth { get; }
    bool IsKinematic { get; }
    
    bool IsColliding(ICollisionVolume other, CollisionData colData)
    {
        return Collisions.IsColliding(this, other, colData);
    }

    Vector3 GetCollisionResponse(CollisionData colData)
    {
        return Collisions.GetResponse(colData);
    }
}

