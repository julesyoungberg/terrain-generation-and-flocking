using UnityEngine;
using System.Collections;

public class KeyboardController : MonoBehaviour
{
    public float speed = 0.1f;
    public float rotationSpeed = 0.1f;

    public void FixedUpdate()
    {
        Vector3 position = transform.position;
        Quaternion rotation = transform.rotation;
        Vector3 forward = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;

        if (Input.GetKey(KeyCode.RightArrow))
        {
            transform.Rotate(0f, rotationSpeed, 0f, Space.World);
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            transform.Rotate(0f, -rotationSpeed, 0f, Space.World);
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            transform.position = new Vector3(position.x - forward.x * speed, position.y, position.z - forward.z * speed);
        }
        if (Input.GetKey(KeyCode.UpArrow))
        {
            transform.position = new Vector3(position.x + forward.x * speed, position.y, position.z + forward.z * speed);
        }
    }
}