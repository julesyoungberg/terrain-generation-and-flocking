using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BezierFollow : MonoBehaviour
{
    [SerializeField]
    private Transform[] routes;

    private int routeToGo;

    private float tParam;

    private Vector3 position, prevPosition;

    [Range(0.1f, 5f)]
    public float speed = 0.5f;

    [Range(0.1f, 1f)]
    public float distanceToTravel = 0.5f;

    private bool coroutineAllowed;

    private void Start()
    {
        routeToGo = 0;
        tParam = 0f;
        coroutineAllowed = true;
    }

    private void Update()
    {
        // check if coroutinne is already running
        if (coroutineAllowed)
        {
            StartCoroutine(GoByTheRoute(routeToGo));
        }
        // update forward if a non zero prev position exists
        if (prevPosition != Vector3.zero)
        {
            transform.forward = (position - prevPosition).normalized;
            transform.Rotate(new Vector3(0, 90, 90));
        }
        prevPosition = position;
    }

    // Position based on cubic bezier with given control points
    private Vector3 Position(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        // calculate values for each basis function, then return sum
        Vector3 a = Mathf.Pow(1 - t, 3) * p0;
        Vector3 b = 3 * Mathf.Pow(1 - t, 2) * t * p1;
        Vector3 c = 3 * (1 - t) * Mathf.Pow(t, 2) * p2;
        Vector3 d = Mathf.Pow(t, 3) * p3;
        return a + b + c + d;
    }

    private IEnumerator GoByTheRoute(int routeNumber)
    {
        // stop multiple instances from running concurrently
        coroutineAllowed = false;

        // get control points for bezier
        Vector3 p0 = routes[routeNumber].GetChild(0).position;
        Vector3 p1 = routes[routeNumber].GetChild(1).position;
        Vector3 p2 = routes[routeNumber].GetChild(2).position;
        Vector3 p3 = routes[routeNumber].GetChild(3).position;

        while (tParam < 1)
        {
            // miniscule step
            float step = Time.deltaTime * speed / 20;

            // geodesic interpolation for constant velocity
            float travelled = 0;
            Vector3 pos = transform.position;
            while (travelled < distanceToTravel)
            {
                tParam += step;
                Vector3 nextPos = Position(tParam, p0, p1, p2, p3);
                travelled += Vector3.Distance(nextPos, pos);
                pos = nextPos;
            }
            position = pos;
            transform.position = position;

            // wait for next frame
            yield return new WaitForEndOfFrame();
        }

        // reset time param and move to next curve
        tParam = 0f;
        routeToGo += 1;
        // loop if at last curve
        if (routeToGo > routes.Length - 1) routeToGo = 0;

        coroutineAllowed = true;
    }
}
