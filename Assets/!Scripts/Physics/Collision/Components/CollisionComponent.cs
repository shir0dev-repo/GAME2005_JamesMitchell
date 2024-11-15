using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PhysicsBody))]
[DisallowMultipleComponent]
public abstract class CollisionComponent : MonoBehaviour, ICollisionVolume, IPhysicsVolume
{
    public abstract ColliderType Type { get; }
    public abstract VelocityMode VelocityMode { get; }
    public abstract bool IsKinematic {  get; }
    
    public bool CurrentlyColliding { get; set; }
    public Stack<ICollisionVolume> CurrentCollisions => m_currentCollisions;
    protected Stack<ICollisionVolume> m_currentCollisions = new();

    public Transform Transform => transform;
    public Vector3 Center => transform.position;
    public Quaternion Rotation => transform.rotation;
    
    public Vector3 CurrentPartitionOrigin { get; set; }

    public abstract float CrossSectionalArea(Vector3 normal);

    public virtual void Start()
    {
        CollisionManager.AddToSimulation(this);
    }

    public void ResolveCollisionsDirect(ref Vector3 resultantVelocity)
    {
        while (CurrentCollisions.Count > 0)
        {
            transform.position += (this as ICollisionVolume).GetCollisionResponse(ref resultantVelocity, CurrentCollisions.Pop());
        }
    }

    private void OnDestroy()
    {
        CollisionManager.RemoveFromSimulation(this);
    }
}
