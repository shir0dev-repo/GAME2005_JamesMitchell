using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test : MonoBehaviour
{
    public SphereCollisionVolume[] spheres;
    Space<CollisionVolume> space;

    private void Start()
    {
        space = new Space<CollisionVolume>(Vector3Int.one * 16);
        for (int i = 0; i < spheres.Length; i++)
            space.AssignPartition(spheres[i]);

        
    }

    private void FixedUpdate()
    {
        space.UpdatePartitions();
    }

    public void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        Gizmos.color = new Color(0, 1, 0, 0.4f);

        foreach (var partition in space.Partitions)
        {
            Vector3 pos = partition.Key + (Vector3) space.ChunkSize * 0.5f;
            Gizmos.DrawCube(pos, space.ChunkSize);
        }
    }
}
