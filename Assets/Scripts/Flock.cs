using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flock : MonoBehaviour
{
    public GameObject prefab;

    [Range(0, 100)]
    public int total = 50;

    [Range(0f, 25f)]
    public float radius = 3;

    [Range(0.001f, 10f)]
    public float maxSpeed = 3;

    [Range(0.001f, 10f)]
    public float maxForce = 0.05f;

    [Range(0f, 5f)]
    public float seekWeight = 1;

    [Range(0f, 5f)]
    public float sepWeight = 1;

    [Range(0f, 5f)]
    public float aliWeight = 1;

    [Range(0f, 5f)]
    public float cohWeight = 1;

    [Range(1f, 200f)]
    public float sepRadius = 25;

    [Range(1f, 200f)]
    public float aliRadius = 50;

    [Range(1f, 200f)]
    public float cohRadius = 50;

    [Range(0f, 10f)]
    public float scale = 2;

    private Boid[] boids;

    private Vector3 prevPosition;

    // create boids
    void OnEnable()
    {
        boids = new Boid[total];
        for (int i = 0; i < total; i++)
        {
            Vector3 pos = new Vector3(Random.Range(0f, 250f), 50, Random.Range(0f, 250f));
            Boid boid = new Boid(prefab, pos, scale, maxSpeed, maxForce, i);
            boid.SetWeights(seekWeight, sepWeight, aliWeight, cohWeight);
            boid.SetRadius(sepRadius, aliRadius, cohRadius);
            boids[i] = boid;
        }
    }

    // run flocking algorithm for each boid each frame
    void Update()
    {
        Vector3 velocity = transform.position - prevPosition;
        for (int i = 0; i < total; i++)
        {
            boids[i].FlockAround(boids, transform.position, velocity);
        }
        prevPosition = transform.position;
    }
}
