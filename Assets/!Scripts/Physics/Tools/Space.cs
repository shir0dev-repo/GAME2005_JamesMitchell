using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class Space<T> where T : MonoBehaviour, IPartitionable
{
    private Dictionary<Vector3Int, PartitionedSpace<T>> m_loadedPartitions;
    public Dictionary<Vector3Int, PartitionedSpace<T>> Partitions => m_loadedPartitions;
    
    private Vector3Int m_chunkSize;
    public Vector3Int ChunkSize => m_chunkSize;

    public Space(Vector3Int chunkSize)
    {
        m_chunkSize = chunkSize;
        m_loadedPartitions = new();
    }

    public void UpdatePartitions()
    {
        ConcurrentBag<(T, Vector3Int)> orphanedObjects = new();

        foreach (var part in Partitions)
        {
            Debug.Log("Updating chunk " + part.Key);
            part.Value.Update(orphanedObjects.Add);
            Debug.Log("Finished");
        }

        while (!orphanedObjects.IsEmpty)
        {
            if (orphanedObjects.TryTake(out (T obj, Vector3Int pos) tuple))
            {
                Debug.Log("Grabbed orphan " + tuple.obj.name + "with chunkID " + tuple.pos);
                AssignPartition(tuple);
            }
        }

        List<Vector3Int> emptyChunkPositions = new();
        
        foreach (var part in Partitions)
        {
            if (part.Value.IsEmpty)
                emptyChunkPositions.Add(part.Key);
        }

        foreach (Vector3Int chunkPos in emptyChunkPositions)
            m_loadedPartitions.Remove(chunkPos);
    }

    public void AssignPartition(T obj)
    {
        Vector3Int key = GetKey(obj.transform.position);
        if (!m_loadedPartitions.ContainsKey(key))
        {
            m_loadedPartitions.Add(key, new PartitionedSpace<T>(key, m_chunkSize));
            Debug.Log("Added chunk at " + key.ToString());
        }
        m_loadedPartitions[key].Add(obj);
    }
    public void AssignPartition((T obj, Vector3Int pos) tuple)
    {
        if (!m_loadedPartitions.ContainsKey(tuple.pos))
        {
            m_loadedPartitions.Add(tuple.pos, new PartitionedSpace<T>(tuple.pos, m_chunkSize));
            Debug.Log("created chunk with " + tuple.pos.ToString());
        }

        m_loadedPartitions[tuple.pos].Add(tuple.obj);
    }
    private Vector3Int GetKey(Vector3 position)
    {
        return Vector3Int.Scale(DivideAndFloor(position, m_chunkSize), m_chunkSize);
    }

    private static Vector3Int DivideAndFloor(Vector3 a, Vector3 b)
    {
        return new Vector3Int(
            Mathf.FloorToInt(a.x / b.x), 
            Mathf.FloorToInt(a.y / b.y), 
            Mathf.FloorToInt(a.z / b.z));
    }
}
