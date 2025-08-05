using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JoystickPlayerExample : MonoBehaviour
{
    public FloatingJoystick floatingJoystick;
    public Rigidbody2D rb;

    [SerializeField]
    private float speed;

    [SerializeField]
    private float verticalThreshold = 0.2f;

    public void FixedUpdate()
    {
        Vector3 direction = Vector3.up * floatingJoystick.Vertical + Vector3.right * floatingJoystick.Horizontal;
        rb.AddForce(direction * speed * Time.fixedDeltaTime, ForceMode2D.Force);

        float verticalInput = floatingJoystick.Vertical;

        if (verticalInput > verticalThreshold)
        {
            Debug.Log("Moving Up");
        }
        else if (verticalInput < -verticalThreshold)
        {
            Debug.Log("Moving Down");
        }
    }
}