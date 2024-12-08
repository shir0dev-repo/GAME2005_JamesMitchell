using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VectorUtil 
{
    public static Vector3 Absolute(this Vector3 v)
    {
        Vector3 result = Vector3.zero;
        for (int i = 0; i < 3; i++)
        {
            result[i] = Mathf.Abs(v[i]);
        }

        return result;
    }
}
