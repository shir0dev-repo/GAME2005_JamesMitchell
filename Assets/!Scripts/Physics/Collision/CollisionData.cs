using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CollisionData 
{
    public CollisionComponent Focused;
    public CollisionComponent Other;

    public Vector3 RelativeVelocity;
    public Vector3 ContactPoint;
    public Vector3 CollisionNormal;

    public float PenetrationDepth;
    public float TimeSinceCollisionStart;
    public bool IsResolved;

    public CollisionData()
    {
        Focused = Other = null;
        
        RelativeVelocity = ContactPoint = CollisionNormal = Vector3.zero;
        
        PenetrationDepth = TimeSinceCollisionStart = 0;
        IsResolved = true;
    }

    public void Update(
        CollisionComponent focused, CollisionComponent other,
        Vector3 collisionNormal, Vector3 relativeVelocity,
        Vector3 contactPoint, float penetrationDepth, bool isResolved)
    {
        Focused = focused;
        Other = other;
        CollisionNormal = collisionNormal;
        RelativeVelocity = relativeVelocity;
        ContactPoint = contactPoint;
        PenetrationDepth = penetrationDepth;
        IsResolved = isResolved;
    }

    [ContextMenu("Draw")]
    public void Draw()
    {
        Debug.DrawRay(ContactPoint, CollisionNormal, Color.green);
        Debug.DrawRay(ContactPoint, RelativeVelocity, Color.yellow);

    }
}