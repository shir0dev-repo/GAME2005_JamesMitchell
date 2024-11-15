using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drag : PhysicsComponentBase
{
    public const float AIR_DENSITY = 1.225f;
    
    public float Coefficient => m_coefficient;
    [SerializeField, Range(0.5f, 1.5f)] private float m_coefficient = 0.75f;
    private CollisionComponent m_collisionComponent;
    private bool m_collisionEnabled;

    protected override void Awake()
    {
        base.Awake();
        m_collisionEnabled = TryGetComponent(out m_collisionComponent);
    }

    public override Vector3 GetForce(Vector3 initial)
    {
        float projectedArea;

        if (m_collisionEnabled)
            projectedArea = m_collisionComponent.CrossSectionalArea(initial.normalized);
        else
            projectedArea = 1;
        
        //https://www.grc.nasa.gov/www/k-12/VirtualAero/BottleRocket/airplane/drageq.html thx nasa
        float speedSqr = initial.sqrMagnitude;
        Vector3 drag = -0.5f * m_coefficient * AIR_DENSITY * projectedArea * speedSqr * initial.normalized;
        
        if (drag.sqrMagnitude == float.NaN)
            return Vector3.zero;
        else 
            return drag;
    }
}
