using System.Collections.Generic;
using UnityEngine;

public interface IPhysicsVolume
{
    public Vector3 Center { get; } 
    public Quaternion Rotation { get; }

    public bool CurrentlyColliding { get; set; }
    public Vector3 CurrentPartitionOrigin { get; set; }

    /// <summary>
    ///     Calculates the cross sectional area of a shape perpendicular to <paramref name="inNormal"/>.
    /// </summary>
    /// <param name="inNormal">The normal of the cross section</param>
    /// <returns>
    ///     The area formed by the cross sectional projection of the shape onto the plane
    ///     defined by <paramref name="inNormal"/>.
    /// </returns>
    float CrossSectionalArea(Vector3 inNormal);
}
