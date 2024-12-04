using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="PhysicsEngine/Physics Material Database")]
public class PhysicsMaterialDatabase : SODatabase.ScriptableObjectDatabase<PhysicsMaterial>
{
    [ContextMenu("Find All")]
    public override void Find()
    {
        FindAll();
    }
}
