using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PhysicsShape : MonoBehaviour
{
    public abstract PhysicsVolume getBoundingBox();
}
