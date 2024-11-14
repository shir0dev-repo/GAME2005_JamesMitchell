using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Launcher : MonoBehaviour
{
    [SerializeField] private float m_initialAngleDegrees = 30;
    [SerializeField] private float m_initialSpeed = 10;
    [SerializeField] private float m_startingHeight = 0;
    [SerializeField] private float m_projectileLifetime = 3f;
    [Space]

    [SerializeField] private Trajectory m_projectilePF;
    [SerializeField] private GameObject m_turretHead;

    private Camera cam;
    private GameObject currentProjectile = null;

    private void Awake()
    {
        cam = Camera.main;
    }

    private void Update()
    {
        transform.position = new Vector3(transform.position.x, m_startingHeight, transform.position.z);
        m_turretHead.transform.rotation = Quaternion.Euler(0, 0, m_initialAngleDegrees);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Trajectory tj = Instantiate(m_projectilePF, m_turretHead.transform.position, Quaternion.identity);
            tj.InitParams(m_initialAngleDegrees, m_initialSpeed, m_projectileLifetime);
            currentProjectile = tj.gameObject;
        }
    }

    private void LateUpdate()
    {
        if (currentProjectile != null)
            cam.transform.position = new Vector3(currentProjectile.transform.position.x, currentProjectile.transform.position.y, -10);
        else
            cam.transform.position = Vector3.back * 10f;
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(m_turretHead.transform.position, m_turretHead.transform.position + m_turretHead.transform.right * 5f);
    }
}
