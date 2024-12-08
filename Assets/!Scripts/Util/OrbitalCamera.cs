using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class OrbitalCamera : Singleton<OrbitalCamera>
{
    [SerializeField] private float m_orbitalSpeed = 2f;
    [SerializeField] private float m_orbitDistance = 5f;
    [SerializeField] private float m_minOrbitDist = 5f;
    [SerializeField] private float m_maxOrbitDist = 25f;
    
    [SerializeField, ReadOnly] Vector3 m_velocity = Vector3.zero;
    [SerializeField] private Transform m_pivot;

    protected override void Awake()
    {
        base.Awake();
    }

    private void LateUpdate()
    {
        if (Input.GetMouseButton(1))
        {
            transform.LookAt(m_pivot);

            transform.eulerAngles += new Vector3(-Input.GetAxis("Mouse Y") * m_orbitalSpeed, Input.GetAxis("Mouse X") * m_orbitalSpeed, 0);
        }

        m_orbitDistance = Mathf.Clamp(m_orbitDistance + Input.mouseScrollDelta.y / m_orbitalSpeed, m_minOrbitDist, m_maxOrbitDist);
        transform.position = m_pivot.position - transform.forward * m_orbitDistance;
    }

    public void SetRotation(Vector3 fwd)
    {
        transform.forward = fwd;
        transform.position = m_pivot.position - transform.forward * m_orbitDistance;
    }
}
