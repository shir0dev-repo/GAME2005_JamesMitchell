using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawPath : MonoBehaviour
{
    private Vector3 m_positionLastFrame;

    private void Awake()
    {
        m_positionLastFrame = transform.position;
    }

    private void Update()
    {
        Debug.DrawLine(m_positionLastFrame, transform.position, Color.red, 1f);
        m_positionLastFrame = transform.position;
    }
}
