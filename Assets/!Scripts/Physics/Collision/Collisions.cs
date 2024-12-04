using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static UnityEngine.ParticleSystem;

public enum ColliderType
{
    Sphere,
    Plane,
    Halfspace,
    AABB,
    OBB,
    LENGTH
}

public enum VelocityMode { Reflect, ZeroOnImpact, Restitution }

public static class Collisions
{
    private static readonly Dictionary<ColliderType, int> _COL_BIT_MASK;

    private static readonly int _UNIMPLEMENTED_COLLISIONS;
    private delegate bool CollisionCheckDelegate(ICollisionVolume colliderA, ICollisionVolume colliderB, ref CollisionData collisionData);
    private static CollisionCheckDelegate[][] Interactions;

    static Collisions()
    {
        _COL_BIT_MASK =
        new Dictionary<ColliderType, int>()
        {
            { ColliderType.Sphere, 1 },
            { ColliderType.Plane, 2 },
            { ColliderType.Halfspace, 4 },
            { ColliderType.AABB, 8 },
            { ColliderType.OBB, 16 }
        };

        _UNIMPLEMENTED_COLLISIONS =
            _COL_BIT_MASK[ColliderType.AABB] |
            _COL_BIT_MASK[ColliderType.OBB];

        Interactions = new CollisionCheckDelegate[(int)ColliderType.LENGTH][];
        int length = (int)ColliderType.LENGTH;
        // initialize array sizes. 
        for (int i = 0; i < length; i++)
        {
            Interactions[i] = new CollisionCheckDelegate[length - i];
        }

        StringBuilder sb = new StringBuilder();
        int warningCount = 0;

        // initialize methods for each collision response.
        // this can be thought of as half of a matrix, or a triangular array.
        // the choice of this implementation allows collision interactions to only be defined once.
        // e.g., if sphere/plane is defined, there is not reason to define plane/sphere
        for (int i = 0; i < length; i++)
        {
            for (int k = 0; k < Interactions[i].Length; k++)
            {
                ColliderType ctI = (ColliderType)i;
                ColliderType ctIK = (ColliderType)(i + k);

                Interactions[i][k] = GetCollisionCheck(ctI, ctIK, ref warningCount);
                if (warningCount > 0)
                {
                    if (warningCount == 1)
                    {
                        sb.AppendLine("unimplemented collision(s) detected during initialization:");
                        sb.AppendLine();
                    }
                    else
                    {
                        sb.AppendLine(
                            string.Format("  [WARN]: Collision between {0} and {1} is unimplemented!",
                            ctI.ToString("f"), ctIK.ToString("f"))
                        );
                    }
                }
            }
        }

        if (warningCount > 0)
        {
            StringBuilder final = new StringBuilder();
            final.Append($"[WARN] {warningCount} ");
            final.Append(sb);
            Debug.LogWarning(final.ToString());
        }
    }

    public static bool IsColliding(ICollisionVolume a, ICollisionVolume b, ref CollisionData colData)
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
            return Interactions[typeB][typeA - typeB].Invoke(b, a, ref colData);
        }
        else
            return Interactions[typeA][typeB - typeA].Invoke(a, b, ref colData);
    }

    private static bool CheckUnimplementedCollisions(int a, int b)
    {
        return ((a | b) & _UNIMPLEMENTED_COLLISIONS) > 0;
    }
    private static bool CheckUnimplementedCollisions(ColliderType a, ColliderType b)
    {
        return CheckUnimplementedCollisions(_COL_BIT_MASK[a], _COL_BIT_MASK[b]);
    }

    // methods are separated into a declaration/implementation style
    // to make casting easier. Child-specific parameters such as a sphere's radius or a plane's normal
    // cannot be obtained without downcasting, which cannot be implicit.
    private static CollisionCheckDelegate GetCollisionCheck(ColliderType first, ColliderType second, ref int warningCount)
    {
        // fill in unimplemented collision response with dummy function 
        static bool unimplementedCollision(ICollisionVolume a, ICollisionVolume b, ref CollisionData colData)
        {
            return false;
        }

        if (CheckUnimplementedCollisions(first, second))
        {
            warningCount += 1;



            return unimplementedCollision;
        }

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
                return unimplementedCollision;
        }
    }

    #region Sphere Checks
    private static bool IsSphereSphereColliding(ICollisionVolume a, ICollisionVolume b, ref CollisionData data)
    {
        return IsSphereSphereColliding_Impl(a as SphereCollisionComponent, b as SphereCollisionComponent, ref data);
    }
    private static bool IsSphereSphereColliding_Impl(SphereCollisionComponent a, SphereCollisionComponent b, ref CollisionData data)
    {
        float distance = Vector3.Distance(a.transform.position, b.transform.position);
        float sumRadii = a.Radius + b.Radius;
        if (distance > sumRadii)
        {
            data.PenetrationDepth = 0;
            data.CollisionNormal = Vector3.zero;
            data.ContactPoint = Vector3.zero;
            data.Other = null;
            return false;
        }
        else
        {
            data.PenetrationDepth = distance - sumRadii;
            data.CollisionNormal = (b.Center - a.Center).normalized;
            data.ContactPoint = data.CollisionNormal * data.PenetrationDepth;
            data.Other = b;
            return true;
        }
    }

    private static bool IsSpherePlaneColliding(ICollisionVolume a, ICollisionVolume b, ref CollisionData data)
    {
        return IsSpherePlaneColliding_Impl(a as SphereCollisionComponent, b as PlaneCollisionComponent, ref data);
    }
    private static bool IsSpherePlaneColliding_Impl(SphereCollisionComponent sphere, PlaneCollisionComponent plane, ref CollisionData data)
    {
        float distance = plane.GetDistance(sphere.transform.position);
        if (distance > sphere.Radius)
        {
            return false;
        }
        else
        {
            Debug.Log("colliding!");
            data.PenetrationDepth = sphere.Radius - distance;
            data.CollisionNormal = plane.Axes.Normal.normalized;
            data.ContactPoint = Vector3.ProjectOnPlane(sphere.transform.position, data.CollisionNormal);
            data.Other = plane;
            return true;
        }
    }

    private static bool IsSphereHalfSpaceColliding(ICollisionVolume a, ICollisionVolume b, ref CollisionData data)
    {
        return IsSphereHalfspaceColliding_Impl(a as SphereCollisionComponent, b as HalfspaceCollisionComponent, ref data);
    }
    private static bool IsSphereHalfspaceColliding_Impl(SphereCollisionComponent sphere, HalfspaceCollisionComponent halfspace, ref CollisionData data)
    {
        return halfspace.GetSignedDistance(sphere.transform.position) - sphere.Radius <= 0;
    }

    private static bool IsSphereAABBColliding(ICollisionVolume a, ICollisionVolume b, ref CollisionData data)
    {
        return IsSphereAABBColliding_Impl(a as SphereCollisionComponent, b as AABBCollisionComponent, ref data);
    }
    private static bool IsSphereAABBColliding_Impl(SphereCollisionComponent a, AABBCollisionComponent b, ref CollisionData data)
    {
        return false;
    }
    #endregion

    #region Plane Checks
    private static bool IsPlanePlaneColliding(ICollisionVolume a, ICollisionVolume b, ref CollisionData data)
    {
        return IsPlanePlaneColliding_Impl(a as PlaneCollisionComponent, b as PlaneCollisionComponent, ref data);
    }
    private static bool IsPlanePlaneColliding_Impl(PlaneCollisionComponent a, PlaneCollisionComponent b, ref CollisionData data)
    {
        return Vector3.Dot(a.Axes.Normal, b.Axes.Normal) != 1.0f;
    }

    private static bool IsPlaneHalfSpaceColliding(ICollisionVolume a, ICollisionVolume b, ref CollisionData data)
    {
        return IsPlaneHalfspaceColliding_Impl(a as PlaneCollisionComponent, b as HalfspaceCollisionComponent, ref data);
    }
    private static bool IsPlaneHalfspaceColliding_Impl(PlaneCollisionComponent a, HalfspaceCollisionComponent b, ref CollisionData data)
    {
        if (b.IsInsideHalfspace(a.transform.position)) return true;
        else return Vector3.Dot(a.Axes.Normal, b.Axes.Normal) != 1.0f;
    }

    private static bool IsPlaneAABBColliding(ICollisionVolume a, ICollisionVolume b, ref CollisionData data)
    {
        return IsPlaneAABBColliding_Impl(a as PlaneCollisionComponent, b as AABBCollisionComponent, ref data);
    }
    private static bool IsPlaneAABBColliding_Impl(PlaneCollisionComponent a, AABBCollisionComponent b, ref CollisionData data)
    {
        return false;
    }
    #endregion

    #region Half Space Checks
    private static bool IsHalfSpaceHalfSpaceColliding(ICollisionVolume a, ICollisionVolume b, ref CollisionData data)
    {
        return IsHalfspaceHalfspaceColliding_Impl(a as HalfspaceCollisionComponent, b as HalfspaceCollisionComponent, ref data);
    }
    private static bool IsHalfspaceHalfspaceColliding_Impl(HalfspaceCollisionComponent a, HalfspaceCollisionComponent b, ref CollisionData data)
    {
        if (a.IsInsideHalfspace(b.transform.position)) return true;
        else return Vector3.Dot(a.Axes.Normal, b.Axes.Normal) != -1.0f;
    }

    private static bool IsHalfSpaceAABBColliding(ICollisionVolume a, ICollisionVolume b, ref CollisionData data)
    {
        return IsHalfspaceAABBColliding_Impl(a as HalfspaceCollisionComponent, b as AABBCollisionComponent, ref data);
    }
    private static bool IsHalfspaceAABBColliding_Impl(HalfspaceCollisionComponent a, AABBCollisionComponent b, ref CollisionData data)
    {
        return false;
    }
    #endregion

    #region AABB Checks
    private static bool IsAABBAABBCollliding(ICollisionVolume a, ICollisionVolume b, ref CollisionData data)
    {
        return IsAABBAABBColliding_Impl(a as AABBCollisionComponent, b as AABBCollisionComponent, ref data);
    }
    private static bool IsAABBAABBColliding_Impl(AABBCollisionComponent a, AABBCollisionComponent b, ref CollisionData data)
    {
        return false;
    }
    #endregion

    /// <summary>
    /// Calculates the required displacement to unintersect <see cref="ICollisionVolume"/>s <paramref name="collider"/> and <paramref name="b"/>.<br/>
    /// If <paramref name="collider"/>'s <see cref="VelocityMode"/> is <see cref="VelocityMode.ZeroOnImpact"/>, <paramref name="velocity"/> will 
    /// be set to <see cref="Vector3.zero"/>.<br/>
    /// If <paramref name="collider"/>'s <see cref="VelocityMode"/> is <see cref="VelocityMode.Reflect"/>, <paramref name="velocity"/> will be reflected along
    /// the plane created from the collision.
    /// </summary>
    /// <param name="velocity">The current velocity (if any) of <see cref="ICollisionVolume"/> <paramref name="collider"/>.</param>
    /// <param name="collider">The current <see cref="ICollisionVolume"/> to check collisions with.</param>
    /// <param name="b">The opposing <see cref="ICollisionVolume"/> to check against.</param>
    /// <returns>
    /// The displacement vector that unintersects <see cref="SphereCollisionComponent"/> <paramref name="collider"/>.<br/>
    /// If <paramref name="b"/> is also kinematic, displacement vector will have 50% magnitude to account for both volumes being displaced.
    /// </returns>
    public static Vector3 GetResponse(ref Vector3 velocity, ref Vector3 position, ICollisionVolume collider, CollisionData colData)
    {
        return (collider.Type, colData.Other.Type) switch
        {
            (ColliderType.Sphere, ColliderType.Sphere) =>
                SphereSphereCollisionResponse(ref velocity, ref position, collider as SphereCollisionComponent, colData),
            (ColliderType.Sphere, ColliderType.Plane) =>
                SpherePlaneCollisionResponse(ref velocity, ref position, collider as SphereCollisionComponent, colData),
            (ColliderType.Sphere, ColliderType.Halfspace) =>
                SphereHalfspaceCollisionResponse(ref velocity, ref position, collider as SphereCollisionComponent, colData),
            (ColliderType.Sphere, ColliderType.AABB) =>
                SphereAABBCollisionResponse(ref velocity, ref position, collider as SphereCollisionComponent, colData),
            _ => Vector3.zero,
        };
    }

    #region Sphere Responses
    private static Vector3 SphereSphereCollisionResponse(ref Vector3 velocity, ref Vector3 position, SphereCollisionComponent sphere, CollisionData colData)
    {
        if (!sphere.IsKinematic)
            return Vector3.zero;

        Vector3 displacement = colData.CollisionNormal * colData.PenetrationDepth;

        /*if (colData.Other.IsKinematic)
            displacement *= 0.5f;*/

        position += displacement;

        switch (sphere.VelocityMode)
        {
            case VelocityMode.ZeroOnImpact:
                velocity = Vector3.zero;
                return displacement;
            case VelocityMode.Reflect:
                velocity = Vector3.Reflect((velocity - displacement).normalized, colData.CollisionNormal) * velocity.magnitude;
                return displacement;
            default:
                return displacement;
        }
    }

    private static Vector3 SpherePlaneCollisionResponse(ref Vector3 velocity, ref Vector3 position, SphereCollisionComponent sphere, CollisionData colData)
    {
        if (sphere.IsKinematic == false)
            return Vector3.zero;

        Vector3 displacement = Vector3.Project(position, colData.CollisionNormal).normalized * (colData.PenetrationDepth - sphere.SkinWidth);
        
        if (colData.Other.IsKinematic)
            displacement *= 0.5f;

        position += displacement;

        // continuous
        if (colData.TimeSinceCollisionStart > PhysicsBodyUpdateSystem.TimeStep)
        {
            velocity -= Vector3.Project(velocity.normalized, colData.CollisionNormal) * velocity.magnitude;
            return displacement;
        }

        // only modify velocity if initial collision
        switch (sphere.VelocityMode)
        {
            case VelocityMode.ZeroOnImpact:
                velocity = Vector3.zero;
                return displacement;
            case VelocityMode.Reflect:
            case VelocityMode.Restitution:
                float effectiveRestitution = Mathf.Min(sphere.GetBody().Restitution(), colData.Other.GetBody().Restitution());
                
                velocity = -(effectiveRestitution) * (Vector3.Project(velocity.normalized, colData.CollisionNormal) * velocity.magnitude);

                //velocity *= (1 - colData.PenetrationDepth);
                return displacement;
            default:
                return displacement;
        }
    }

    private static Vector3 SphereHalfspaceCollisionResponse(ref Vector3 velocity, ref Vector3 position, SphereCollisionComponent a, CollisionData colData)
    {
        HalfspaceCollisionComponent b = colData.Other as HalfspaceCollisionComponent;
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

        float displacement = a.Radius - b.GetSignedDistance(position);

        return normal * displacement;
    }

    private static Vector3 SphereAABBCollisionResponse(ref Vector3 velocity, ref Vector3 position, SphereCollisionComponent a, CollisionData colData)
    {
        return Vector3.zero;
    }
    #endregion

}