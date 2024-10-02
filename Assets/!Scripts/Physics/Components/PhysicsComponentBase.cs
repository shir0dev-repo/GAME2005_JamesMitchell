using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PhysicsComponentBase : MonoBehaviour, IPhysicsComponent
{
    public abstract Vector3 Modify(Vector3 initial);
}
