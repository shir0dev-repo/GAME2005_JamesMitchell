using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(PhysicsBody))]
[DisallowMultipleComponent]
public abstract class CollisionComponent : MonoBehaviour, ICollisionVolume, IPhysicsVolume
{
    public abstract ColliderType Type { get; }
    public abstract VelocityMode VelocityMode { get; }
    public abstract bool IsKinematic { get; }

    public float SkinWidth => m_skinWidth;
    [SerializeField] protected float m_skinWidth = 0.03f;

    public bool CurrentlyColliding { get; set; }
    public List<CollisionData> CurrentCollisions => m_currentCollisions;
    [SerializeField] protected List<CollisionData> m_currentCollisions = new();
    public Stack<CollisionData> CollisionsLastFrame => m_collisionsLastFrame;
    protected Stack<CollisionData> m_collisionsLastFrame = new();

    public Transform Transform => transform;
    public Vector3 Center => transform.position;
    public Quaternion Rotation => transform.rotation;

    public Vector3 CurrentPartitionOrigin { get; set; }

    public Vector3 DisplacementThisFrame => m_displacementThisFrame;
    [SerializeField] protected Vector3 m_displacementThisFrame = Vector3.zero;

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

    public Vector3 ResolveCollisions(Vector3 position, Vector3 velocity, out Vector3 resolvedVelocity)
    {
        m_displacementThisFrame = Vector3.zero;

        Vector3 initialVelocity = velocity;
        Vector3 resolvedPosition = position;

        if (CurrentCollisions.Count <= 0)
        {
            resolvedVelocity = velocity;
            return position;
        }

        resolvedVelocity = initialVelocity;

        CollisionData colData;
        for (int i = 0; i < CurrentCollisions.Count; i++)
        {
            colData = CurrentCollisions[i];
            
            Vector3 displacement = (this as ICollisionVolume).GetCollisionResponse(ref resolvedVelocity, ref resolvedPosition, colData);

            m_displacementThisFrame += displacement;
        }

        return resolvedPosition;
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
