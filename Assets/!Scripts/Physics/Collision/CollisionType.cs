using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Flags]
public enum CollisionType 
{
    NONE = 0,
    Sphere = 1, 
    Cube = 2,
    Plane = 4,
    Halfspace = 8
}

public static class CollisionCalculator
{

}