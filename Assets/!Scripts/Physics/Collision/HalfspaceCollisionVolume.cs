using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HalfspaceCollisionVolume : PhysicsComponentBase, ICollisionVolume
{
    public ColliderType Type => ColliderType.Halfspace;
    

    public bool IsKinematic { get; private set; }
    public bool CurrentlyColliding { get; set; }
    public ICollisionVolume CurrentCollision { get; set; }

    public Vector3 CurrentPartitionOrigin { get; set; }
    public Transform Transform => transform;

    [SerializeField] private PlaneAxis m_axes = new PlaneAxis(Vector3.up);

    private Vector3 m_positionLastFrame;
    private Quaternion m_rotationLastFrame;

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
        Vector3 p = position - transform.position;
        return
            m_axes.Normal.x * p.x +
                m_axes.Normal.y * p.y +
                m_axes.Normal.z * p.z;
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
