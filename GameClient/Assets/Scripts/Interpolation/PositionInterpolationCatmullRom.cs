using System.Collections.Generic;
using UnityEngine;

// https://stackoverflow.com/questions/50655395/adding-alpha-to-catmull-rom

public class PositionInterpolationCatmullRom : IPositionInterpolation
{
    private readonly float ALPHA = 0.5f;

    public int InterpolationCount => 4;

    public Vector3 Interpolate(List<Vector3> v, float t)
    {
        // calculate dt depending on alpha
        float dt0 = GetTime(v[0], v[1], ALPHA);
        float dt1 = GetTime(v[1], v[2], ALPHA);
        float dt2 = GetTime(v[2], v[3], ALPHA);

        // safety check for repeated points
        if (dt1 < 1e-4f) dt1 = 1.0f;
        if (dt0 < 1e-4f) dt0 = dt1;
        if (dt2 < 1e-4f) dt2 = dt1;

        // compute tangents when parameterized in [t1,t2]
        Vector3 t1 = ((v[1] - v[0]) / dt0) - ((v[2] - v[0]) / (dt0 + dt1)) + ((v[2] - v[1]) / dt1);
        Vector3 t2 = ((v[2] - v[1]) / dt1) - ((v[3] - v[1]) / (dt1 + dt2)) + ((v[3] - v[2]) / dt2);

        // rescale tangents for parametrization in [0,1]
        t1 *= dt1;
        t2 *= dt1;

        // compute coefficients for a cubic polynomial
        Vector3 c0 = v[1];
        Vector3 c1 = t1;
        Vector3 c2 = (3 * v[2]) - (3 * v[1]) - (2 * t1) - t2;
        Vector3 c3 = (2 * v[1]) - (2 * v[2]) + t1 + t2;

        // evaluate position
        Vector3 pos = CalculatePosition(t, c0, c1, c2, c3);

        return pos;
    }

    private float GetTime(Vector3 v0, Vector3 v1, float alpha)
    {
        return Mathf.Pow((v1 - v0).sqrMagnitude, 0.5f * alpha);
    }

    private Vector3 CalculatePosition(float t, Vector3 c0, Vector3 c1, Vector3 c2, Vector3 c3)
    {
        float t2 = t * t;
        float t3 = t2 * t;

        return c0 + c1 * t + c2 * t2 + c3 * t3;
    }

    // without alpha
    //public Vector3 Interpolate(List<Vector3> v, float t)
    //{
    //    float t2 = t * t;
    //    float t3 = t2 * t;
    //
    //    float f1 = -t3 + 2.0f * t2 - t;
    //    float f2 = 3.0f * t3 - 5.0f * t2 + 2.0f;
    //    float f3 = -3.0f * t3 + 4.0f * t2 + t;
    //    float f4 = t3 - t2;
    //
    //    return (f1 * v[0] + f2 * v[1] + f3 * v[2] + f4 * v[3]) / 2.0f;
    //}
}