using System;
using UnityEngine;

public enum ColliderType
{
    Sphere,
    Plane,
    Halfspace,
    AABB,
    LENGTH
}

public static class Collisions
{
    public static Func<ICollisionVolume, ICollisionVolume, bool>[][] Interactions;

    static Collisions()
    {
        Interactions = new Func<ICollisionVolume, ICollisionVolume, bool>[(int)ColliderType.LENGTH][];
        int length = (int)ColliderType.LENGTH;
        // initialize array sizes. 
        for (int i = 0; i < length; i++)
        {
            Interactions[i] = new Func<ICollisionVolume, ICollisionVolume, bool>[length - i];
        }

        // initialize methods for each collision response.
        // this can be thought of as half of a matrix, or a triangular array.
        // the choice of this implementation allows collision interactions to only be defined once.
        // e.g., if sphere/plane is defined, there is not reason to define plane/sphere
        for (int i = 0; i < length; i++)
        {
            for (int k = 0; k < Interactions[i].Length; k++)
            {
                Interactions[i][k] = GetInteraction((ColliderType)i, (ColliderType)(i + k));
            }
        }
    }

    public static bool IsColliding(ICollisionVolume a, ICollisionVolume b)
    {
        int typeA = (int)a.Type;
        int typeB = (int)b.Type;

        // because interactions are implemented in a triangular manner,
        // passing in the larger value first results in an IndexOutOfRangeException.
        // e.g. If typeA is AABB (3) and typeB is Sphere (0), the definition of this collision is at 
        // Interactions[0][3]. Assuming 4 VolumeTypes, Interactions[3][0] is defined as the collision
        // between two VolumeTypes, both of type 4.
        if (typeB < typeA)
        {
            return Interactions[typeB][typeA].Invoke(b, a);
        }
        else
            return Interactions[typeA][typeB].Invoke(a, b);
    }

    // methods are separated into a declaration/implementation style
    // to make casting easier. Child-specific parameters such as a sphere's radius or a plane's normal
    // cannot be obtained without downcasting, which cannot be implicit.
    private static Func<ICollisionVolume, ICollisionVolume, bool> GetInteraction(ColliderType first, ColliderType second)
    {
        switch (first, second)
        {
            case (ColliderType.Sphere, ColliderType.Sphere):
                return SSCollision;
            case (ColliderType.Sphere, ColliderType.Plane):
                return SPCollision;
            case (ColliderType.Sphere, ColliderType.Halfspace):
                return SHCollision;
            case (ColliderType.Sphere, ColliderType.AABB):
                return SBCollision;

            case (ColliderType.Plane, ColliderType.Plane):
                return PPCollision;
            case (ColliderType.Plane, ColliderType.Halfspace):
                return PHCollision;
            case (ColliderType.Plane, ColliderType.AABB):
                return PBCollision;

            case (ColliderType.Halfspace, ColliderType.Halfspace):
                return HHCollision;
            case (ColliderType.Halfspace, ColliderType.AABB):
                return HBCollision;
            case (ColliderType.AABB, ColliderType.AABB):
                return BBCollision;

            default:
                throw new UnimplementedCollisionException("Collision may be implemented, try switching the order.");
        }
    }

    #region CheckImplementation
    private static bool SSCollision(ICollisionVolume a, ICollisionVolume b)
    {
        return SphereSphereCollision(a as SphereCollisionVolume, b as SphereCollisionVolume);
    }
    private static bool SphereSphereCollision(SphereCollisionVolume a, SphereCollisionVolume b)
    {
        float distance = Vector3.Distance(a.transform.position, b.transform.position);
        return distance <= a.Radius + b.Radius;
    }

    private static bool SPCollision(ICollisionVolume a, ICollisionVolume b)
    {
        return SpherePlaneCollision(a as SphereCollisionVolume, b as PlaneCollisionVolume);
    }
    private static bool SpherePlaneCollision(SphereCollisionVolume sphere, PlaneCollisionVolume plane)
    {
        return plane.GetDistance(sphere.transform.position) <= sphere.Radius;
    }

    private static bool SHCollision(ICollisionVolume a, ICollisionVolume b)
    {
        return SphereHalfspaceCollision(a as SphereCollisionVolume, b as HalfspaceCollisionVolume);
    }
    private static bool SphereHalfspaceCollision(SphereCollisionVolume sphere, HalfspaceCollisionVolume halfspace)
    {
        return halfspace.GetDistance(sphere.transform.position) - sphere.Radius <= 0;
    }

    private static bool SBCollision(ICollisionVolume a, ICollisionVolume b)
    {
        return SphereAABBCollision(a as SphereCollisionVolume, b as AABBCollisionVolume);
    }
    private static bool SphereAABBCollision(SphereCollisionVolume a, AABBCollisionVolume b)
    {
        return false;
    }

    private static bool PPCollision(ICollisionVolume a, ICollisionVolume b)
    {
        return PlanePlaneCollision(a as PlaneCollisionVolume, b as PlaneCollisionVolume);
    }
    private static bool PlanePlaneCollision(PlaneCollisionVolume a, PlaneCollisionVolume b)
    {
        return false;
    }

    private static bool PHCollision(ICollisionVolume a, ICollisionVolume b)
    {
        return PlaneHalfspaceCollision(a as PlaneCollisionVolume, b as HalfspaceCollisionVolume);
    }
    private static bool PlaneHalfspaceCollision(PlaneCollisionVolume a, HalfspaceCollisionVolume b)
    {
        return false;
    }

    private static bool PBCollision(ICollisionVolume a, ICollisionVolume b)
    {
        return PlaneAABBCollision(a as PlaneCollisionVolume, b as AABBCollisionVolume);
    }
    private static bool PlaneAABBCollision(PlaneCollisionVolume a, AABBCollisionVolume b)
    {
        return false;
    }

    private static bool HHCollision(ICollisionVolume a, ICollisionVolume b)
    {
        return HalfspaceHalfspaceCollision(a as HalfspaceCollisionVolume, b as HalfspaceCollisionVolume);
    }
    private static bool HalfspaceHalfspaceCollision(HalfspaceCollisionVolume a, HalfspaceCollisionVolume b)
    {
        return false;
    }

    private static bool HBCollision(ICollisionVolume a, ICollisionVolume b)
    {
        return HalfspaceAABBCollision(a as HalfspaceCollisionVolume, b as AABBCollisionVolume);
    }
    private static bool HalfspaceAABBCollision(HalfspaceCollisionVolume a, AABBCollisionVolume b)
    {
        return false;
    }

    private static bool BBCollision(ICollisionVolume a, ICollisionVolume b)
    {
        return AABBAABBCollision(a as AABBCollisionVolume, b as AABBCollisionVolume);
    }
    private static bool AABBAABBCollision(AABBCollisionVolume a, AABBCollisionVolume b)
    {
        return false;
    }
    #endregion

    public static Vector3 GetResponse(ref Vector3 velocity, ICollisionVolume a, ICollisionVolume b)
    {
        return SSResponse(ref velocity, a as SphereCollisionVolume, b as SphereCollisionVolume);
        Vector3 result;
        switch (a.Type, b.Type)
        {
            case (ColliderType.Sphere, ColliderType.Sphere):
                
                
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="velocity"></param>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns>
    /// The displacement vector that unintersects <see cref="SphereCollisionVolume"/> <paramref name="a"/>.<br/>
    /// If <paramref name="b"/> is also kinematic, displacement vector will have 50% magnitude to account for both volumes being displaced.
    /// </returns>
    private static Vector3 SSResponse(ref Vector3 velocity, SphereCollisionVolume a, SphereCollisionVolume b)
    {
        Vector3 currentDisplacement = a.transform.position + velocity * PhysicsManager.Instance.DeltaTime;

        Vector3 collisionPlaneNormal = (a.Center - b.Center).normalized;
        float mag = velocity.magnitude;
        velocity = Vector3.Reflect(velocity.normalized, collisionPlaneNormal) * mag;

        Debug.Log("collision");

        if (a.IsKinematic == false)
            return Vector3.zero;

        float sumRadii = a.Radius + b.Radius;
        float distance = Vector3.Distance(a.Center, b.Center);
        float intersectionDistance = Mathf.Abs(distance - sumRadii);

        Vector3 displacement = collisionPlaneNormal * intersectionDistance;
        //velocity -= displacement * currentDisplacement.magnitude;

        if (b.IsKinematic == false)
            displacement *= 0.5f;

        return displacement;
    }
}