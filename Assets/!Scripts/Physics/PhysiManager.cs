using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysiManager : Singleton<PhysiManager>
{
    [SerializeField] private Vector3 m_gravity = Vector3.down * 9.81f;
    public static Vector3 Gravity => Instance.m_gravity;

    [SerializeField] private float m_deltaTime = 0.02f;
    public static float DeltaTime => Instance.m_deltaTime;
}
