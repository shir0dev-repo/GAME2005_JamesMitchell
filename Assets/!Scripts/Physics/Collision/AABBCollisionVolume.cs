using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AABBCollisionVolume : ICollisionVolume
{
    public ColliderType Type => ColliderType.AABB;
    public VelocityMode VelocityMode => m_velocityMode;
    [SerializeField] private VelocityMode m_velocityMode;

    public bool CurrentlyColliding { get; set; }
    private readonly Stack<ICollisionVolume> m_currentCollisions = new();
    public Stack<ICollisionVolume> CurrentCollisions { get => m_currentCollisions; }
    public Vector3 CurrentPartitionOrigin { get; set; }
    public Transform Transform => throw new System.NotImplementedException();

    public ICollisionVolume CurrentCollision { get; set; }

    public bool IsKinematic => false;
}
