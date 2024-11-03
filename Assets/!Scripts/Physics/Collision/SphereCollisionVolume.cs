using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereCollisionVolume : PhysicsComponentBase, ICollisionVolume
{
    [SerializeField] private float m_radius = 0.5f;
    public float Radius { get => m_radius; } 

    public ColliderType Type { get => ColliderType.Sphere; }

    public bool CurrentlyColliding { get; set; }
    private readonly Stack<ICollisionVolume> m_currentCollisions = new();
    public Stack<ICollisionVolume> CurrentCollisions { get => m_currentCollisions; }

    public Vector3 CurrentPartitionOrigin { get; set; }
    public Transform Transform { get => transform; }

    public bool IsColliding { get; private set; }

    protected override void Awake()
    {
        base.Awake();
    }

    public override Vector3 Modify(Vector3 initial)
    {
        /*
        The plan is to use the existing system to allow for the collision object to "react" to collisions,
        and essentially use the last point in the production line to "push out" from the collision object.
        For now, just return the initial velocity at this point to avoid modifying it.
        */

        return initial;
    }
}
