using System;
using UnityEngine;

public enum VolumeType
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
        Interactions = new Func<ICollisionVolume, ICollisionVolume, bool>[(int)VolumeType.LENGTH][];
        int length = (int)VolumeType.LENGTH;
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
                Interactions[i][k] = GetInteraction((VolumeType)i, (VolumeType)(i + k));
            }
        }
    }

    public static bool IsColliding(ICollisionVolume a, ICollisionVolume b)
    {
        int typeA = (int)a.Type; // 3
        int typeB = (int)b.Type; // 0

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
    private static Func<ICollisionVolume, ICollisionVolume, bool> GetInteraction(VolumeType first, VolumeType second)
    {
        switch (first, second)
        {
            case (VolumeType.Sphere, VolumeType.Sphere):
                return SSCollision;
            case (VolumeType.Sphere, VolumeType.Plane):
                return SPCollision;
            case (VolumeType.Sphere, VolumeType.Halfspace):
                return SHCollision;
            case (VolumeType.Sphere, VolumeType.AABB):
                return SBCollision;

            case (VolumeType.Plane, VolumeType.Plane):
                return PPCollision;
            case (VolumeType.Plane, VolumeType.Halfspace):
                return PHCollision;
            case (VolumeType.Plane, VolumeType.AABB):
                return PBCollision;

            case (VolumeType.Halfspace, VolumeType.Halfspace):
                return HHCollision;
            case (VolumeType.Halfspace, VolumeType.AABB):
                return HBCollision;
            case (VolumeType.AABB, VolumeType.AABB):
                return BBCollision;

            default:
                throw new UnimplementedCollisionException("Collision may be implemented, try switching the order.");
        }
    }

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
}