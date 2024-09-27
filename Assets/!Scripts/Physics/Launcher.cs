using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Launcher : MonoBehaviour
{
    [SerializeField] private float m_initialAngleDegrees = 30;
    [SerializeField] private float m_initialSpeed = 10;
    [Space]

    [SerializeField] private Trajectory m_projectilePF;
    [SerializeField] private GameObject m_turretHead;
    private void Update()
    {
        m_turretHead.transform.rotation = Quaternion.Euler(0, 0, m_initialAngleDegrees);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Trajectory tj = Instantiate(m_projectilePF, m_turretHead.transform.position, Quaternion.identity);
            tj.InitParams(m_initialAngleDegrees, m_initialSpeed);
        }
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(m_turretHead.transform.position, m_turretHead.transform.position + m_turretHead.transform.right * 5f);
    }
}
