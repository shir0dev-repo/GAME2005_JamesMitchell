using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(Gravity), typeof(PhysicsShape))]
public class Drag : PhysicsComponentBase
{
    [SerializeField] private PhysicsShape m_shape;
    [SerializeField] private Gravity m_gravity;

    protected override void Awake()
    {
        base.Awake();
        m_shape = GetComponent<PhysicsShape>();
        m_gravity = GetComponent<Gravity>();
    }

    public override Vector3 Modify(Vector3 initial)
    {
        initial += CalculateDrag(initial, m_body.Drag, m_shape.getBoundingBox()) * PhysicsManager.Instance.DeltaTime;
        return initial;
    }

    private Vector3 CalculateDrag(Vector3 velocity, float dragCoefficient, PhysicsVolume boundingBox)
    {
        Vector3 horizontalVelocity = velocity;
        horizontalVelocity.y = 0;

        float area = boundingBox.CrossSectionalArea(velocity);
        Vector3 dragForce = m_body.Mass * m_gravity.GravityScale * m_gravity.GravityDirection;
        dragForce -= dragCoefficient * (horizontalVelocity.sqrMagnitude * 0.25f) * area * velocity.normalized;

        Debug.Log(-dragForce);
        return dragForce;
    }
}
