using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CollisionComponent))]
public class Drag : PhysicsComponentBase
{
    public const float AIR_DENSITY = 1.225f;
    private CollisionComponent m_collisionComponent;

    protected override void Awake()
    {
        base.Awake();
        m_collisionComponent = GetComponent<CollisionComponent>();
    }

    public override Vector3 GetForce(Vector3 initial)
    {
        float projectedArea = m_collisionComponent.CrossSectionalArea(initial.normalized);
        
        //https://www.grc.nasa.gov/www/k-12/VirtualAero/BottleRocket/airplane/drageq.html thx nasa
        float speedSqr = initial.sqrMagnitude;
        Vector3 drag = -0.5f * m_body.Drag * AIR_DENSITY * projectedArea * speedSqr * initial.normalized;
        
        if (drag.sqrMagnitude == float.NaN)
            return Vector3.zero;
        else 
            return drag;
    }
}
