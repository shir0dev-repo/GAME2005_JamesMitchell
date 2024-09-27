using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleBounceCollision : PhysicsComponent
{
    [SerializeField] private float m_collisionHeight = 0;
    [SerializeField] private float m_collisionRadius = 0.5f;

    public override Vector3 ApplyToObject(ref Vector3 initial)
    {
        if (transform.position.y < m_collisionHeight + m_collisionRadius)
            initial = -initial + (Vector3.up * initial.y) * PhysicsManager.Instance.DeltaTime;
        return initial;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(new Vector3(0, m_collisionHeight, 0), new Vector3(1, 0.01f, 1));
    }
}
