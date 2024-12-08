using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AABBCollisionComponent : CollisionComponent
{
    public override ColliderType Type => ColliderType.AABB;

    public override VelocityMode VelocityMode => VelocityMode.Restitution;

    public override bool IsKinematic => m_IsKinematic;
    [SerializeField] private bool m_IsKinematic;

    public Vector3 HalfExtents => m_halfExtents;
    [SerializeField, Min(0)] private Vector3 m_halfExtents = Vector3.one * 0.5f;

    public override float CrossSectionalArea(Vector3 normal)
    {
        return new ConvexHull(normal, ScaleAll(BoxVertices, m_halfExtents)).Area;
    }

    protected override void Awake()
    {
        base.Awake();
        UpdateBounds();
    }

    public override float Volume()
    {
        return 2.0f * m_halfExtents.x * 2.0f * m_halfExtents.y * 2.0f * m_halfExtents.z;
    }

    public float SquaredDistance(Vector3 point)
    {
        float dist = 0.0f;
        Vector3 min = -m_halfExtents;
        Vector3 max = m_halfExtents;
        for (int i = 0; i < 3; i++)
        {
            float v = point[i];

            if (v < min[i]) dist += (min[i] - v) * (min[i] - v);
            if (v > max[i]) dist += (v - max[i]) * (v - max[i]);
        }

        return dist;
    }

    public Vector3 FindMostSimilarVertex(Vector3 direction)
    {
        Vector3 dir = direction.normalized;
        Vector3[] vertices = ScaleAll(BoxVertices, m_halfExtents);
        Vector3 output = vertices[0];
        float currentDot = float.MinValue;

        foreach (Vector3 v in vertices)
        {
            float d = Vector3.Dot(dir, v);
            if (d > currentDot)
            {
                output = v;
                currentDot = d;
            }
        }

        return output;
    }

    public Vector3 GetClosestPoint(Vector3 point)
    {
        Vector3 result = Vector3.zero;
        Vector3 min = -m_halfExtents;
        Vector3 max = m_halfExtents;

        for (int i = 0; i < 3; i++)
        {
            float v = point[i];
            if (v <= min[i]) v = min[i];
            if (v >= max[i]) v = max[i];

            result[i] = v;
        }

        Debug.DrawRay(transform.position, result, Color.green);
        return result;
    }

    [ContextMenu("Update Bounds")]
    public void UpdateBounds()
    {
        transform.localScale = 2.0f * m_halfExtents;
    }

    private static Vector3[] ScaleAll(Vector3[] vArr, Vector3 scale)
    {
        Vector3[] result = new Vector3[vArr.Length];

        for (int i = 0; i < vArr.Length; i++)
        {
            result[i] = Vector3.Scale(vArr[i], scale);
        }

        return result;
    }
    private static Vector3[] BoxVertices = new Vector3[8]
    {
        
        new Vector3(1, -1, 1),      // rbf
        new Vector3(-1, -1, 1),     // lbf
        new Vector3(1, 1, 1),       // rtf
        new Vector3(-1, 1, 1),      // ltf

        new Vector3(1, -1, -1),     // rbb
        new Vector3(-1, -1, -1),    // lbb
        new Vector3(1, 1, -1),      // rtb
        new Vector3(-1, 1, -1),     // ltb
    };
}
