using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HalfspaceCollisionVolume : PhysicsComponentBase, ICollisionVolume
{
    public ColliderType Type => ColliderType.Halfspace;
    public VelocityMode VelocityMode => VelocityMode.ZeroOnImpact;

    public bool IsKinematic => false;

    public bool CurrentlyColliding { get; set; }
    public Stack<ICollisionVolume> CurrentCollisions { get => m_currentCollisions; }
    private Stack<ICollisionVolume> m_currentCollisions = new();

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
        return Vector3.Dot(m_axes.Normal, p);
    }
    public float GetDistance(Vector3 position)
    {
        return
            Mathf.Abs(Vector3.Dot(m_axes.Normal, position - transform.position));
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
        return initial;
    }
}
