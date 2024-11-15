using System.Collections.Generic;
using UnityEngine;

public class CollisionManager : Singleton<CollisionManager>
{
    private static void MarkForUpdate() => m_shouldUpdate = true;
    private static bool m_shouldUpdate = false;
    private static PartitionedSpace<ICollisionVolume> m_space;
    [SerializeField] private Vector3Int m_chunkSize = Vector3Int.one * 16;

    private static readonly List<ICollisionVolume> m_planesAndHalfspaces = new();
    private static readonly List<ICollisionVolume> m_currentlySimulatedColliders = new();

    [SerializeField] private bool m_drawChunks = false;

    private static void CollisionManagerUpdateInjected()
    {
        if (m_shouldUpdate)
        {
            m_shouldUpdate = false;
            m_space.UpdatePartitions();
        }
    }

    protected override void Awake()
    {
        base.Awake();
        m_space = new PartitionedSpace<ICollisionVolume>(m_chunkSize, CalculateCollisionsPerChunk);
    }

    private void Start()
    {
        PhysicsBodyUpdateSystem.OnMarkForUpdate += MarkForUpdate;
    }

    public static void AddToSimulation(CollisionComponent collider)
    {

        if (collider is PlaneCollisionComponent or HalfspaceCollisionComponent)
        {
            if (!m_planesAndHalfspaces.Contains(collider))
            {
                m_planesAndHalfspaces.Add(collider);
            }
        }
        else
        {
            if (!m_currentlySimulatedColliders.Contains(collider))
            {
                m_currentlySimulatedColliders.Add(collider);
                m_space.AssignPartition(collider);
            }
        }
    }

    //TODO:
    // implement way to remove objects from PartitionedSpace<T>
    public static void RemoveFromSimulation(CollisionComponent collider)
    {
        if (collider is PlaneCollisionComponent or HalfspaceCollisionComponent)
        {
            m_planesAndHalfspaces.Remove(collider);
        }
        else
        {
            m_currentlySimulatedColliders.Remove(collider);
            Vector3Int key = m_space.GetKey(collider.transform.position);
            if (m_space.Partitions.ContainsKey(key))
            {
                Partition<ICollisionVolume> currentSpace = m_space.Partitions[m_space.GetKey(collider.transform.position)];
                currentSpace.Remove(collider);
            }
        }
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
                bool collisionOccurred = compare.IsColliding(current);
                if (collisionOccurred)
                {
                    current.CurrentlyColliding = true;
                    current.CurrentCollisions.Push(compare);
                }
            }
            for (int j = 0; j < chunk.Objects.Count; j++)
            {
                if (i == j) continue;

                compare = chunk.Objects[j];

                bool collisionOccurred = current.IsColliding(compare);
                if (collisionOccurred)
                {
                    current.CurrentlyColliding = true;
                    current.CurrentCollisions.Push(compare);
                    compare.CurrentlyColliding = true;
                    compare.CurrentCollisions.Push(current);
                }
            }
        }
    }

    protected override void OnApplicationQuit()    
    {
        PhysicsBodyUpdateSystem.OnMarkForUpdate -= MarkForUpdate;
        base.OnApplicationQuit();
    }

    public void OnDrawGizmos()
    {
        if (!Application.isPlaying || !m_drawChunks) return;
        Gizmos.color = new Color(0, 1, 0, 0.4f);

        foreach (var partition in m_space.Partitions)
        {
            Vector3 pos = partition.Key + (Vector3)m_chunkSize * 0.5f;
            Gizmos.DrawCube(pos, m_chunkSize);
        }
    }
}
