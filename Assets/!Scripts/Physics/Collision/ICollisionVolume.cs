using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICollisionVolume : IPartitionable
{
    ColliderType Type { get; }
    VelocityMode VelocityMode { get; }

    float SkinWidth { get; }
    bool IsKinematic { get; }
    bool CurrentlyColliding { get; set; }
    List<CollisionData> CurrentCollisions { get; }
    
    bool IsColliding(ICollisionVolume other, ref CollisionData colData)
    {
        return Collisions.IsColliding(this, other, ref colData);
    }

    Vector3 GetCollisionResponse(ref Vector3 velocity, ref Vector3 position, CollisionData colData)
    {
        return Collisions.GetResponse(ref velocity, ref position, this, colData);
    }
}

