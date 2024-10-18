using System;
using System.Collections.Generic;
using UnityEngine;

public class Partition<T> where T : IPartitionable
{
    private List<T> m_objects = new List<T>();
    public List<T> Objects => m_objects;
    public bool IsEmpty => m_objects.Count == 0;

    private Vector3Int m_chunkPosition;
    private static Vector3Int s_chunkSize = Vector3Int.zero;
    public Partition(Vector3Int chunkPosition, Vector3Int chunkSize)
    {
        m_chunkPosition = chunkPosition;

        if (s_chunkSize == Vector3Int.zero)
            s_chunkSize = chunkSize;
    }

    public void Add(T obj)
    {
        if (!m_objects.Contains(obj))
            m_objects.Add(obj);
    }

    public void Remove(T obj)
    {
        m_objects.Remove(obj);
    }

    public void Update(Action<(T, Vector3Int)> addToBag)
    {
        for (int i = 0; i < m_objects.Count; i++)
        {
            T obj = m_objects[i];
            if (!IsInsideChunk(obj.Transform.position, out Vector3Int pos))
            {
                addToBag((obj, pos));
                Remove(obj);
            }
        }
    }

    private bool IsInsideChunk(Vector3 position, out Vector3Int flooredPos)
    {
        flooredPos = Vector3Int.Scale(new Vector3Int(
            Mathf.FloorToInt(position.x / s_chunkSize.x),
            Mathf.FloorToInt(position.y / s_chunkSize.y),
            Mathf.FloorToInt(position.z / s_chunkSize.z)),
            s_chunkSize);

        return flooredPos == m_chunkPosition;
    }

}
