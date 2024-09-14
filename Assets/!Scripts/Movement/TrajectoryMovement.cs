using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrajectoryMovement : Movement
{
    
    [SerializeField] private GameObject _projectile;
    [SerializeField] private Trajectory _launchDetails;

    [Space]
    [SerializeField, ReadOnly] private float _timeSinceLaunch = 0.0f;
    [SerializeField] private float _lifetime = 2.0f;

    [Header("Gizmos")]
    [SerializeField] private bool _showTrajectory = false;
    [SerializeField, Range(1, 50)] private int _arcSamples = 25;

    [Header("Debugging")]
    [SerializeField] private bool _allowReflexAngles = false;
    [SerializeField] private bool _allowNegativeAngles = false;

    private Vector3[] arc;

    private void Start()
    {
        _launchDetails.Init();
    }
    
    void Update()
    {
        if (_projectile == null) return;

        if (!_projectile.activeSelf && Input.GetKeyDown(KeyCode.Space))
        {
            _projectile.SetActive(true);
            _projectile.transform.position = transform.position;
            _timeSinceLaunch = 0.0f;
        }
    }

    protected override void Move()
    {
        if (_timeSinceLaunch >= _lifetime) _projectile.SetActive(false);
        else if (_projectile.activeSelf)
        {
            _projectile.transform.localPosition = _launchDetails.getPositionAtPoint(_timeSinceLaunch);
            _timeSinceLaunch += Time.fixedDeltaTime;
        }
    }

    private void OnValidate()
    {
        if (!_allowReflexAngles && _launchDetails.LaunchAngle > 90f)
            _launchDetails.LaunchAngle = 90f;
        if (!_allowNegativeAngles && _launchDetails.LaunchAngle < 0f)
            _launchDetails.LaunchAngle = 0f;

        _launchDetails.Init();
        arc = _launchDetails.GetTrajectoryArcLocal(_arcSamples, _lifetime, transform);
    }

    private void OnDrawGizmos()
    {
        if (!_showTrajectory) return;

        Gizmos.color = Color.red;
        
        for (int i = 0; i < arc.Length - 1; i++)
            Gizmos.DrawLine(arc[i], arc[i + 1]);
    }
}
