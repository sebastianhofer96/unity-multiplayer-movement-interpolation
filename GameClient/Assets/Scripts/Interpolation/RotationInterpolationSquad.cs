using System.Collections.Generic;
using UnityEngine;

// https://github.com/leixingyu/unity-interpolation-system/blob/master/Assets/Script/Squad.cs
// https://github.com/leixingyu/unity-interpolation-system/blob/master/Assets/Script/MathFunc.cs

public class RotationInterpolationSquad : IRotationInterpolation
{
    public int InterpolationCount => 4;

    public Quaternion Interpolate(List<Quaternion> q, float t)
    {
        // calculate intermediates
        Quaternion s0 = Intermediate(q[0], q[1], q[2]);
        Quaternion s1 = Intermediate(q[1], q[2], q[3]);

        // calculate inner slerps
        Quaternion slerp0 = Quaternion.Slerp(q[1], q[2], t);
        Quaternion slerp1 = Quaternion.Slerp(s0, s1, t);

        float slerpT = 2.0f * t * (1.0f - t);
        return Quaternion.Slerp(slerp0, slerp1, slerpT);
    }

    private Quaternion Intermediate(Quaternion q0, Quaternion q1, Quaternion q2)
    {
        Quaternion q1inv = Quaternion.Inverse(q1);
        Quaternion c1 = q1inv * q2;
        Quaternion c2 = q1inv * q0;
        Log(ref c1);
        Log(ref c2);
        Quaternion c3 = Add(c2, c1);
        Scale(ref c3, -0.25f);
        Exp(ref c3);
        Quaternion r = q1 * c3;
        r.Normalize();
        return r;
    }

    private void Log(ref Quaternion a)
    {
        float a0 = a.w;
        a.w = 0.0f;
        if (Mathf.Abs(a0) < 1.0f)
        {
            float angle = Mathf.Acos(a0);
            float sinAngle = Mathf.Sin(angle);
            if (Mathf.Abs(sinAngle) >= 1.0e-15)
            {
                float coeff = angle / sinAngle;
                a.x *= coeff;
                a.y *= coeff;
                a.z *= coeff;
            }
        }
    }

    private void Exp(ref Quaternion a)
    {
        float angle = Mathf.Sqrt(a.x * a.x + a.y * a.y + a.z * a.z);

        float sinAngle = Mathf.Sin(angle);
        a.w = Mathf.Cos(angle);

        if (Mathf.Abs(sinAngle) >= 1.0e-15)
        {
            float coeff = sinAngle / angle;
            a.x *= coeff;
            a.y *= coeff;
            a.z *= coeff;
        }
    }

    private Quaternion Add(Quaternion a, Quaternion b)
    {
        var r = new Quaternion();
        r.w = a.w + b.w;
        r.x = a.x + b.x;
        r.y = a.y + b.y;
        r.z = a.z + b.z;
        return r;
    }

    private void Scale(ref Quaternion a, float s)
    {
        a.w *= s;
        a.x *= s;
        a.y *= s;
        a.z *= s;
    }
}