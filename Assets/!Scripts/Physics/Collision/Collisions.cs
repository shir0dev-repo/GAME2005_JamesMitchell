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

public enum VelocityMode { Reflect, ZeroOnImpact }

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
                return IsSphereSphereColliding;
            case (ColliderType.Sphere, ColliderType.Plane):
                return IsSpherePlaneColliding;
            case (ColliderType.Sphere, ColliderType.Halfspace):
                return IsSphereHalfSpaceColliding;
            case (ColliderType.Sphere, ColliderType.AABB):
                return IsSphereAABBColliding;

            case (ColliderType.Plane, ColliderType.Plane):
                return IsPlanePlaneColliding;
            case (ColliderType.Plane, ColliderType.Halfspace):
                return IsPlaneHalfSpaceColliding;
            case (ColliderType.Plane, ColliderType.AABB):
                return IsPlaneAABBColliding;

            case (ColliderType.Halfspace, ColliderType.Halfspace):
                return IsHalfSpaceHalfSpaceColliding;
            case (ColliderType.Halfspace, ColliderType.AABB):
                return IsHalfSpaceAABBColliding;
            case (ColliderType.AABB, ColliderType.AABB):
                return IsAABBAABBCollliding;

            default:
                throw new UnimplementedCollisionException("Collision may be implemented, try switching the order.");
        }
    }

    #region Sphere Checks
    private static bool IsSphereSphereColliding(ICollisionVolume a, ICollisionVolume b)
    {
        return IsSphereSphereColliding_Impl(a as SphereCollisionVolume, b as SphereCollisionVolume);
    }
    private static bool IsSphereSphereColliding_Impl(SphereCollisionVolume a, SphereCollisionVolume b)
    {
        float distance = Vector3.Distance(a.transform.position, b.transform.position);
        return distance <= a.Radius + b.Radius;
    }

    private static bool IsSpherePlaneColliding(ICollisionVolume a, ICollisionVolume b)
    {
        return IsSpherePlaneColliding_Impl(a as SphereCollisionVolume, b as PlaneCollisionVolume);
    }
    private static bool IsSpherePlaneColliding_Impl(SphereCollisionVolume sphere, PlaneCollisionVolume plane)
    {
        return plane.GetDistance(sphere.transform.position) <= sphere.Radius;
    }

    private static bool IsSphereHalfSpaceColliding(ICollisionVolume a, ICollisionVolume b)
    {
        return IsSphereHalfspaceColliding_Impl(a as SphereCollisionVolume, b as HalfspaceCollisionVolume);
    }
    private static bool IsSphereHalfspaceColliding_Impl(SphereCollisionVolume sphere, HalfspaceCollisionVolume halfspace)
    {
        return halfspace.GetSignedDistance(sphere.transform.position) - sphere.Radius <= 0;
    }

    private static bool IsSphereAABBColliding(ICollisionVolume a, ICollisionVolume b)
    {
        return IsSphereAABBColliding_Impl(a as SphereCollisionVolume, b as AABBCollisionVolume);
    }
    private static bool IsSphereAABBColliding_Impl(SphereCollisionVolume a, AABBCollisionVolume b)
    {
        return false;
    }
    #endregion

    #region Plane Checks
    private static bool IsPlanePlaneColliding(ICollisionVolume a, ICollisionVolume b)
    {
        return IsPlanePlaneColliding_Impl(a as PlaneCollisionVolume, b as PlaneCollisionVolume);
    }
    private static bool IsPlanePlaneColliding_Impl(PlaneCollisionVolume a, PlaneCollisionVolume b)
    {
        return Vector3.Dot(a.Axes.Normal, b.Axes.Normal) != 1.0f;
    }

    private static bool IsPlaneHalfSpaceColliding(ICollisionVolume a, ICollisionVolume b)
    {
        return IsPlaneHalfspaceColliding_Impl(a as PlaneCollisionVolume, b as HalfspaceCollisionVolume);
    }
    private static bool IsPlaneHalfspaceColliding_Impl(PlaneCollisionVolume a, HalfspaceCollisionVolume b)
    {
        if (b.IsInsideHalfspace(a.transform.position)) return true;
        else return Vector3.Dot(a.Axes.Normal, b.Axes.Normal) != 1.0f;
    }

    private static bool IsPlaneAABBColliding(ICollisionVolume a, ICollisionVolume b)
    {
        return IsPlaneAABBColliding_Impl(a as PlaneCollisionVolume, b as AABBCollisionVolume);
    }
    private static bool IsPlaneAABBColliding_Impl(PlaneCollisionVolume a, AABBCollisionVolume b)
    {
        return false;
    }
    #endregion

    #region Half Space Checks
    private static bool IsHalfSpaceHalfSpaceColliding(ICollisionVolume a, ICollisionVolume b)
    {
        return IsHalfspaceHalfspaceColliding_Impl(a as HalfspaceCollisionVolume, b as HalfspaceCollisionVolume);
    }
    private static bool IsHalfspaceHalfspaceColliding_Impl(HalfspaceCollisionVolume a, HalfspaceCollisionVolume b)
    {
        if (a.IsInsideHalfspace(b.transform.position)) return true;
        else return Vector3.Dot(a.Axes.Normal, b.Axes.Normal) != -1.0f;
    }

    private static bool IsHalfSpaceAABBColliding(ICollisionVolume a, ICollisionVolume b)
    {
        return IsHalfspaceAABBColliding_Impl(a as HalfspaceCollisionVolume, b as AABBCollisionVolume);
    }
    private static bool IsHalfspaceAABBColliding_Impl(HalfspaceCollisionVolume a, AABBCollisionVolume b)
    {
        return false;
    }
    #endregion

    #region AABB Checks
    private static bool IsAABBAABBCollliding(ICollisionVolume a, ICollisionVolume b)
    {
        return IsAABBAABBColliding_Impl(a as AABBCollisionVolume, b as AABBCollisionVolume);
    }
    private static bool IsAABBAABBColliding_Impl(AABBCollisionVolume a, AABBCollisionVolume b)
    {
        return false;
    }
    #endregion

    /// <summary>
    /// Calculates the required displacement to unintersect <see cref="ICollisionVolume"/>s <paramref name="a"/> and <paramref name="b"/>.<br/>
    /// If <paramref name="a"/>'s <see cref="VelocityMode"/> is <see cref="VelocityMode.ZeroOnImpact"/>, <paramref name="velocity"/> will 
    /// be set to <see cref="Vector3.zero"/>.<br/>
    /// If <paramref name="a"/>'s <see cref="VelocityMode"/> is <see cref="VelocityMode.Reflect"/>, <paramref name="velocity"/> will be reflected along
    /// the plane created from the collision.
    /// </summary>
    /// <param name="velocity">The current velocity (if any) of <see cref="ICollisionVolume"/> <paramref name="a"/>.</param>
    /// <param name="a">The current <see cref="ICollisionVolume"/> to check collisions with.</param>
    /// <param name="b">The opposing <see cref="ICollisionVolume"/> to check against.</param>
    /// <returns>
    /// The displacement vector that unintersects <see cref="SphereCollisionVolume"/> <paramref name="a"/>.<br/>
    /// If <paramref name="b"/> is also kinematic, displacement vector will have 50% magnitude to account for both volumes being displaced.
    /// </returns>
    public static Vector3 GetResponse(ref Vector3 velocity, ICollisionVolume a, ICollisionVolume b)
    {
        return (a.Type, b.Type) switch
        {
            (ColliderType.Sphere, ColliderType.Sphere) =>
                SphereSphereCollisionResponse(ref velocity, a as SphereCollisionVolume, b as SphereCollisionVolume),
            (ColliderType.Sphere, ColliderType.Plane) =>
                SpherePlaneCollisionResponse(ref velocity, a as SphereCollisionVolume, b as PlaneCollisionVolume),
            (ColliderType.Sphere, ColliderType.Halfspace) =>
                SphereHalfspaceCollisionResponse(ref velocity, a as SphereCollisionVolume, b as HalfspaceCollisionVolume),
            (ColliderType.Sphere, ColliderType.AABB) =>
                SphereAABBCollisionResponse(ref velocity, a as SphereCollisionVolume, b as AABBCollisionVolume),
            _ => Vector3.zero,
        };
    }

    #region Sphere Responses
    private static Vector3 SphereSphereCollisionResponse(ref Vector3 velocity, SphereCollisionVolume a, SphereCollisionVolume b)
    {
        Vector3 collisionPlaneNormal = (a.Center - b.Center).normalized;

        if (a.VelocityMode == VelocityMode.ZeroOnImpact)
        {
            velocity = Vector3.zero;
        }
        else
        {
            float mag = velocity.magnitude;
            velocity = Vector3.Reflect(velocity.normalized, collisionPlaneNormal) * mag;
        }

        Debug.Log("SS collision");

        if (a.IsKinematic == false)
            return Vector3.zero;

        float sumRadii = a.Radius + b.Radius;
        float distance = Vector3.Distance(a.Center, b.Center);
        float intersectionDistance = Mathf.Abs(distance - sumRadii);

        Vector3 displacement = collisionPlaneNormal * intersectionDistance;

        if (b.IsKinematic == true)
            displacement *= 0.5f;

        return displacement;
    }

    private static Vector3 SpherePlaneCollisionResponse(ref Vector3 velocity, SphereCollisionVolume a, PlaneCollisionVolume b)
    {
        Vector3 normal = b.Axes.Normal;

        float distance = b.GetDistance(a.Center);
        Vector3 displacement = Vector3.Project(a.Center - b.transform.position, normal) * (a.Radius - distance);

        if (a.VelocityMode == VelocityMode.ZeroOnImpact)
        {
            velocity = Vector3.zero;
        }
        else
        {
            velocity = Vector3.Reflect(velocity, normal);
        }

        
        return displacement;
    }

    private static Vector3 SphereHalfspaceCollisionResponse(ref Vector3 velocity, SphereCollisionVolume a, HalfspaceCollisionVolume b)
    {
        Vector3 normal = b.Axes.Normal;

        if (a.VelocityMode == VelocityMode.ZeroOnImpact)
        {
            velocity = Vector3.zero;
        }
        else
        {
            float mag = velocity.magnitude;
            velocity = Vector3.Reflect(velocity.normalized, normal) * mag;
        }

        Debug.Log("SHS collision");

        float displacement = a.Radius - b.GetSignedDistance(a.Center);

        return normal * displacement;
    }

    private static Vector3 SphereAABBCollisionResponse(ref Vector3 velocity, SphereCollisionVolume a, AABBCollisionVolume b)
    {
        return Vector3.zero;
    }
    #endregion

}