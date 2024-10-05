using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragVisualizer : MonoBehaviour
{
    [SerializeField, ReadOnly] private Vector3 m_dragDirection;

    public void UpdateDragVector(Vector3 drag)
    {
        m_dragDirection = drag.normalized;
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + m_dragDirection);
    }
}
