using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bounce : MonoBehaviour
{
    private float _velocity = 0.0f;

    private Vector3 _forceDirection = Vector3.zero;

    private void FixedUpdate()
    {
        _forceDirection = transform.position;
        _velocity += Physics.gravity.y * Time.fixedDeltaTime;
        _forceDirection.y += _velocity * Time.fixedDeltaTime;

        transform.position = _forceDirection;

        if (transform.position.y < -1 && _velocity < 0f)
            _velocity *= -1f;
    }
}
