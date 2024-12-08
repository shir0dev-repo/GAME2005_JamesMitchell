using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(PhysicsBody))]
[DisallowMultipleComponent]
public abstract class CollisionComponent : MonoBehaviour, ICollisionVolume
{
    public abstract ColliderType Type { get; }
    public abstract VelocityMode VelocityMode { get; }
    public abstract bool IsKinematic { get; }

    public float SkinWidth => m_skinWidth;
    [SerializeField] protected float m_skinWidth = 0.03f;

    public bool CurrentlyColliding { get; set; }

    public Transform Transform => transform;

    public Vector3 TheoreticalPosition { get; set; }
    public Vector3 CurrentPartitionOrigin { get; set; }

    public PhysicsMaterial Material => m_material;
    [SerializeField] protected PhysicsMaterial m_material;

    [SerializeField, ReadOnly] protected Vector3 m_displacementThisFrame = Vector3.zero;

    public abstract float Volume();

    public void SetMaterial(PhysicsMaterial newMaterial)
    {
        m_material = newMaterial;
        if (TryGetComponent(out MeshRenderer rnd))
        {
            rnd.sharedMaterial = newMaterial.RenderMaterial();
        }
    }

    public PhysicsBody GetBody() { return m_body; }
    protected PhysicsBody m_body;

    public abstract float CrossSectionalArea(Vector3 normal);

    protected virtual void Awake()
    {
        m_body = GetComponent<PhysicsBody>();
    }

    public virtual void Start()
    {
        CollisionManager.AddToSimulation(this);
    }

    protected virtual void ApplyFriction(ref Vector3 resultantVelocity, ICollisionVolume other, Vector3 collisionAdjustment)
    {
        Vector3 collisionNormal = collisionAdjustment.normalized;
        float t = Vector3.Dot(resultantVelocity, collisionNormal);
        Vector3 m2 = collisionNormal * t;
        Vector3 frictionDirection = -1 * (resultantVelocity - m2).normalized;
        float frictionMagnitude = Vector3.Dot(resultantVelocity, frictionDirection);

        resultantVelocity -= frictionDirection * frictionMagnitude;
    }

    private void OnDestroy()
    {
        CollisionManager.RemoveFromSimulation(this);
    }
}
