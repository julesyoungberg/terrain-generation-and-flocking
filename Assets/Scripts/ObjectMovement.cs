using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectMovement : MonoBehaviour
{
    [Range(0.001f, 1f)]
    public float speed = 0.1f;

    private float length;

    private float current_t;

    private CatmulSpline spline;

    private void Start()
    {
        spline = new CatmulSpline();
        Vector3[] cps = new Vector3[6];
        cps[0] = new Vector3(60, 90, 60);
        cps[1] = new Vector3(100, 130, 300);
        cps[2] = new Vector3(300, 170, 150);
        cps[3] = cps[0];
        cps[4] = cps[1];
        cps[5] = cps[2];
        length = cps.Length / 2;
        spline.control_points = cps;
        current_t = 0;
    }

    private void Update()
    {
        current_t += speed;
        if (current_t > length) current_t -= length;

        Vector3 curr_pos = EvaluateAt(current_t);
        Vector3 next_pos = EvaluateAt(current_t + speed);

        gameObject.transform.position = curr_pos;

        Vector3 forward = next_pos - curr_pos;
        forward.Normalize();
        gameObject.transform.forward = forward;
        gameObject.transform.Rotate(new Vector3(0, 90, 90));
    }

    Vector3 EvaluateAt(float t)
    {
        if (t > length) t -= length;
        return spline.Sample(t);
    }
}
