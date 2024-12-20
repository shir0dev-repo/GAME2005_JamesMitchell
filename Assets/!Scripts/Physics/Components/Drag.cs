using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(IPhysicsVolume))]
public class Drag : PhysicsComponentBase
{
    public const float AIR_DENSITY = 1.225f;
    private IPhysicsVolume m_volume;

    protected override void Awake()
    {
        base.Awake();
        m_volume = GetComponent<IPhysicsVolume>();
    }

    public override Vector3 Modify(Vector3 initial)
    {
        float projectedArea = m_volume.CrossSectionalArea(initial.normalized);
        Vector3 drag = CalculateDrag(initial, m_body.Drag, projectedArea) * PhysicsManager.Instance.DeltaTime;

        initial -= drag;
        return initial;
    }

    private Vector3 CalculateDrag(Vector3 velocity, float dragCoefficient, float area)
    {
        Vector3 horizontalVelocity = velocity;
        horizontalVelocity.y = 0;
        Vector3 verticalVelocity = velocity - horizontalVelocity;

        //https://www.grc.nasa.gov/www/k-12/VirtualAero/BottleRocket/airplane/drageq.html thx nasa
        Vector3 dragXZ = HorizontalDrag(horizontalVelocity, area, dragCoefficient);

        Vector3 dragY = -VerticalDrag(verticalVelocity, m_body.Mass, area, dragCoefficient);
        return dragY + dragXZ;
    }

    private Vector3 HorizontalDrag(Vector3 horizontalVelocity, float area, float dragCoefficient)
    {
        float horizontalSpeedSqr = horizontalVelocity.sqrMagnitude;
        return dragCoefficient * area * AIR_DENSITY * (0.5f * horizontalSpeedSqr) * horizontalVelocity.normalized;
    }

    private Vector3 VerticalDrag(Vector3 verticalVelocity, float mass, float area, float dragCoefficient)
    {
        //https://dynref.engr.illinois.edu/afp.html thx UOI
        Vector3 mg = mass * PhysicsManager.Instance.Gravity;
        float c = 0.5f * dragCoefficient * AIR_DENSITY * area;
        return mg - c * verticalVelocity.sqrMagnitude * verticalVelocity.normalized;
    }
}
