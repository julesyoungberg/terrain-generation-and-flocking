using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatmulSpline 
{
    private Vector3[] cps;
    private Vector3[] tangents;
    private float parametric_range;

    private Vector4 h_function(float t)
    {
        float t2 = t * t;
        float t3 = t2 * t;
        float hx = 2 * t3 - 3 * t2 + 1;
        float hy = t3 - 2 * t2 + t;
        float hz = -2 * t3 + 3 * t2;
        float hw = t3 - t2;

        return new Vector4(hx, hy, hz, hw);
    }

    public Vector3[] control_points
    {
        set
        {
            cps = value;
            parametric_range = cps.Length - 3;
            tangents = new Vector3[cps.Length - 2];
            for (int i = 0; i < tangents.Length; i++)
            {
                tangents[i] = (cps[i + 2] - cps[i]) / 2.0f;
            }
        }
    }

    public Vector3 Sample(float t)
    {
        if (t > 0 && t < parametric_range)
        {
            int knot_prev = Mathf.FloorToInt(t);
            float local_param = t - (float)(knot_prev);
            Vector4 h = h_function(local_param);
            Vector3 p0 = cps[knot_prev + 1];
            Vector3 p1 = cps[knot_prev + 2];
            Vector3 m0 = tangents[knot_prev];
            Vector3 m1 = tangents[knot_prev + 1];
            return h.x * p0 + h.y * m0 + h.z * p1 + h.w * m1;
        }
        else
        {
            return Vector3.zero;
        }
    }
}
