using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bounce : Movement
{
    [SerializeField] private float _planeHeightWS = 0f;
    [SerializeField, ReadOnly] private float _velocity = 0.0f;

    private readonly float _sphereRadius = 0.5f;
    private static readonly float _acceleration = Physics.gravity.y;


    protected override void Move()
    {
        _velocity += _acceleration * Time.fixedDeltaTime;

        transform.position += Time.fixedDeltaTime * _velocity * Vector3.up;

        if (transform.position.y < _planeHeightWS + _sphereRadius)
        {
            transform.position.Set(transform.position.x, _sphereRadius + Mathf.Epsilon, transform.position.z);
            _velocity = -_velocity - _acceleration * Time.fixedDeltaTime;
        }
    }
}
