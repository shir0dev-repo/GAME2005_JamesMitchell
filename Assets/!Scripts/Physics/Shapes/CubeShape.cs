using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeShape : PhysicsShape
{
    private PhysicsVolume m_boundingBox;
    public override PhysicsVolume BoundingBox => m_boundingBox;
    private void Awake()
    {
        m_boundingBox = new PhysicsVolume(transform.position, transform.localRotation, transform.localScale);
    }

    private void OnEnable()
    {
        PhysicsManager.OnPhysicsUpdate += UpdateBounds;
    }

    private void UpdateBounds(object physicsManager, float deltaTime)
    {
        m_boundingBox.UpdatePositionAndRotation(transform.position, transform.rotation);
    }

    private void OnDisable()
    {
        PhysicsManager.OnPhysicsUpdate -= UpdateBounds;
    }

    private void OnDrawGizmosSelected()
    {
        if (Application.isPlaying)
        {
            Mesh m = new Mesh();
            m.vertices = m_boundingBox.Points;
            m.triangles = new int[]
            {
                0, 1, 3,
                3, 2, 0,

                1, 5, 7,
                7, 3, 1,

                0, 2, 6,
                6, 4, 0,

                5, 4, 6,
                6, 7, 5,

                3, 2, 6,
                6, 5, 3,

                0, 4, 5,
                5, 1, 0
            };
            m.RecalculateNormals();
            Gizmos.color = Color.magenta;

            Gizmos.DrawWireMesh(m, 0, m_boundingBox.Center);
            /*
        m_points[0] = m_center + new Vector3(m_halfExtents.x, -m_halfExtents.y, m_halfExtents.z); // rbf
        m_points[1] = m_center + new Vector3(-m_halfExtents.x, -m_halfExtents.y, m_halfExtents.z); // lbf
        m_points[2] = m_center + new Vector3(m_halfExtents.x, m_halfExtents.y, m_halfExtents.z); // rtf
        m_points[3] = m_center + new Vector3(-m_halfExtents.x, m_halfExtents.y, m_halfExtents.z); // ltf

        m_points[4] = m_center + new Vector3(m_halfExtents.x, -m_halfExtents.y, -m_halfExtents.z); // rbb
        m_points[5] = m_center + new Vector3(-m_halfExtents.x, -m_halfExtents.y, -m_halfExtents.z); // lbb
        m_points[6] = m_center + new Vector3(m_halfExtents.x, m_halfExtents.y, -m_halfExtents.z); // rtb
        m_points[7] = m_center + new Vector3(-m_halfExtents.x, m_halfExtents.y, -m_halfExtents.z); // ltb*/
        }
    }
}
