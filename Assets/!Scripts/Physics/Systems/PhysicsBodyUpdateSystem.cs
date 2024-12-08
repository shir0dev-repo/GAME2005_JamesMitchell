using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.LowLevel;

public class PhysicsSystemInjected { }

public class PhysicsBodyUpdateSystem
{
    public const float TimeStep = 0.02f;
    private static float m_timer = 0;

    private static readonly Type[] m_friendsList = new Type[]
    {
        typeof(PhysicsManager),
        typeof(CollisionManager)
    };

    public static Action OnMarkForUpdate;
    public static Action OnPreCollisionUpdate;
    public static Action OnCollisionUpdate;
    public static Action OnUnintersectionUpdate;
    public static Action OnPostCollisionUpdate;

    [RuntimeInitializeOnLoadMethod]
    private static void Initialize()
    {
        PlayerLoopSystem playerLoopSystem = new PlayerLoopSystem
        {
            type = typeof(PhysicsSystemInjected),
            updateDelegate = PhysicsUpdateInjected
        };

        PlayerLoopInjector.InsertSystemAfter(playerLoopSystem, typeof(UnityEngine.PlayerLoop.TimeUpdate.WaitForLastPresentationAndUpdateTime));
    }

    private static void PhysicsUpdateInjected()
    {
        /*
        Desired Order:
           - PhysicsManager    - Move all bodies
           - CollisionManager  - Check for overlaps
           - PhysicsManager    - Unintersect all bodies
           - PhysicsManager    - Adjust velocities based on collisions
        */
        m_timer += Time.deltaTime;

        if (m_timer >= TimeStep)
        {
            m_timer -= TimeStep;
            OnPreCollisionUpdate?.Invoke();
            OnCollisionUpdate?.Invoke();
            OnUnintersectionUpdate?.Invoke();
            OnPostCollisionUpdate?.Invoke();
        }
    }

    private static PlayerLoopSystem.UpdateFunction GetSubsystemUpdateFunction(Type friend, string methodName)
    {
        bool validType = false;
        foreach (Type type in m_friendsList)
        {
            if (type == friend)
            {
                validType = true;
                break;
            }
        }

        if (!validType)
            throw new ArgumentException($"{friend.Name} is not allowed to be a part of the {typeof(PhysicsBodyUpdateSystem).Name}!");

        MethodInfo subsystemUpdateMethod = friend.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static);

        if (subsystemUpdateMethod == null)
        {
            throw new MethodAccessException(
                $"{typeof(PhysicsBodyUpdateSystem).Name} could not find the specified method {methodName} " +
                $"in {friend.Name}. Make sure the method signature matches: \"private static void {friend.Name}UpdateInjected()\"."
            );
        }

        return (PlayerLoopSystem.UpdateFunction)subsystemUpdateMethod.CreateDelegate(typeof(PlayerLoopSystem.UpdateFunction));
    }
}
