using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="PhysicsEngine/Physics Material")]
public class PhysicsMaterial : SODatabase.DatabaseObject
{
    public float Bounciness() => m_restitution;
    [SerializeField, Range(0, 1)] private float m_restitution;

    public float FrictionThreshold() => m_staticFrictionThreshold;
    [SerializeField, Range(0, 1)] private float m_staticFrictionThreshold;
    
    public float Roughness() => m_kineticFrictionCoefficient;
    [SerializeField, Range(0, 1)] private float m_kineticFrictionCoefficient;

    public float Density() => m_density;
    [SerializeField, Min(0.00016f)] private float m_density = 1f; // 0.00016 g/m^3 is the density of aerographene, the lightest material on earth
    public Material RenderMaterial() => m_material;
    [SerializeField] private Material m_material;
}
