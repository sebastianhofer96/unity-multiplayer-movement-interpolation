using System.Collections.Generic;
using UnityEngine;

public class PositionInterpolationLinear : IPositionInterpolation
{
    public int InterpolationCount => 2;

    public Vector3 Interpolate(List<Vector3> v, float t)
    {
        return v[0] * (1.0f - t) + v[1] * t;
    }
}