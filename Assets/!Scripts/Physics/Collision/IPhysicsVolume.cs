using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPhysicsVolume : IPartitionable 
{
    Vector3 Center { get; }
    Quaternion Rotation { get; }
}
