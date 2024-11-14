using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleDrag : PhysicsComponentBase
{
    public override Vector3 GetForce(Vector3 initial)
    {
        return initial + PhysicsManager.Instance.Gravity / m_body.Drag * PhysicsManager.Instance.DeltaTime;
    }
}
