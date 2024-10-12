using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CollisionVolume : MonoBehaviour, IPartitionable
{
    public Vector3 CurrentPartitionOrigin { get; set; }

    public abstract bool IsColliding(CollisionVolume other);

}

