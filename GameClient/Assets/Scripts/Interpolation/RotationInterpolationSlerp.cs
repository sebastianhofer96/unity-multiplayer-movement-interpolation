using System.Collections.Generic;
using UnityEngine;

public class RotationInterpolationSlerp : IRotationInterpolation
{
    public int InterpolationCount => 2;

    public Quaternion Interpolate(List<Quaternion> q, float t)
    {
        return Quaternion.Slerp(q[0], q[1], t);
    }
}