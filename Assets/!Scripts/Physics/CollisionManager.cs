using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CollisionManager : Singleton<CollisionManager>
{
    private static void MarkForUpdate() => m_shouldUpdate = true;
    private static bool m_shouldUpdate = false;
    private static PartitionedSpace<CollisionComponent> m_space;
    [SerializeField] private Vector3Int m_chunkSize = Vector3Int.one * 16;

    private static readonly List<CollisionComponent> m_planesAndHalfspaces = new();
    private static readonly List<CollisionComponent> m_currentlySimulatedColliders = new();

    [SerializeField] private bool m_drawChunks = false;
    public static Action OnCollisionChecked;
    private static void CollisionManagerUpdateInjected()
    {
        if (m_shouldUpdate)
        {
            m_shouldUpdate = false;
            m_space.UpdatePartitions();
            OnCollisionChecked?.Invoke();
        }
    }

    protected override void Awake()
    {
        base.Awake();
        m_space = new PartitionedSpace<CollisionComponent>(m_chunkSize, CalculateCollisionsPerChunk);
    }

    private void Start()
    {
        PhysicsBodyUpdateSystem.OnMarkForUpdate += MarkForUpdate;
    }

    public static void AddToSimulation(CollisionComponent collider)
    {

        if (collider is PlaneCollisionComponent or HalfspaceCollisionComponent)
        {
            m_planesAndHalfspaces.Add(collider);
        }
        else
        {
            m_space.AssignPartition(collider);
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
            Vector3Int key = m_space.GetKey(collider.transform.position);
            if (m_space.Partitions.ContainsKey(key))
            {
                Partition<CollisionComponent> currentSpace = m_space.Partitions[m_space.GetKey(collider.transform.position)];
                currentSpace.Remove(collider);
            }
        }
    }

    private void CalculateCollisionsPerChunk(Partition<CollisionComponent> chunk)
    {
        CollisionComponent current;
        CollisionComponent compare;

        for (int i = 0; i < chunk.Objects.Count; i++)
        {
            current = chunk.Objects[i];

            foreach (var planarVolume in m_planesAndHalfspaces)
            {
                CollisionData collisionData = current.CurrentCollisions.Find(d => d.Other.GetInstanceID() == planarVolume.GetInstanceID());
                int existingIndex;
                if (collisionData == null)
                {
                    collisionData = new();
                    existingIndex = -1;
                }
                else
                {
                    existingIndex = current.CurrentCollisions.IndexOf(collisionData);
                }

                if ((current as ICollisionVolume).IsColliding(planarVolume, ref collisionData))
                {
                    current.CurrentlyColliding = true;

                    if (existingIndex == -1)
                        current.CurrentCollisions.Add(collisionData);
                    else
                        current.CurrentCollisions[existingIndex].Update(collisionData);
                }
                else
                {
                    if (existingIndex != -1)
                        current.CurrentCollisions.RemoveAt(existingIndex);
                }
            }
            for (int j = 0; j < chunk.Objects.Count; j++)
            {
                if (i == j) continue;

                compare = chunk.Objects[j];
                CollisionData collisionData = current.CurrentCollisions.Find(d => d.Other.Equals(compare));
                int existingIndex = current.CurrentCollisions.IndexOf(collisionData);

                if ((current as ICollisionVolume).IsColliding(compare, ref collisionData))
                {
                    current.CurrentlyColliding = true;
                    
                    if (existingIndex == -1)
                        current.CurrentCollisions.Add(collisionData);
                    else
                        current.CurrentCollisions[existingIndex].Update(collisionData);
                }
                else
                {
                    if (existingIndex != -1)
                        current.CurrentCollisions.RemoveAt(existingIndex);                    
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
