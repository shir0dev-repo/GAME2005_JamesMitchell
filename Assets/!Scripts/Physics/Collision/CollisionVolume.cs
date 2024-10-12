using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CollisionVolume : MonoBehaviour
{
    public abstract bool IsColliding(CollisionVolume other);

}
