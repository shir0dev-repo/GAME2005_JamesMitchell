using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereCollisionVolume : PhysicsComponentBase, IPhysicsVolume, ICollisionVolume
{
    public ColliderType Type => ColliderType.Sphere;
    
    public float Radius => m_radius;
    [SerializeField] private float m_radius = 0.5f;

    public bool IsKinematic { get => m_isKinematic; }
    [SerializeField] private bool m_isKinematic = true;

    public VelocityMode VelocityMode => m_velocityMode;
    [SerializeField] private VelocityMode m_velocityMode;

    public bool CurrentlyColliding { get; set; }
    public Stack<ICollisionVolume> CurrentCollisions { get => m_currentCollisions; }
    private Stack<ICollisionVolume> m_currentCollisions = new();

    public Transform Transform { get => transform; }
    public Vector3 Center { get => transform.position; }
    public Quaternion Rotation { get => transform.rotation; }

    public Vector3 CurrentPartitionOrigin { get; set; }

    private Vector3 m_lastKnownNormal = Vector3.zero;

    public float CrossSectionalArea(Vector3 inNormal)
    {
        m_lastKnownNormal = inNormal.normalized;
        return Mathf.PI * Mathf.Pow(m_radius, 2);
    }

    public override Vector3 Modify(Vector3 initial)
    {
        if (CurrentCollisions.Count == 0) return initial;

        Vector3 adjustment = Vector3.zero;
        Vector3 deltaV = initial - m_body.VelocityLastFrame;

        while (CurrentCollisions.Count > 0)
        {
            var collision = CurrentCollisions.Pop();
            adjustment += (this as ICollisionVolume).GetCollisionResponse(ref initial, collision);
        }
        transform.position += adjustment;


        return initial + deltaV;
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        Gizmos.color = Color.green;
        
        if (m_lastKnownNormal != Vector3.zero)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(Center, Center + m_lastKnownNormal);
        }
    }
}
