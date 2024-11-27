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
    [SerializeField] protected float m_skinWidth = 0.05f;

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

    protected PhysicsBody m_body;

    public abstract float CrossSectionalArea(Vector3 normal);

    private void Awake()
    {
        m_body = GetComponent<PhysicsBody>();
    }

    public virtual void Start()
    {
        CollisionManager.AddToSimulation(this);
    }

    public Vector3 ResolveCollisions(Vector3 velocity, out Vector3 resolvedPosition)
    {
        m_displacementThisFrame = Vector3.zero;
        Vector3 result = velocity;
        resolvedPosition = transform.position;
        CollisionData colData;
        for (int i = 0; i < CurrentCollisions.Count; i++)
        {
            colData = CurrentCollisions[i];
            // this is a new collision
            Vector3 displacement = (this as ICollisionVolume).GetCollisionResponse(ref result, ref resolvedPosition, colData);
            if (displacement.sqrMagnitude > m_skinWidth * m_skinWidth)
            {
                m_displacementThisFrame += displacement;
                resolvedPosition += displacement;
            }
        }

        return result;
    }

    public void ResolveCollisionsDirect(ref Vector3 resultantVelocity)
    {
        Vector3 initialPosition = transform.position;
        CollisionData colData;
        for (int i = 0; i < CurrentCollisions.Count; i++)
        {
             colData = CurrentCollisions[i];
            Vector3 vel = resultantVelocity;
            Vector3 displacement = (this as ICollisionVolume).GetCollisionResponse(ref vel, ref initialPosition, colData);

            m_displacementThisFrame += displacement;
            initialPosition += displacement;
            
            resultantVelocity = vel;
        }

        transform.position = initialPosition;
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
