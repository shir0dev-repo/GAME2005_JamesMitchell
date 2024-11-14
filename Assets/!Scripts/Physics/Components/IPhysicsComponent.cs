using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPhysicsComponent
{
    Vector3 GetForce(Vector3 initial);
}
