using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Movement : MonoBehaviour
{

    protected abstract void Move();

    protected virtual void FixedUpdate()
    {
        Move();
    }
}