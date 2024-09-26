using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ShapeType { Box, Sphere }

[System.Serializable]
public abstract class Shape
{
    public abstract bool CheckCollisions();
}
