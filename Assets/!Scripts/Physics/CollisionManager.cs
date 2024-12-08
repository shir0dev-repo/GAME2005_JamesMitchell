using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CollisionManager : InjectedSystem<CollisionManager>
{
    private static PartitionedSpace<CollisionComponent> m_space;
    [SerializeField] private Vector3Int m_chunkSize = Vector3Int.one * 16;

    [SerializeField] private List<CollisionData> m_collisions = new();
    private static readonly List<CollisionComponent> m_planesAndHalfspaces = new();
    private static readonly Dictionary<(long focusedHashCode, long otherHashCode), CollisionData> m_collisionsThisFrame = new();
    
    public PhysicsMaterialDatabase MaterialDatabase => m_materialDB;
    [SerializeField] private PhysicsMaterialDatabase m_materialDB;

    [SerializeField] private bool m_drawChunks = false;
    protected override void OnCollisionUpdate()
    {
        m_space.UpdatePartitions();
    }

    protected override void OnUnintersectionUpdate()
    {
        ResolveCollisions();
    }
    
    protected override void Awake()
    {
        base.Awake();
        m_space = new PartitionedSpace<CollisionComponent>(m_chunkSize, CalculateCollisionsPerChunk);
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
            if (m_space.Partitions.TryGetValue(key, out Partition<CollisionComponent> currentSpace))
            {
                currentSpace.Remove(collider);
            }
        }
    }
    
    private void CalculateCollisionsPerChunk(Partition<CollisionComponent> centerChunk)
    {
        Partition<CollisionComponent>[] neighbours = m_space.GetNeighbours(centerChunk, true);

        // for each object in chunk
            // check for collision
                // if not contains
                    // enqueue collision data 
                // else
                    // continue
            // for each plane
            // foreach neighbour chunk
                // for each object in neighbour chunk

        // iterate through this chunk [O(N)]
        for (int i = 0; i < centerChunk.Objects.Count; i++)
        {             
            CollisionComponent current = centerChunk.Objects[i];

            foreach (CollisionComponent planarCollider in m_planesAndHalfspaces)
            {
                long aCode = current.GetHashCode();
                long bCode = planarCollider.GetHashCode();
                // same objects
                if (aCode == bCode) continue;

                (long a, long b) key;

                // always use larger key as initial value, for a pseudo sort
                if (aCode > bCode)
                    key = (aCode, bCode);
                else
                    key = (bCode, aCode);

                bool exists = m_collisionsThisFrame.TryGetValue(key, out CollisionData data);

                if (!exists)
                    data = new CollisionData();

                if (Collisions.IsColliding(current, planarCollider, data))
                {
                    current.CurrentlyColliding = planarCollider.CurrentlyColliding = true;
                    if (exists)
                    {
                        data.TimeSinceCollisionStart += PhysicsBodyUpdateSystem.TimeStep;
                        m_collisionsThisFrame[key] = data;
                    }
                    else
                    {
                        m_collisionsThisFrame.Add(key, data);
                    }
                }
                // collision didn't happen, remove if existing
                else if (exists)
                    m_collisionsThisFrame.Remove(key);
            }

            // for each active chunk surrounding this one [O(C)]
            foreach (Partition<CollisionComponent> chunk in neighbours)
            {
                // for each object inside chunk [O(J)]
                for (int j = 0; j < chunk.Objects.Count; j++)
                {
                    CollisionComponent other = chunk.Objects[j];
                    long aCode = current.GetHashCode();
                    long bCode = other.GetHashCode();
                    // same objects
                    if (aCode == bCode) continue;

                    (long a, long b) key;

                    // always use larger key as initial value, for a pseudo sort
                    if (aCode > bCode)
                        key = (aCode, bCode);
                    else 
                        key = (bCode, aCode);

                    // try find existing collision data, to keep track of continuous collisions
                    bool exists = m_collisionsThisFrame.TryGetValue(key, out CollisionData data);

                    if (!exists)
                        data = new CollisionData();

                    // IsResolved will be set to false every collision check
                    // therefore, finding a not null, unresolved collision within the table means we've already calculated this interaction
                    // this also means that IsResolved must not be changed between the time a collision is resolved, to the next time it is checked
                    //if (data != null && data.IsResolved == false) continue;
                    

                    if (Collisions.IsColliding(current, other, data))
                    {
                        current.CurrentlyColliding = other.CurrentlyColliding = true;
                        if (exists)
                        {
                            data.TimeSinceCollisionStart += PhysicsBodyUpdateSystem.TimeStep;
                            m_collisionsThisFrame[key] = data;
                        }
                        else
                        {
                            m_collisionsThisFrame.Add(key, data);
                        }
                    }
                    // collision didn't happen, remove if existing
                    else if (exists)
                        m_collisionsThisFrame.Remove(key);
                }
            }
        }

        
        //for (int i = 0; i < chunk.Objects.Count; i++)
        //{
        //    current = chunk.Objects[i];
        //
        //    foreach (CollisionComponent planarVolume in m_planesAndHalfspaces)
        //    {
        //        CollisionData collisionData = current.CurrentCollisions.Find(d => d.Other.GetInstanceID() == planarVolume.GetInstanceID());
        //        int existingIndex;
        //        if (collisionData == null)
        //        {
        //            collisionData = new();
        //            existingIndex = -1;
        //        }
        //        else
        //        {
        //            existingIndex = current.CurrentCollisions.IndexOf(collisionData);
        //        }
        //
        //        if ((current as ICollisionVolume).IsColliding(planarVolume, ref collisionData))
        //        {
        //            current.CurrentlyColliding = true;
        //
        //            if (existingIndex == -1)
        //                current.CurrentCollisions.Add(collisionData);
        //            else
        //            {
        //                current.CurrentCollisions[existingIndex].Update(collisionData);
        //                collisionData.TimeSinceCollisionStart += PhysicsBodyUpdateSystem.TimeStep;
        //            }
        //            
        //        }
        //        else if(existingIndex != -1)
        //        {
        //            current.CurrentCollisions.RemoveAt(existingIndex);
        //        }
        //    }
        //
        //    foreach (Partition<CollisionComponent> neighbour in neighbours)
        //    {
        //        for (int j = 0; j < neighbour.Objects.Count; j++)
        //        {
        //            if (i == j && neighbour == chunk) continue;
        //
        //            compare = neighbour.Objects[j];
        //
        //            CollisionData collisionData = current.CurrentCollisions.Find(d => d.Other.GetInstanceID() == compare.GetInstanceID());
        //            int existingIndex;
        //            if (collisionData == null)
        //            {
        //                collisionData = new();
        //                existingIndex = -1;
        //            }
        //            else
        //            {
        //                existingIndex = current.CurrentCollisions.IndexOf(collisionData);
        //            }
        //
        //            if ((current as ICollisionVolume).IsColliding(compare, ref collisionData))
        //            {
        //                current.CurrentlyColliding = true;
        //
        //                if (existingIndex == -1)
        //                    current.CurrentCollisions.Add(collisionData);
        //                else
        //                    current.CurrentCollisions[existingIndex].Update(collisionData);
        //            }
        //            else
        //            {
        //                if (existingIndex != -1)
        //                    current.CurrentCollisions.RemoveAt(existingIndex);
        //            }
        //        }
        //    }
        //}
    }

    private static void ResolveCollisions()
    {
        Instance.m_collisions = m_collisionsThisFrame.Values.ToList();

        Queue<CollisionData> toResolve = new Queue<CollisionData>(m_collisionsThisFrame.Values);

        while (toResolve.TryDequeue(out CollisionData data) && data.IsResolved == false)
        {
            Collisions.GetResponse(data);
        }
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
