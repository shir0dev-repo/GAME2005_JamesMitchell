using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SineMovement : Movement
{
    [Flags]
    private enum Axis 
    {
        X = 0b0001,
        Y = 0b0010
    }

    [SerializeField] private Axis _axis;
    [SerializeField] private float _frequency = 5f;
    [SerializeField] private float _amplitude = 0.5f;

    private float _timeSinceStart = 0;
    private Vector3 position = new();



    protected override void Move()
    {
        if ((_axis & Axis.X) == Axis.X)
            position.x += (-Mathf.Sin(_timeSinceStart * _frequency)) * _frequency * _amplitude * Time.fixedDeltaTime;
        if ((_axis & Axis.Y) == Axis.Y)
            position.y += Mathf.Cos(_timeSinceStart * _frequency) * _frequency * _amplitude * Time.fixedDeltaTime;
        transform.position = position;

        _timeSinceStart += Time.fixedDeltaTime;
    }
}
