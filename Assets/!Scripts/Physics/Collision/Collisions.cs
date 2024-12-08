using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;

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
    private delegate bool CollisionCheckDelegate(ICollisionVolume colliderA, ICollisionVolume colliderB, CollisionData collisionData);
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

    public static bool IsColliding(ICollisionVolume a, ICollisionVolume b, CollisionData colData)
    {
        int typeA = (int)a.Type;
        int typeB = (int)b.Type;

        colData ??= new CollisionData();

        // because interactions are implemented in a triangular manner,
        // passing in the larger value first results in an IndexOutOfRangeException.
        // e.g. If typeA is AABB (3) and typeB is Sphere (0), the definition of this collision is at 
        // Interactions[0][3]. Assuming 4 VolumeTypes, Interactions[3][0] is defined as the collision
        // between two VolumeTypes, both of type 4.        
        if (typeB < typeA)
        {
            return Interactions[typeB][typeA - typeB].Invoke(b, a, colData);
        }
        else
            return Interactions[typeA][typeB - typeA].Invoke(a, b, colData);
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
        static bool unimplementedCollision(ICollisionVolume a, ICollisionVolume b, CollisionData colData)
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
    private static bool IsSphereSphereColliding(ICollisionVolume a, ICollisionVolume b, CollisionData data)
    {
        return IsSphereSphereColliding_Impl(a as SphereCollisionComponent, b as SphereCollisionComponent, data);
    }
    private static bool IsSphereSphereColliding_Impl(SphereCollisionComponent a, SphereCollisionComponent b, CollisionData data)
    {
        float distance = Vector3.Distance(a.TheoreticalPosition, b.TheoreticalPosition);
        float sumRadii = a.Radius + b.Radius;

        if (distance > sumRadii)
        {
            return false;
        }
        else
        {
            Vector3 contact = (b.TheoreticalPosition - a.TheoreticalPosition).normalized * (distance - sumRadii);
            data.Update(a, b,
                (a.transform.position - b.transform.position).normalized,
                b.GetBody().Velocity - a.GetBody().Velocity,
                contact,
                sumRadii - distance,
                false);

            return true;
        }
    }

    private static bool IsSpherePlaneColliding(ICollisionVolume a, ICollisionVolume b, CollisionData data)
    {
        return IsSpherePlaneColliding_Impl(a as SphereCollisionComponent, b as PlaneCollisionComponent, data);
    }
    private static bool IsSpherePlaneColliding_Impl(SphereCollisionComponent sphere, PlaneCollisionComponent plane, CollisionData data)
    {
        float distance = plane.GetDistance(sphere.TheoreticalPosition);

        if (distance > sphere.Radius)
        {
            return false;
        }
        else
        {
            Vector3 contact = (sphere.TheoreticalPosition - plane.transform.position).normalized * (sphere.Radius - distance);
            data.Update(sphere, plane,
                plane.Axes.Normal,
                -sphere.GetBody().Velocity,
                contact,
                sphere.Radius - distance,
                false);

            return true;
        }
    }

    private static bool IsSphereHalfSpaceColliding(ICollisionVolume a, ICollisionVolume b, CollisionData data)
    {
        return IsSphereHalfspaceColliding_Impl(a as SphereCollisionComponent, b as HalfspaceCollisionComponent, data);
    }
    private static bool IsSphereHalfspaceColliding_Impl(SphereCollisionComponent sphere, HalfspaceCollisionComponent halfspace, CollisionData data)
    {
        float distance = halfspace.GetSignedDistance(sphere.TheoreticalPosition);

        if (distance > sphere.Radius)
        {
            return false;
        }
        else
        {
            Vector3 contact = (sphere.TheoreticalPosition - halfspace.transform.position).normalized * (sphere.Radius - distance);
            data.Update(sphere, halfspace,
                halfspace.Axes.Normal,
                -sphere.GetBody().Velocity,
                contact,
                Mathf.Abs(sphere.Radius - distance),
                false);
            return true;
        }
    }

    private static bool IsSphereAABBColliding(ICollisionVolume a, ICollisionVolume b, CollisionData data)
    {
        return IsSphereAABBColliding_Impl(a as SphereCollisionComponent, b as AABBCollisionComponent, data);
    }
    private static bool IsSphereAABBColliding_Impl(SphereCollisionComponent sphere, AABBCollisionComponent box, CollisionData data)
    {
        Vector3 closestPoint = box.GetClosestPoint(sphere.TheoreticalPosition);
        float distance = Vector3.Distance(closestPoint, sphere.TheoreticalPosition);

        if (distance > sphere.Radius)
        {
            return false;
        }
        else
        {
            data.Update(sphere, box,
                (sphere.transform.position - box.transform.position).normalized,
                box.GetBody().Velocity - sphere.GetBody().Velocity,
                closestPoint,
                sphere.Radius - distance,
                false);

            return true;
        }
    }
    #endregion

    #region Plane Checks
    private static bool IsPlanePlaneColliding(ICollisionVolume a, ICollisionVolume b, CollisionData data)
    {
        return false;
    }

    private static bool IsPlaneHalfSpaceColliding(ICollisionVolume a, ICollisionVolume b, CollisionData data)
    {
        return false;
    }

    private static bool IsPlaneAABBColliding(ICollisionVolume a, ICollisionVolume b, CollisionData data)
    {
        return IsPlaneAABBColliding_Impl(a as PlaneCollisionComponent, b as AABBCollisionComponent, data);
    }
    private static bool IsPlaneAABBColliding_Impl(PlaneCollisionComponent plane, AABBCollisionComponent box, CollisionData data)
    {
        float distanceToExtents = plane.GetDistance(box.TheoreticalPosition + box.HalfExtents);
        float distanceToCenter = Mathf.Abs(Vector3.Dot(plane.Axes.Normal, box.TheoreticalPosition) - plane.Axes.D);

        if (distanceToCenter > distanceToExtents)
        {
            return false;
        }
        else
        {
            data.Update(plane, box,
                plane.Axes.Normal,
                -box.GetBody().Velocity,
                box.GetClosestPoint(box.TheoreticalPosition - Vector3.Project(box.TheoreticalPosition, plane.Axes.Normal)),
                distanceToExtents - distanceToCenter,
                false);
            return true;
        }
    }
    #endregion

    #region Half Space Checks
    private static bool IsHalfSpaceHalfSpaceColliding(ICollisionVolume a, ICollisionVolume b, CollisionData data)
    {
        return false;
    }

    private static bool IsHalfSpaceAABBColliding(ICollisionVolume a, ICollisionVolume b, CollisionData data)
    {
        return IsHalfspaceAABBColliding_Impl(a as HalfspaceCollisionComponent, b as AABBCollisionComponent, data);
    }
    private static bool IsHalfspaceAABBColliding_Impl(HalfspaceCollisionComponent halfspace, AABBCollisionComponent box, CollisionData data)
    {
        Vector3 normal = halfspace.Axes.Normal;
        float projectionRadius =
            Mathf.Abs(normal.x * box.HalfExtents.x) +
            Mathf.Abs(normal.y * box.HalfExtents.y) +
            Mathf.Abs(normal.z * box.HalfExtents.z);

        float distance = halfspace.GetSignedDistance(box.TheoreticalPosition);

        if (distance > projectionRadius)
        {
            return false;
        }
        else
        {
            data.Update(halfspace, box,
                halfspace.Axes.Normal,
                -box.GetBody().Velocity,
                box.FindMostSimilarVertex(-normal) + box.TheoreticalPosition,
                projectionRadius - distance,
                false);
            return true;
        }
    }
    #endregion

    #region AABB Checks
    private static bool IsAABBAABBCollliding(ICollisionVolume a, ICollisionVolume b, CollisionData data)
    {
        return IsAABBAABBColliding_Impl(a as AABBCollisionComponent, b as AABBCollisionComponent, data);
    }
    private static bool IsAABBAABBColliding_Impl(AABBCollisionComponent a, AABBCollisionComponent b, CollisionData data)
    {
        Vector3 sumExtents = a.HalfExtents + b.HalfExtents;
        Vector3 axialDistance = (a.TheoreticalPosition - b.TheoreticalPosition).Absolute();

        if (axialDistance.x > sumExtents.x || axialDistance.y > sumExtents.y || axialDistance.z > sumExtents.z)
        {
            return false;
        }
        else
        {
            Vector3 normal = axialDistance.normalized;
            Vector3 overlap = sumExtents - (a.TheoreticalPosition - b.TheoreticalPosition).Absolute();
            float depth = Mathf.Min(overlap.x, Mathf.Min(overlap.y, overlap.z));

            /* Might need later
            Vector3 axis;
            if (depth == overlap.x)
                axis = Vector3.left;
            else if (depth == overlap.y)
                axis = Vector3.down;
            else
                axis = Vector3.back;
            
            normal = axis * Mathf.Sign(Vector3.Dot(axis, normal));
            */

            data.Update(a, b,
                normal,
                b.GetBody().Velocity - a.GetBody().Velocity,
                a.TheoreticalPosition + Vector3.Scale(normal, a.HalfExtents),
                depth,
                false);

            return true;
        }
        
    }
    #endregion

    /// <summary>
    /// First, calculates the required displacement to unintersect <see cref="CollisionData.Focused"/> and <see cref="CollisionData.Other"/>.<br/>
    /// Then, applies an impulse to both colliders based on <see cref="PhysicsBody.Velocity"/> and <see cref="PhysicsBody.Mass"/>.<br/>
    /// Finally, applies friction if the collision is continuous (<see cref="CollisionData.TimeSinceCollisionStart"/> > 0).
    /// </summary>
    /// <param name="velocity">The current velocity (if any) of <see cref="ICollisionVolume"/> <paramref name="collider"/>.</param>
    /// <param name="collider">The current <see cref="ICollisionVolume"/> to check collisions with.</param>
    /// <param name="b">The opposing <see cref="ICollisionVolume"/> to check against.</param>
    /// <returns>
    /// The displacement vector that unintersects <see cref="SphereCollisionComponent"/> <paramref name="collider"/>.<br/>
    /// If <paramref name="b"/> is also kinematic, displacement vector will have 50% magnitude to account for both volumes being displaced.
    /// </returns>
    public static Vector3 GetResponse(CollisionData colData)
    {
        return (colData.Focused.Type, colData.Other.Type) switch
        {
            (ColliderType.Sphere, ColliderType.Sphere) =>
                SphereSphereCollisionResponse(ref colData),
            (ColliderType.Sphere, ColliderType.Plane) =>
                SpherePlaneCollisionResponse(ref colData),
            (ColliderType.Sphere, ColliderType.Halfspace) =>
                SphereHalfspaceCollisionResponse(ref colData),
            (ColliderType.Sphere, ColliderType.AABB) =>
                SphereAABBCollisionResponse(ref colData),
            (ColliderType.Plane, ColliderType.AABB) =>
                PlaneAABBCollisionResponse(ref colData),
            (ColliderType.Halfspace, ColliderType.AABB) =>
                HalfspaceAABBCollisionResponse(ref colData),
                (ColliderType.AABB, ColliderType.AABB) =>
                    AABBAABBCollisionResponse(ref colData),
            _ => Vector3.zero,
        };
    }

    #region Sphere Responses
    private static Vector3 SphereSphereCollisionResponse(ref CollisionData colData)
    {
        SphereCollisionComponent sphereA = colData.Focused as SphereCollisionComponent;
        SphereCollisionComponent sphereB = colData.Other as SphereCollisionComponent;

        Vector3 displacement = colData.CollisionNormal * (colData.PenetrationDepth - sphereA.SkinWidth);

        sphereA.transform.position += displacement;
        sphereB.transform.position -= displacement;

        ApplyRelativeImpulses(colData);

        colData.IsResolved = true;
        return displacement;
    }

    private static Vector3 SpherePlaneCollisionResponse(ref CollisionData colData)
    {
        SphereCollisionComponent sphere = colData.Focused as SphereCollisionComponent;

        if (sphere.IsKinematic == false)
            return Vector3.zero;

        Vector3 displacement = colData.CollisionNormal * (colData.PenetrationDepth - sphere.SkinWidth);

        sphere.transform.position += displacement;

        ApplyImpulse(sphere, colData);

        if (colData.TimeSinceCollisionStart >= PhysicsBodyUpdateSystem.TimeStep)
        {
            Vector3 velocityOnPlane = colData.RelativeVelocity - Vector3.Project(colData.RelativeVelocity, colData.CollisionNormal);
            ApplyFriction(sphere, velocityOnPlane, colData);
        }

        colData.IsResolved = true;
        return displacement;
    }

    private static Vector3 SphereHalfspaceCollisionResponse(ref CollisionData colData)
    {
        SphereCollisionComponent sphere = colData.Focused as SphereCollisionComponent;

        Vector3 displacement = colData.CollisionNormal * (colData.PenetrationDepth - sphere.SkinWidth);

        sphere.transform.position += displacement;

        ApplyImpulse(sphere, colData);

        if (colData.TimeSinceCollisionStart >= PhysicsBodyUpdateSystem.TimeStep)
        {
            Vector3 velocityOnPlane = colData.RelativeVelocity - Vector3.Project(colData.RelativeVelocity, colData.CollisionNormal);
            ApplyFriction(sphere, velocityOnPlane, colData);
        }

        colData.IsResolved = true;
        return displacement;
    }

    private static Vector3 SphereAABBCollisionResponse(ref CollisionData colData)
    {
        return Vector3.zero;
    }

    private static Vector3 PlaneAABBCollisionResponse(ref CollisionData colData)
    {
        AABBCollisionComponent box = colData.Other as AABBCollisionComponent;

        Vector3 displacement = colData.CollisionNormal * (colData.PenetrationDepth - box.SkinWidth);
        box.transform.position += displacement;

        ApplyImpulse(box, colData);

        if (colData.TimeSinceCollisionStart >= PhysicsBodyUpdateSystem.TimeStep)
        {
            Vector3 velocityOnPlane = colData.RelativeVelocity - Vector3.Project(colData.RelativeVelocity, colData.CollisionNormal);
            ApplyFriction(box, velocityOnPlane, colData);
        }

        colData.IsResolved = true;
        return displacement;
    }

    private static Vector3 HalfspaceAABBCollisionResponse(ref CollisionData colData)
    {
        AABBCollisionComponent box = colData.Other as AABBCollisionComponent;

        Vector3 displacement = colData.CollisionNormal * (colData.PenetrationDepth - box.SkinWidth);
        box.transform.position += displacement;

        ApplyImpulse(box, colData);

        if (colData.TimeSinceCollisionStart >= PhysicsBodyUpdateSystem.TimeStep)
        {
            Vector3 velocityOnPlane = colData.RelativeVelocity - Vector3.Project(colData.RelativeVelocity, colData.CollisionNormal);
            ApplyFriction(box, velocityOnPlane, colData);
        }

        colData.IsResolved = true;
        return displacement;
    }

    private static Vector3 AABBAABBCollisionResponse(ref CollisionData colData)
    {
        AABBCollisionComponent boxA = colData.Focused as AABBCollisionComponent;
        AABBCollisionComponent boxB = colData.Other as AABBCollisionComponent;
        
        float avgSkinWidth = (boxA.SkinWidth + boxB.SkinWidth) / 2.0f;
        Vector3 displacement = (colData.PenetrationDepth - avgSkinWidth) * 0.5f * colData.CollisionNormal;
        
        boxA.transform.position += displacement;
        boxB.transform.position -= displacement;

        ApplyRelativeImpulses(colData);

        if (colData.TimeSinceCollisionStart >= PhysicsBodyUpdateSystem.TimeStep)
        {
            //ApplyRelativeFriction(colData);
        }

        colData.IsResolved = true;
        return displacement;
    }

    #endregion

    #region Common
    /// <summary>
    /// Apply an impulse to <paramref name="obj"/>. This is typically used when the opposing collider is a plane or halfspace.
    /// </summary>
    private static void ApplyImpulse(CollisionComponent obj, CollisionData colData)
    {
        // velocity is towards plane
        float vDotN = Vector3.Dot(colData.RelativeVelocity, colData.CollisionNormal);

        // apply impulse
        if (colData.TimeSinceCollisionStart <= PhysicsBodyUpdateSystem.TimeStep)
        {
            float effectiveRestitution = Mathf.Min(obj.Material.Bounciness(), colData.Other.Material.Bounciness());

            Vector3 impulseForce = ((1 + effectiveRestitution) * vDotN * colData.CollisionNormal);
            obj.GetBody().AddImpulse(impulseForce, 1);
        }
        // apply the normal force to cancel out the velocity coinciding with the collision normal
        else
        {
            Vector3 velocityOnNormal = Vector3.Project(colData.RelativeVelocity, colData.CollisionNormal);
            obj.GetBody().AddImpulse(velocityOnNormal, 1);
        }
    }

    /// <summary>
    /// Applies an equal impulse to both objects. This is typically used when the opposing collider is also kinematic.
    /// </summary>
    private static void ApplyRelativeImpulses(CollisionData colData)
    {
        float v1DotN = Vector3.Dot(colData.RelativeVelocity, colData.CollisionNormal);

        float inverseSumMasses = 1.0f / (colData.Focused.GetBody().Mass + colData.Other.GetBody().Mass);
        float effectiveRestitution = Mathf.Min(colData.Focused.Material.Bounciness(), colData.Other.Material.Bounciness());

        float power = (1 + effectiveRestitution) * v1DotN / (colData.Focused.GetBody().Mass + colData.Other.GetBody().Mass);
        Vector3 impulseForce = power * colData.CollisionNormal;

        colData.Focused.GetBody().AddImpulse(impulseForce, colData.Other.GetBody().Mass);
        colData.Other.GetBody().AddImpulse(-impulseForce, colData.Focused.GetBody().Mass);
    }

    private static void ApplyFriction(CollisionComponent obj, Vector3 tangentialVelocity, CollisionData colData)
    {
        // apply friction
        Vector3 frictionDirection = -tangentialVelocity.normalized;

        float effectiveFriction = Vector3.Dot(tangentialVelocity, frictionDirection);
        float staticCoeff = (obj.Material.FrictionThreshold() + colData.Other.Material.FrictionThreshold()) / 2.0f;

        if (effectiveFriction <= staticCoeff)
        {
            obj.GetBody().AddImpulseUnscaledTime(frictionDirection * effectiveFriction);
        }
        // apply kinetic friction
        else
        {
            float effectiveKinetic = (obj.Material.Roughness() + colData.Other.Material.Roughness()) / 2.0f;
            Vector3 kineticForce = Mathf.Lerp(0, tangentialVelocity.magnitude, effectiveKinetic) * frictionDirection;

            obj.GetBody().AddImpulseUnscaledTime(kineticForce);
        }
    }

    private static void ApplyRelativeFriction(CollisionData colData)
    {
        Vector3 v1AlongN = colData.RelativeVelocity - Vector3.Project(colData.RelativeVelocity, colData.CollisionNormal);
        Vector3 velBA = colData.Other.GetBody().Velocity - colData.Focused.GetBody().Velocity;
        Vector3 v2AlongN = velBA - Vector3.Project(velBA, -colData.CollisionNormal);

        float staticCoefficient = (colData.Focused.Material.FrictionThreshold() + colData.Other.Material.FrictionThreshold()) * 0.5f;
        float kineticCoefficient = (colData.Focused.Material.Roughness() + colData.Other.Material.Roughness()) * 0.5f;

        colData.Focused.GetBody().AddImpulseUnscaledTime(GetFrictionForce(v1AlongN, colData.CollisionNormal, staticCoefficient, kineticCoefficient));
        colData.Other.GetBody().AddImpulseUnscaledTime(GetFrictionForce(v2AlongN, -colData.CollisionNormal, staticCoefficient, kineticCoefficient));
    }

    private static Vector3 GetFrictionForce(Vector3 tangentialVelocity, Vector3 normal, float staticThreshold, float kineticCoefficient)
    {
        Vector3 frictionDirection = -tangentialVelocity.normalized;
        float effectiveFriction = Vector3.Dot(tangentialVelocity, frictionDirection);

        if (effectiveFriction < staticThreshold)
        {
            return frictionDirection * effectiveFriction;
        }
        else
        {
            return Mathf.Lerp(0, tangentialVelocity.magnitude, kineticCoefficient) * frictionDirection;
        }
    }
    #endregion
}