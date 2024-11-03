using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereVolume : PhysicsComponentBase, IPhysicsVolume, ICollisionVolume
{
    #region IPhysicsVolume
    public Vector3 Center { get => transform.position; }
    public Quaternion Rotation { get => transform.rotation; }
    public bool CurrentlyColliding { get; set; }
    public Vector3 CurrentPartitionOrigin { get; set; }
#endregion

    #region ICollisionVolume
    public Transform Transform { get => transform; }
    private readonly Stack<ICollisionVolume> m_currentCollisions = new();
    public Stack<ICollisionVolume> CurrentCollisions { get => m_currentCollisions; }
#endregion

    [SerializeField] private float m_radius = 0.5f;
    public float Radius => m_radius;

    public ColliderType Type => ColliderType.Sphere;

    private Vector3 m_lastKnownNormal = Vector3.zero;

    public float CrossSectionalArea(Vector3 inNormal)
    {
        m_lastKnownNormal = inNormal.normalized;
        return Mathf.PI * Mathf.Pow(m_radius, 2);
    }


    public override Vector3 Modify(Vector3 initial)
    {
        Vector3 result = Vector3.zero;

        // check each collision and adjust position
        while (m_currentCollisions.Count > 0)
        {
            ICollisionVolume current = m_currentCollisions.Pop();
            
        }

        return result;
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
