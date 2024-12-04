
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneCollisionComponent : CollisionComponent
{
    public override ColliderType Type => ColliderType.Plane;
    public override VelocityMode VelocityMode => VelocityMode.ZeroOnImpact;
    public override bool IsKinematic => false;

    public PlaneAxis Axes => m_axes;
    private PlaneAxis m_axes;
    
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
    }

    /// <summary>Planes are infinite, and that would be baaad.</summary>
    public override float CrossSectionalArea(Vector3 _) => 1;

    public float GetDistance(Vector3 position)
    {
        return
            Mathf.Abs(
            m_axes.Normal.x * position.x +
            m_axes.Normal.y * position.y +
            m_axes.Normal.z * position.z -
            m_axes.DistanceFromOrigin);
    }
}
