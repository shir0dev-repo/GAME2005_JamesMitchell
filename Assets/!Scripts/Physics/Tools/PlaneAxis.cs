using UnityEngine;

[System.Serializable]
public class PlaneAxis
{
    public Vector3 Origin;
    public Vector3 Normal;
    public Vector3 Tangent;
    public Vector3 Bitangent;
    public float D => Origin.magnitude;

    public PlaneAxis(Vector3 normal)
    {
        Normal = normal.normalized;
        Tangent = CalculateTangent(Normal);
        Bitangent = CalculateBitangent(Normal, Tangent);
        Origin = Vector3.zero;
    }
    public PlaneAxis(Vector3 normal, Vector3 origin)
    {
        Normal = normal.normalized;
        Tangent = CalculateTangent(Normal);
        Bitangent = CalculateBitangent(Normal, Tangent);
        Origin = origin;
    }

    public PlaneAxis(Transform t)
    {
        Normal = t.up;
        Tangent = t.right;
        Bitangent = t.forward;
        Origin = t.position;
    }
    public void Recalculate(Transform t)
    {
        Normal = t.up.normalized;
        Tangent = t.right.normalized;// CalculateTangent(Normal);
        Bitangent = t.forward.normalized;// CalculateBitangent(Normal, Tangent);
        Origin = t.position;
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

    public Vector3 Project(Vector3 point)
    {
        return new Vector3()
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