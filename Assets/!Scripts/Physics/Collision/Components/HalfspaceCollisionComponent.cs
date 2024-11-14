using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HalfspaceCollisionComponent : CollisionComponent, ICollisionVolume
{
    public override ColliderType Type => ColliderType.Halfspace;
    public override VelocityMode VelocityMode => VelocityMode.ZeroOnImpact;
    public override bool IsKinematic => false;
    
    public PlaneAxis Axes => m_axes;
    private PlaneAxis m_axes;   


    private Vector3 m_positionLastFrame;
    private Quaternion m_rotationLastFrame;

    private void Awake()
    {
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

    /// <summary>Half spaces are infinite, and that would be baaad.</summary>
    public override float CrossSectionalArea(Vector3 normal)
    {
        return 1;
    }

    public float GetSignedDistance(Vector3 position)
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
}
