using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

public class PartitionedSpace<T> where T : IPartitionable
{
    public Dictionary<Vector3Int, Partition<T>> Partitions => m_loadedPartitions;
    private Dictionary<Vector3Int, Partition<T>> m_loadedPartitions;
    
    public Vector3Int ChunkSize => m_chunkSize;
    private Vector3Int m_chunkSize;

    public delegate void PartitionProcess(Partition<T> part);
    private PartitionProcess m_partitionProcess;

    public PartitionedSpace(Vector3Int chunkSize)
    {
        m_chunkSize = chunkSize;
        m_loadedPartitions = new();
    }

    public PartitionedSpace(Vector3Int chunkSize, PartitionProcess perChunkCalculation)
    {
        m_chunkSize = chunkSize;
        m_loadedPartitions = new();
        m_partitionProcess = perChunkCalculation;
    }

    public void UpdatePartitions()
    {
        ConcurrentBag<(T, Vector3Int)> orphanedObjects = new();

        foreach (var part in Partitions)
        {
            part.Value.Update(orphanedObjects.Add);
        }

        while (!orphanedObjects.IsEmpty)
        {
            if (orphanedObjects.TryTake(out (T obj, Vector3Int pos) tuple))
            {
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

        //objects are now inside their corresponding chunk, free to do actual process
        foreach (var part in Partitions)
        {
            m_partitionProcess?.Invoke(part.Value);
        }
    }

    public void SetPerChunkCalculation(PartitionProcess chunkAction)
    {
        m_partitionProcess = chunkAction as PartitionProcess;
    }

    public void AssignPartition(T obj)
    {
        Vector3Int key = GetKey(obj.Transform.position);
        if (!m_loadedPartitions.ContainsKey(key))
        {
            m_loadedPartitions.Add(key, new Partition<T>(key, m_chunkSize));
        }
        m_loadedPartitions[key].Add(obj);
    }
    public void AssignPartition((T obj, Vector3Int pos) tuple)
    {
        if (!m_loadedPartitions.ContainsKey(tuple.pos))
        {
            m_loadedPartitions.Add(tuple.pos, new Partition<T>(tuple.pos, m_chunkSize));
        }

        m_loadedPartitions[tuple.pos].Add(tuple.obj);
    }
    public Vector3Int GetKey(Vector3 position)
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
