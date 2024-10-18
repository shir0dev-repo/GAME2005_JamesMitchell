using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereCollisionVolume : PhysicsComponentBase, ICollisionVolume
{
    [SerializeField] private float m_radius = 0.5f;
    public float Radius => m_radius;

    public VolumeType Type => VolumeType.Sphere;
    private bool m_currentlyColliding = false;
    public bool CurrentlyColliding { get; set; }

    public Vector3 CurrentPartitionOrigin { get; set; }
    public Transform Transform => transform;

    private MeshRenderer m_renderer;

    public bool IsColliding { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        m_renderer = GetComponent<MeshRenderer>();
    }

    private void FixedUpdate()
    {
        Debug.Log(CurrentlyColliding);
        if (CurrentlyColliding)
        {
            m_renderer.material.color = Color.red;
        }
        else
        {
            m_renderer.material.color = Color.green;
        }
        
    }

    public override Vector3 Modify(Vector3 initial)
    {
        /*
        The plan is to use the existing system to allow for the collision object to "react" to collisions,
        and essentially use the last point in the production line to "push out" from the collision object.
        For now, just return the initial velocity at this point to avoid modifying it.
        */

        return initial;
    }
}
