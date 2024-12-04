using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InjectedSystem<T> : Singleton<T> where T : MonoBehaviour
{
    protected override void Awake()
    {
        base.Awake();

        PhysicsBodyUpdateSystem.OnPreCollisionUpdate += OnPreCollisionUpdate;
        PhysicsBodyUpdateSystem.OnCollisionUpdate += OnCollisionUpdate;
        PhysicsBodyUpdateSystem.OnUnintersectionUpdate += OnUnintersectionUpdate;
        PhysicsBodyUpdateSystem.OnPostCollisionUpdate += OnPostCollisionUpdate;
    }

    protected virtual void OnPreCollisionUpdate() { }
    protected virtual void OnCollisionUpdate() { }
    protected virtual void OnUnintersectionUpdate() { }
    protected virtual void OnPostCollisionUpdate() { }

    protected override void OnApplicationQuit()
    {
        PhysicsBodyUpdateSystem.OnPreCollisionUpdate -= OnPreCollisionUpdate;
        PhysicsBodyUpdateSystem.OnCollisionUpdate -= OnCollisionUpdate;
        PhysicsBodyUpdateSystem.OnUnintersectionUpdate -= OnUnintersectionUpdate;
        PhysicsBodyUpdateSystem.OnPostCollisionUpdate -= OnPostCollisionUpdate;
        base.OnApplicationQuit();
    }
}
