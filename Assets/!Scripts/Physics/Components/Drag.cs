using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class Drag : PhysicsComponent
{
    [SerializeField] private PhysicsBody m_body;
    [SerializeField] private PhysicsShape m_shape;
    
    private void Awake()
    {
        m_body = GetComponent<PhysicsBody>();
        m_shape = GetComponent<PhysicsShape>();
    }

    private void Update()
    {
        CalculateDrag(m_velocity, m_body.Drag, m_shape.getBoundingBox());
    }

    public override Vector3 ApplyToObject(Vector3 initial)
    {
        initial += CalculateDrag(in m_velocity, m_body.Drag, m_shape.getBoundingBox()) * PhysicsManager.Instance.DeltaTime;
        return initial;
    }

    private Vector3 CalculateDrag(in Vector3 velocity, float dragCoefficient, PhysicsVolume boundingBox)
    {
        float Ca = boundingBox.CrossSectionalArea(velocity);
        Vector3 dragForce = (velocity.sqrMagnitude * 0.5f) * dragCoefficient * Ca * velocity.normalized;
        return dragForce;
    }
}
