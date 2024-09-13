using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    [SerializeField] private bool _useGizmos = false;
    [Space]
    [SerializeField] private float _frequency = 2.0f;
    [SerializeField] private float _amplitude = 0.5f;

    private float _timeSinceStart = 0;
    private Vector3 position = new();

    private void FixedUpdate()
    {
        position.x += (-Mathf.Sin(_timeSinceStart * _frequency)) * _frequency * _amplitude * Time.fixedDeltaTime;
        position.y += Mathf.Cos(_timeSinceStart * _frequency) * _frequency * _amplitude * Time.fixedDeltaTime;
        transform.position = position;

        _timeSinceStart += Time.fixedDeltaTime;
    }

    private void OnDrawGizmosSelected()
    {
        if (!_useGizmos) return;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.right);

        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.forward);
    }
}