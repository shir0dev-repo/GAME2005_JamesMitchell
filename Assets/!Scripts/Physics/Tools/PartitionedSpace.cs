using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class PartitionedSpace<T> where T : MonoBehaviour, IPartitionable
{
    private List<T> m_objects = new List<T>();

    public void Add(T obj)
    {
        if (m_objects.Contains(obj)) return;
        m_objects.Add(obj);
    }
}
