using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bounce : MonoBehaviour
{
    private float _velocity = 0.0f;

    private readonly float _sphereRadius = 0.5f;

    private void FixedUpdate()
    {
        _velocity += Physics.gravity.y * Time.fixedDeltaTime;

        transform.position += Time.fixedDeltaTime * _velocity * Vector3.up;

        if (transform.position.y < _sphereRadius && _velocity < 0f)
            _velocity *= -1f;
    }
}
