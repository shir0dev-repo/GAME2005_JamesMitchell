using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereShape : PhysicsShape
{
    [SerializeField] private float m_radius = 0.5f;

    private PhysicsVolume m_boundingBox;

    private void Awake()
    {
        m_boundingBox = new PhysicsVolume(transform.position, transform.localRotation, Vector3.one * m_radius);
    }

    public override PhysicsVolume getBoundingBox()
    {
        return m_boundingBox;
    }

    private void Update()
    {
        m_boundingBox.UpdatePositionAndRotation(transform.position, transform.rotation);
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
            Gizmos.color = Color.green;

            //Gizmos.DrawWireMesh(m);
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
