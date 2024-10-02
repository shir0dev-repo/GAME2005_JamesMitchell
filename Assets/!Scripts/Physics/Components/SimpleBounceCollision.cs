using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleBounceCollision : PhysicsComponentBase
{
    [SerializeField] private float m_collisionHeight = 0;
    [SerializeField] private float m_collisionRadius = 0.5f;

    public override Vector3 Modify(Vector3 initial)
    {
        if (transform.position.y < m_collisionHeight + m_collisionRadius)
        {
            Vector3 groundNormal = Vector3.up;
            Vector3 reflection = Vector3.Reflect(initial.normalized, groundNormal);
            initial = reflection.normalized * initial.magnitude;
        }
        return initial;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(new Vector3(0, m_collisionHeight, 0), new Vector3(1, 0.01f, 1));
    }
}
