using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Trajectory
{
    public float LaunchAngle;
    public float LaunchSpeed;
    public Vector2 InitialVelocity;
    private float _launchAngleRadians;

    public Trajectory(float launchAngle, float launchSpeed)
    {
        this.LaunchAngle = launchAngle;
        this._launchAngleRadians = Mathf.Deg2Rad * launchAngle;
        this.LaunchSpeed = launchSpeed <= 0 ? 1 : launchSpeed; // avoids dividing by zero or backwards velocities.
        this.InitialVelocity = Vector2.zero;
    }

    public void Init()
    {
        this._launchAngleRadians = Mathf.Deg2Rad * LaunchAngle;
        this.InitialVelocity = new Vector2(LaunchSpeed * Mathf.Cos(_launchAngleRadians), LaunchSpeed * Mathf.Sin(_launchAngleRadians));
    }

    public Vector3 getPositionAtPoint(float timeSinceLaunch)
    {
        float x = InitialVelocity.x * timeSinceLaunch;
        float y = InitialVelocity.y * timeSinceLaunch + (0.5f * Physics.gravity.y * Mathf.Pow(timeSinceLaunch, 2));
        return new Vector3(x, y, 0);
    }

    public Vector3[] GetTrajectoryArc(int maxSamples, float lifetime)
    {
        Vector3[] samples = new Vector3[maxSamples];

        for (int i = 0; i < maxSamples; i++)
        {
            float samplePoint = (i / (float) maxSamples) * lifetime;
            samples[i] = getPositionAtPoint(samplePoint);
        }

        return samples;
    }

    public Vector3[] GetTrajectoryArcLocal(int maxSamples, float lifetime, Transform transform)
    {
        Vector3[] samples = GetTrajectoryArc(maxSamples, lifetime);
        for (int i = 0; i < samples.Length; i++)
        {
            samples[i] += transform.position;
        }

        return samples;
    }
}
