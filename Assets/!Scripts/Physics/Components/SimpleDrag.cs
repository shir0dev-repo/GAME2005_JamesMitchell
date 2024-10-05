using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleDrag : PhysicsComponentBase
{
    public override Vector3 Modify(Vector3 initial)
    {
        return initial + PhysicsManager.Instance.Gravity / m_body.Drag * PhysicsManager.Instance.DeltaTime;
    }
}
