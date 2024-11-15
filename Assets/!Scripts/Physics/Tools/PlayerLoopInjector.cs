using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.LowLevel;

public abstract class PlayerLoopInjector
{
    public enum InsertType { Before, After }

    private static List<PlayerLoopSystem> m_currentlyInjectedEvents = new();

    [RuntimeInitializeOnLoadMethod]
    private static void AppStart()
    {
        Application.quitting += RestorePlayerLoopSystemState;       
    }

    private static void RestorePlayerLoopSystemState()
    {
        foreach (var playerLoopSystem in m_currentlyInjectedEvents)
            TryRemoveSystem(playerLoopSystem.type);

        m_currentlyInjectedEvents.Clear();
        Application.quitting -= RestorePlayerLoopSystemState;
    }

    public static void InsertSystemBefore(Type newSystemMarker, PlayerLoopSystem.UpdateFunction newSystemUpdate, Type insertBefore)
    {
        var playerLoopSystem = new PlayerLoopSystem { type = newSystemMarker, updateDelegate = newSystemUpdate };
        InsertSystemBefore(playerLoopSystem, insertBefore);
    }

    public static void InsertSystemBefore(PlayerLoopSystem toInsert, Type insertBefore)
    {
        if (toInsert.type == null)
            throw new ArgumentException("Inserted player loop has null marker type!", nameof(toInsert.type));
        if (toInsert.updateDelegate == null)
            throw new ArgumentException("Inserted player loop must have update delegate!", nameof(toInsert.updateDelegate));
        if (insertBefore == null)
            throw new ArgumentException(nameof(insertBefore));

        PlayerLoopSystem rootSystem = PlayerLoop.GetCurrentPlayerLoop();
        InsertSystem(ref rootSystem, toInsert, insertBefore, InsertType.Before, out bool couldInsert);
        if (!couldInsert)
        {
            throw new ArgumentException(
                $"Failed to insert the type {toInsert.type.Name} into the player loop before {insertBefore.Name}. " +
                $"{insertBefore.Name} could not be found in the current player loop!");
        }

        m_currentlyInjectedEvents.Add(toInsert);
        PlayerLoop.SetPlayerLoop(rootSystem);
    }

    public static void InsertSystemAfter(Type newSystemMarker, PlayerLoopSystem.UpdateFunction newSystemUpdate, Type insertAfter)
    {
        PlayerLoopSystem playerLoopSystem = new PlayerLoopSystem { type = newSystemMarker, updateDelegate = newSystemUpdate };
        InsertSystemAfter(playerLoopSystem, insertAfter);
    }

    public static void InsertSystemAfter(PlayerLoopSystem toInsert, Type insertAfter) 
    {
        if (toInsert.type == null)
            throw new ArgumentException("Inserted player loop has null marker type!", nameof(toInsert.type));
        if (toInsert.updateDelegate == null)
            throw new ArgumentException("Inserted player loop must have update delegate!", nameof(toInsert.updateDelegate));
        if (insertAfter == null)
            throw new ArgumentException(nameof(insertAfter));

        PlayerLoopSystem rootSystem = PlayerLoop.GetCurrentPlayerLoop();
        
        InsertSystem(ref rootSystem, toInsert, insertAfter, InsertType.After, out bool couldInsert);
        if (!couldInsert)
        {
            throw new ArgumentException(
                $"Failed to insert the type {toInsert.type.Name} into the player loop before {insertAfter.Name}. " +
                $"{insertAfter.Name} could not be found in the current player loop!");
        }

        m_currentlyInjectedEvents.Add(toInsert);
        PlayerLoop.SetPlayerLoop(rootSystem);
    }

    public static bool TryRemoveSystem(Type type)
    {
        if (type == null)
            throw new ArgumentNullException(nameof(type), "Can't remove a null type!");

        var currentSystem = PlayerLoop.GetCurrentPlayerLoop();
        bool couldRemove = TryRemoveTypeFrom(ref currentSystem, type);
        PlayerLoop.SetPlayerLoop(currentSystem);
        
        return couldRemove;
    }

    private static bool TryRemoveTypeFrom(ref PlayerLoopSystem currentSystem, Type type)
    {
        var subSystems = currentSystem.subSystemList;
        if (subSystems == null)
            return false;

        for (int i = 0; i < subSystems.Length; i++)
        {
            if (subSystems[i].type == type)
            {
                var newSubSystems = new PlayerLoopSystem[subSystems.Length - 1];

                Array.Copy(subSystems, newSubSystems, i);
                Array.Copy(subSystems, i + 1, newSubSystems, i, subSystems.Length - i - 1);

                currentSystem.subSystemList = newSubSystems;

                return true;
            }

            if (TryRemoveTypeFrom(ref subSystems[i], type))
                return true;
        }

        return false;
    }

    private static void InsertSystem(ref PlayerLoopSystem currentLoopRecursive, PlayerLoopSystem toInsert, Type insertTarget, InsertType insertType, out bool couldInsert)
    {
        PlayerLoopSystem[] currentSubSystems = currentLoopRecursive.subSystemList;
        if (currentSubSystems == null)
        {
            couldInsert = false;
            return;
        }

        int targetIndex = -1;
        for (int i = 0; i < currentSubSystems.Length; i++)
        {
            if (currentSubSystems[i].type == insertTarget)
            {
                targetIndex = i;
                break;
            }
        }

        if (targetIndex != -1)
        {
            PlayerLoopSystem[] newSubSystems = new PlayerLoopSystem[currentSubSystems.Length + 1];
            int insertIndex = insertType == InsertType.Before ? targetIndex : targetIndex + 1;

            for (int i = 0; i < newSubSystems.Length; i++)
            {
                if (i < insertIndex)
                    newSubSystems[i] = currentSubSystems[i];
                else if (i == insertIndex)
                    newSubSystems[i] = toInsert;
                else
                    newSubSystems[i] = currentSubSystems[i - 1];
            }

            couldInsert = true;
            currentLoopRecursive.subSystemList = newSubSystems;
        }
        else
        {
            for (int i = 0; i < currentSubSystems.Length; i++)
            {
                PlayerLoopSystem subSystem = currentSubSystems[i];
                InsertSystem(ref subSystem, toInsert, insertTarget, insertType, out bool couldInsertInner);
                if (couldInsertInner)
                {
                    currentSubSystems[i] = subSystem;
                    couldInsert = true;
                    return;
                }
            }

            couldInsert = false;
        }
    }

    public static string CurrentLoopToString()
    {
        return PrintSystemToString(PlayerLoop.GetCurrentPlayerLoop());
    }

    private static string PrintSystemToString(PlayerLoopSystem s)
    {
        List<(PlayerLoopSystem, int)> systems = new();

        AddRecursively(s, 0);
        void AddRecursively(PlayerLoopSystem system, int depth)
        {
            systems.Add((system, depth));
            if (system.subSystemList != null)
            {
                foreach (var subSystem in system.subSystemList)
                    AddRecursively(subSystem, depth + 1);
            }
        }

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("Systems").AppendLine("=====");
        foreach (var (system, depth) in systems)
        {
            Append($"SystemType: {system.type?.Name ?? "NULL"}");

            Append($"Delegate: {system.updateDelegate}");

            Append($"Update Function: {system.updateFunction}");

            Append($"Loop Condition Function: {system.loopConditionFunction}");

            Append($"{system.subSystemList?.Length ?? 0} subsystems");

            void Append(string s)
            {
                for (int i = 0; i < depth; i++)
                {
                    sb.Append("  ");
                }
                
                sb.AppendLine(s);
            }
        }

        return sb.ToString();
    }

    public static void OutputCurrentStateToFile()
    {
        string str = PrintSystemToString(PlayerLoop.GetCurrentPlayerLoop());
        File.WriteAllText(Path.Combine(Application.persistentDataPath, "playerLoopOutput.txt"), str);
    }
}
