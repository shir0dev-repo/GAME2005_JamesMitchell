
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneCollisionVolume : PhysicsComponentBase, ICollisionVolume
{
    public ColliderType Type => ColliderType.Plane;
    public VelocityMode VelocityMode => m_velocityMode;
    [SerializeField] private VelocityMode m_velocityMode;

    public bool IsKinematic => false;
    public ICollisionVolume CurrentCollision { get; set; }
    public bool CurrentlyColliding { get; set; }

    public Vector3 CurrentPartitionOrigin { get; set; }

    public PlaneAxis Axes => m_axes;
    private PlaneAxis m_axes;
    public Transform Transform => transform;

    private Vector3 m_positionLastFrame;
    private Quaternion m_rotationLastFrame;

    protected override void Awake()
    {
        base.Awake();

        m_axes = new PlaneAxis(transform.up);
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
        return initial;
    }
}
