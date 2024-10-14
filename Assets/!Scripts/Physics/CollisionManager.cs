using UnityEngine;

public class CollisionManager : Singleton<CollisionManager>
{
    private PartitionedSpace<CollisionVolume> m_space;
    [SerializeField] private Vector3Int m_chunkSize = Vector3Int.one * 16;
    protected override void Awake()
    {
        base.Awake();
        m_space = new PartitionedSpace<CollisionVolume>(m_chunkSize);
        m_space.SetPerChunkCalculation(CalculateCollisionsPerChunk);
    }

    private void OnEnable()
    {
        PhysicsManager.OnPhysicsUpdate += UpdateChunks;
        PhysicsManager.OnObjectAdded += TryIncludeInCollisions;
    }

    private void TryIncludeInCollisions(object physicsManager, PhysicsBody body)
    {
        if (body.TryGetComponent(out CollisionVolume cv))
        {
            m_space.AssignPartition(cv);
        }
    }

    private void OnDisable()
    {
        PhysicsManager.OnPhysicsUpdate -= UpdateChunks;
        PhysicsManager.OnObjectAdded -= TryIncludeInCollisions;
    }

    private void UpdateChunks(object physicsManager, float dt)
    {
        m_space.UpdatePartitions();
    }

    private void CalculateCollisionsPerChunk(Partition<CollisionVolume> chunk)
    {
        for (int i = 0; i < chunk.Objects.Count; i++)
        {
            for (int j = 0; j < chunk.Objects.Count; j++)
            {
                if (i == j) continue;

                if (chunk.Objects[i].IsColliding(chunk.Objects[j]))
                    Debug.Log("collision occured");
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
