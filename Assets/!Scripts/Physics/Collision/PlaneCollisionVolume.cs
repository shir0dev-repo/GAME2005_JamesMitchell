using System.Collections.Generic;
using UnityEngine;

public class PlaneCollisionVolume : PhysicsComponentBase, ICollisionVolume
{
    public ColliderType Type => ColliderType.Plane;
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

        m_axes = new PlaneAxis(transform.up, Vector3.Distance(Vector3.zero, transform.position));
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
        return Mathf.Abs(GetSignedDistance(position));
    }

    public override Vector3 Modify(Vector3 initial)
    {
        return initial;
    }

    private Vector3 SignVector(Vector3 vector)
    {
        Vector3 sign = new();
        if (vector.x > 0) sign.x = 1;
        else if (vector.x == 0) sign.x = 0;
        else sign.x = -1;

        if (vector.y > 0) sign.y = 1;
        else if (vector.y == 0) sign.y = 0;
        else sign.y = -1;

        if (vector.z > 0) sign.z = 1;
        else if (vector.z == 0) sign.z = 0;
        else sign.z = -1;

        return sign;
    }
}
