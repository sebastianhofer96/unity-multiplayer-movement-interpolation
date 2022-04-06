using System.Collections.Generic;
using UnityEngine;

public interface IPlayerInputSource
{
    public PlayerIntention SampleIntention(long tick);
}

public interface IPositionInterpolation
{
    public int InterpolationCount { get; }
    public Vector3 Interpolate(List<Vector3> v, float t);
}

public interface IRotationInterpolation
{
    public int InterpolationCount { get; }
    public Quaternion Interpolate(List<Quaternion> q, float t);
}