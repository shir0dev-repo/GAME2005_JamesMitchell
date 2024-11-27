using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.LowLevel;

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

    [RuntimeInitializeOnLoadMethod]
    private static void Initialize()
    {
        PlayerLoopSystem playerLoopSystem = new PlayerLoopSystem
        {
            type = typeof(PhysicsBodyUpdateSystem),
            updateDelegate = UpdateInjected,
            subSystemList = new PlayerLoopSystem[]
            {
                
                // PhysicsManagerUpdate
                new PlayerLoopSystem
                {
                    type = typeof(PhysicsManager),
                    updateDelegate = GetSubsystemUpdateFunction(typeof(PhysicsManager))
                },
                // CollisionManagerUpdate
                new PlayerLoopSystem
                {
                    type = typeof(CollisionManager),
                    updateDelegate = GetSubsystemUpdateFunction(typeof(CollisionManager))
                },                
            }
        };

        PlayerLoopInjector.InsertSystemAfter(playerLoopSystem, typeof(UnityEngine.PlayerLoop.TimeUpdate.WaitForLastPresentationAndUpdateTime));
    }

    private static void UpdateInjected()
    {
        m_timer += Time.deltaTime;
        if (m_timer >= TimeStep)
        {
            m_timer -= TimeStep;
            OnMarkForUpdate?.Invoke();
        }
    }

    private static PlayerLoopSystem.UpdateFunction GetSubsystemUpdateFunction(Type friend)
    {
        string methodName = friend.Name + "UpdateInjected";
        return GetSubsystemUpdateFunction(friend, methodName);
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
