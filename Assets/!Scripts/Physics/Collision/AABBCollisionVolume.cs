using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AABBCollisionVolume : ICollisionVolume
{
    public VolumeType Type => VolumeType.AABB;
    public bool CurrentlyColliding { get; set; }
    public Vector3 CurrentPartitionOrigin { get; set; }
    public Transform Transform => throw new System.NotImplementedException();

}
