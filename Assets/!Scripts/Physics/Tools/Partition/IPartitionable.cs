using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPartitionable
{
    Vector3 CurrentPartitionOrigin { get; set; }
    Transform Transform { get; }
}
