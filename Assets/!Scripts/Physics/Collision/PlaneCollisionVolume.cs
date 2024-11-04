
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneCollisionVolume : PhysicsComponentBase, ICollisionVolume
{
    public ColliderType Type => ColliderType.Plane;

    public bool IsKinematic { get; private set; }
    public bool CurrentlyColliding { get; set; }
    public ICollisionVolume CurrentCollision { get; set; }
    public Vector3 CurrentPartitionOrigin { get; set; }

    public Transform Transform => transform;

    private Vector3 m_positionLastFrame;
    private Quaternion m_rotationLastFrame;

    private PlaneAxis m_axes = new(Vector3.up);

    protected override void Awake()
    {
        base.Awake();
        m_positionLastFrame = transform.position;
        m_rotationLastFrame = transform.rotation;
    }

    private void FixedUpdate()
    {
        if (transform.position != m_positionLastFrame || transform.rotation != m_rotationLastFrame)
        {
            m_axes.Recalculate(transform);
            m_positionLastFrame = transform.position;
            m_rotationLastFrame = transform.rotation;
        }
    }

    public float GetDistance(Vector3 position)
    {

        return
            Mathf.Abs(
            m_axes.Normal.x * position.x +
            m_axes.Normal.y * position.y +
            m_axes.Normal.z * position.z -
            m_axes.DistanceFromOrigin);
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
