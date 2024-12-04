using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="PhysicsEngine/Physics Material")]
public class PhysicsMaterial : SODatabase.DatabaseObject
{
    [Range(0, 1)] public float Restitution;
    [Range(0, 1)] public float StaticFrictionThreshold;
    [Range(0, 1)] public float KineticFrictionCoefficient;
}
