using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HalfspaceCollisionVolume : PhysicsComponentBase, ICollisionVolume
{
    public ColliderType Type => ColliderType.Halfspace;
    public bool IsKinematic { get; private set; }
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

        Debug.DrawLine(transform.position, transform.position + m_axes.Normal, Color.green);
        Debug.DrawLine(transform.position, transform.position + m_axes.Tangent, Color.red);
        Debug.DrawLine(transform.position, transform.position + m_axes.Bitangent, Color.blue);
    }

    public float GetSignedDistance(Vector3 position)
    {
        Vector3 p = position - transform.position;
        return
            m_axes.Normal.x * p.x +
                m_axes.Normal.y * p.y +
                m_axes.Normal.z * p.z;
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

    public bool IsInsideHalfspace(Vector3 position)
    {
        // get direction from plane origin to position
        Vector3 direction = position - transform.position;

        Debug.DrawLine(transform.position, transform.position + m_axes.Normal, Color.red);
        Debug.DrawLine(transform.position, transform.position + direction, Color.green);

        // range (0 < p <= 1) when angle is < 90°
        // range (0 == p == 0) when angle is == 90°
        // range (-1 <= p < 0) when angle is > 90°
        float nDotP = Vector3.Dot(m_axes.Normal, direction);
        
        // closed halfspace, care about points on hyperplane as well
        return nDotP <= 0;
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
