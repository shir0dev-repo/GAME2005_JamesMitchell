using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpatialPartition<T> where T : MonoBehaviour, IPartitionable
{
    private Vector3Int m_chunkSize;
    private Dictionary<Vector3, PartitionedSpace<T>> m_loadedPartitions;
    public SpatialPartition(Vector3Int chunkSize)
    {
        m_chunkSize = chunkSize;
        m_loadedPartitions = new();
    }

    public List<PartitionedSpace<T>> Partitions => new List<PartitionedSpace<T>>(m_loadedPartitions.Values);

    public void AssignPartitions(T obj)
    {
        Vector3 pos = obj.transform.position;
        Vector3 key = new()
        {
            x = m_chunkSize.x % pos.x,
            y = m_chunkSize.y % pos.y,
            z = m_chunkSize.z % pos.z
        };

        if (!m_loadedPartitions.ContainsKey(key))
            m_loadedPartitions.Add(key, new PartitionedSpace<T>());

        m_loadedPartitions[key].Add(obj);
    }

}
