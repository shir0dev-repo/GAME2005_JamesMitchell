using UnityEngine;

[System.Serializable]
public class PlaneAxis
{
    public Vector3 Normal;
    public Vector3 Tangent;
    public Vector3 Bitangent;
    public float DistanceFromOrigin;

    public PlaneAxis(Vector3 normal)
    {
        Normal = normal.normalized;
        Tangent = CalculateTangent(Normal);
        Bitangent = CalculateBitangent(Normal, Tangent);
        DistanceFromOrigin = 0;
    }
    public PlaneAxis(Vector3 normal, float distanceFromOrigin)
    {
        Normal = normal.normalized;
        Tangent = CalculateTangent(Normal);
        Bitangent = CalculateBitangent(Normal, Tangent);
        DistanceFromOrigin = distanceFromOrigin;
    }

    public void Recalculate(Transform t)
    {
        Normal = t.up;
        Tangent = CalculateTangent(Normal);
        Bitangent = CalculateBitangent(Normal, Tangent);
        DistanceFromOrigin = Vector3.Distance(Vector3.zero, t.position);
    }

    private static Vector3 CalculateTangent(Vector3 normal)
    {
        Vector3 arbitrary;
        if (Mathf.Abs(normal.x) > Mathf.Abs(normal.y) && Mathf.Abs(normal.x) > Mathf.Abs(normal.z))
            arbitrary = Vector3.up;
        else if (Mathf.Abs(normal.y) > Mathf.Abs(normal.x) && Mathf.Abs(normal.y) > Mathf.Abs(normal.z))
            arbitrary = Vector3.forward;
        else
            arbitrary = Vector3.up;

        return Vector3.Cross(normal, arbitrary).normalized;
    }

    private static Vector3 CalculateBitangent(Vector3 normal, Vector3 tangent)
    {
        return Vector3.Cross(normal, tangent).normalized;
    }

    public Vector2 Project(Vector3 point)
    {
        return new Vector2()
        {
            x = Vector3.Dot(point, Tangent),
            y = Vector3.Dot(point, Bitangent)
        };
    }
    public Vector3 ToWorldSpace(Vector3 worldPosition, Vector3 projectedPoint)
    {
        return worldPosition +
            (projectedPoint.x * Tangent) +
            (projectedPoint.y * Bitangent);
    }
}
