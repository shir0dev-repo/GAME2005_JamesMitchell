using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test : MonoBehaviour
{
    public SphereCollisionVolume a, b;

    private void FixedUpdate()
    {
        if (a.IsColliding(b))
            Debug.Log("colliding!");
    }
}
