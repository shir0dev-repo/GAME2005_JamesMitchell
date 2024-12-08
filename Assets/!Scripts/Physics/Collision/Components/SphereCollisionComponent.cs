using UnityEngine;

[UnityEditor.CanEditMultipleObjects]
public class SphereCollisionComponent : CollisionComponent
{
    private const float VOLUME_COEFF = (4.0f / 3.0f) * Mathf.PI;

    public override ColliderType Type => ColliderType.Sphere;
    public override bool IsKinematic => m_isKinematic;
    [SerializeField] protected bool m_isKinematic = true;
    public override VelocityMode VelocityMode => m_velocityMode;
    [SerializeField] protected VelocityMode m_velocityMode = VelocityMode.Restitution;

    public float Radius => m_radius;
    [SerializeField] private float m_radius = 0.5f;

    /// <summary>Used for debugging the CSA and drawing it with gizmos.</summary>
    private Vector3 m_lastKnownNormal = Vector3.zero;

    public override float Volume()
    {
        return VOLUME_COEFF * m_radius;
    }

    public override float CrossSectionalArea(Vector3 inNormal)
    {
        m_lastKnownNormal = inNormal.normalized;
        return Mathf.PI * Mathf.Pow(m_radius, 2);
    }

    [ContextMenu("Update Radius")]
    public void UpdateRadius()
    {
        transform.localScale = Vector3.one * m_radius * 2f;
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        Gizmos.color = Color.green;
        
        if (m_lastKnownNormal != Vector3.zero)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, transform.position + m_lastKnownNormal);
        }
    }
}
