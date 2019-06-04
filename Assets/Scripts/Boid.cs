using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boid
{
    public GameObject boid;
    public Rigidbody rigidbody;
    public Vector3 position;
    public Vector3 velocity;
    public Vector3 acceleration;

    public float maxSpeed, maxForce;
    public float seekWeight, sepWeight, aliWeight, cohWeight;
    public float sepRadius, aliRadius, cohRadius;
    public int id;

    public Boid(GameObject prefab, Vector3 pos, float scale, float ms, float mf, int iD)
    {
        boid = Object.Instantiate(prefab, pos, Quaternion.identity) as GameObject;
        boid.transform.localScale = new Vector3(scale, scale, scale);

        position = pos;
        maxSpeed = ms;
        maxForce = mf;
        velocity = Vector3.zero;
        acceleration = Vector3.zero;
        id = iD;
    }

    // initializer function for force weights
    public void SetWeights(float sWeight, float s, float a, float c)
    {
        seekWeight = sWeight;
        sepWeight = s;
        aliWeight = a;
        cohWeight = c;
    }

    // initializer function for boid perception
    public void SetRadius(float sRad, float aRad, float cRad)
    {
        sepRadius = sRad;
        aliRadius = aRad;
        cohRadius = cRad;
    }

    // updates boids physics values
    public void Update()
    {
        velocity += acceleration; //Limit(acceleration, maxForce);
        velocity = Limit(velocity, maxSpeed);
        position += velocity;
        acceleration *= 0f;
        boid.transform.position = position;
        boid.transform.forward = velocity.normalized;
        boid.transform.Rotate(new Vector3(0, 90, 90));
    }

    // adds a force to the acceleration
    public void ApplyForce(Vector3 force)
    {
        acceleration += force;
    }

    // limits the magnitude of a vector
    public Vector3 Limit(Vector3 v, float max)
    {
        if (v.magnitude > max)
        {
            v.Normalize();
            v *= max;
        }
        return v;
    }

    // distance between boids position and target vector
    public float Distance(Vector3 target)
    {
        return Vector3.Distance(position, target);
    }

    // distance between boids position and other boid position
    public float Distance(Boid other)
    {
        return Vector3.Distance(position, other.position);
    }

    // distance between next position and other boids next position
    public float NextDistance(Boid other)
    {
        return Vector3.Distance(NextPosition(), other.NextPosition());
    }

    // sets the magnitude of a vector
    public Vector3 SetMag(Vector3 v, float mag)
    {
        v.Normalize();
        v *= mag;
        return v;
    }

    // calculates steering force as creig reynolds specifies
    public Vector3 Steer(Vector3 desired)
    {
        return Limit(desired - velocity, maxForce);
    }

    // A method that calculates and applies a steering force towards a target
    public Vector3 Seek(Vector3 target)
    {
        // find vector from position to target
        Vector3 desired = target - position;
        // scale to max speed
        desired = SetMag(desired, maxSpeed);
        return Steer(desired);
    }

    // check if going to collide with neighbors, if so apply force to avoid collision
    public Vector3 Seperate(Boid[] others, Vector3 target, Vector3 targetVelocity)
    {
        Vector3 nextPosition = NextPosition();
        Vector3 sum = Vector3.zero;
        int count = 0;

        // check if going to collide with target
        Vector3 nextTargetPosition = target + targetVelocity;
        float dist = Vector3.Distance(nextPosition, nextTargetPosition);
        if (dist < sepRadius * 2)
        {
            Vector3 diff = position - target;
            diff.Normalize();
            diff *= (sepRadius / dist);
            sum += diff;
            count++;
        }

        // check if going to collide with any others
        for (int i = 0; i < others.Length; i++)
        {
            Boid other = others[i];
            float d = NextDistance(other);
            // if not same as current boid and too close 
            if (id != other.id && d < sepRadius)
            {
                // find vector pointing away from neighbor
                Vector3 diff = position - other.position;
                diff.Normalize();
                diff *= (sepRadius / (2 * d) + 0.5f); // weight by distance
                sum += diff;
                count++;  
            }

        }

        // find average replusion vector and steer towards it
        if (count > 0)
        {
            sum *= (1 / count);
            sum = SetMag(sum, maxSpeed);
            return Steer(sum);
        }

        return sum;
    }

    // check neighbors velocity, and steer towards average
    public Vector3 Align(Boid[] others, Vector3 targetVelocity)
    {
        // always steer towards target velocity
        Vector3 sum = targetVelocity;
        int count = 1;

        // check if any other boids are within range
        for (int i = 0; i < others.Length; i++)
        {
            Boid other = others[i];
            float d = Distance(other);

            // if they are within range add their velocity weighted by distance
            if (d > 0 && d < aliRadius)
            {
                sum += other.velocity * (aliRadius / (2 * d) + 0.5f);
                count++;
            }
        }

        // find average velocity and steer towards it
        sum *= (1 / count);
        sum = SetMag(sum, maxSpeed);
        return Steer(sum);
    }

    // check neighbors position, and steer towards average
    public Vector3 Cohesion(Boid[] others, Vector3 target)
    {
        // always steer towards target position
        float neighborDist = 50;
        Vector3 sum = target;
        int count = 1;

        // check if any other boids are within range
        for (int i = 0; i < others.Length; i++)
        {
            Boid other = others[i];
            float d = Distance(other);

            // if they are within range add their position 
            if (d > 0 && d < neighborDist)
            {
                sum += other.position;
                count++;
            }
        }

        // find average and steer towards it
        sum *= (1 / count);
        return Seek(sum);
    }

    // main flocking algorithm
    public void FlockAround(Boid[] others, Vector3 target, Vector3 targetVelocity)
    {
        Vector3 seek = Seek(target);
        Vector3 seperation = Seperate(others, target, targetVelocity);
        Vector3 alignment = Align(others, targetVelocity);
        Vector3 cohesion = Cohesion(others, target);

        seek *= seekWeight * Distance(target) / 50;
        seperation *= sepWeight;
        alignment *= aliWeight;
        cohesion *= cohWeight;

        ApplyForce(seek);
        ApplyForce(seperation);
        ApplyForce(alignment);
        ApplyForce(cohesion);

        Update();
    }

    public Vector3 NextPosition()
    {
        return position + velocity;
    }
}
