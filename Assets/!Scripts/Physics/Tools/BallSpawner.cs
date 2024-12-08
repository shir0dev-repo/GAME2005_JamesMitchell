using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallSpawner : MonoBehaviour
{
    [SerializeField] PhysicsMaterialDatabase m_materialDB;
    [SerializeField] private PhysicsBody pf_ball;
    [Space]
    [SerializeField] private float m_spawnForce = 10f;
    [SerializeField] bool m_flipCamera = false;
    private float m_spawnTimer = 0.25f;

    // Update is called once per frame
    void Update()
    {
        if (m_spawnTimer >= 0f)
        {
            m_spawnTimer -= Time.deltaTime;
        }

        // fire on left click
        if (Input.GetMouseButton(0) && m_spawnTimer <= 0)
        {
            m_spawnTimer = 0.25f;
            PhysicsBody pb = Instantiate(pf_ball, transform.position, Quaternion.identity);
            pb.AddImpulse(transform.forward, m_spawnForce);
            pb.GetComponent<CollisionComponent>().SetMaterial(m_materialDB.Database[Random.Range(0, m_materialDB.Database.Count)]);
            
            if (m_flipCamera)
                GetComponent<OrbitalCamera>().SetRotation(-transform.forward);
        }
    }
}
