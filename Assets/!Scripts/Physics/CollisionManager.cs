using System.Collections.Generic;
using UnityEngine;

public class CollisionManager : Singleton<CollisionManager>
{
    private PartitionedSpace<ICollisionVolume> m_space;
    [SerializeField] private Vector3Int m_chunkSize = Vector3Int.one * 16;

    private List<ICollisionVolume> m_planesAndHalfspaces = new();

    protected override void Awake()
    {
        base.Awake();
        m_space = new PartitionedSpace<ICollisionVolume>(m_chunkSize);
        m_space.SetPerChunkCalculation(CalculateCollisionsPerChunk);
    }

    private void OnEnable()
    {
        PhysicsManager.OnPhysicsUpdate += CheckCollisions;
        PhysicsManager.OnObjectAdded += TryIncludeInCollisions;
    }

    private void TryIncludeInCollisions(object physicsManager, PhysicsBody body)
    {
        if (body.TryGetComponent(out ICollisionVolume cv))
        {
            if (cv is SphereCollisionVolume)
                m_space.AssignPartition(cv);
            else
            {
                m_planesAndHalfspaces.Add(cv);
                Debug.Log("added one");
            }
        }
    }

    private void OnDisable()
    {
        PhysicsManager.OnPhysicsUpdate -= CheckCollisions;
        PhysicsManager.OnObjectAdded -= TryIncludeInCollisions;
    }

    private void CheckCollisions(object physicsManager, float dt)
    {
        m_space.UpdatePartitions();
    }

    private void CalculateCollisionsPerChunk(Partition<ICollisionVolume> chunk)
    {
        ICollisionVolume current;
        ICollisionVolume compare;

        for (int i = 0; i < chunk.Objects.Count; i++)
        {
            current = chunk.Objects[i];
            
            foreach (var planarVolume in m_planesAndHalfspaces)
            {
                compare = planarVolume;
                current.CurrentlyColliding = compare.IsColliding(current);
            }
            for (int j = 0; j < chunk.Objects.Count; j++)
            {
                if (i == j) continue;
                
                compare = chunk.Objects[j];

                current.CurrentlyColliding = compare.CurrentlyColliding = current.IsColliding(compare);                
            }
        }
    }

    public void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        Gizmos.color = new Color(0, 1, 0, 0.4f);

        foreach (var partition in m_space.Partitions)
        {
            Vector3 pos = partition.Key + (Vector3)m_chunkSize * 0.5f;
            Gizmos.DrawCube(pos, m_chunkSize);
        }
    }
}
