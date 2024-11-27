using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CollisionData 
{
    public CollisionComponent Other;
    public Vector3 ContactPoint;
    public Vector3 CollisionNormal;
    public float PenetrationDepth;

    public CollisionData()
    {
        Other = null;
        ContactPoint = Vector3.zero;
        CollisionNormal = Vector3.zero;
        PenetrationDepth = 0;
    }

    public CollisionData(CollisionComponent other, Vector3 contactPoint, Vector3 collisionNormal, float penDepth)
    {
        Other = other;
        ContactPoint = contactPoint;
        CollisionNormal = collisionNormal;
        PenetrationDepth = penDepth;
    }

    public void Update(CollisionData other)
    {
        ContactPoint = other.ContactPoint;
        CollisionNormal = other.CollisionNormal;
        PenetrationDepth = other.PenetrationDepth;
    }
}