using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class InputManager : MonoBehaviour
{
    private bool clicking;

    [Header(" Action ")]
    public static Action<Vector3> onTouching;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            clicking = true;
        }
        else if (Input.GetMouseButton(0) && clicking)
        {
            Clicking();
        }
        else if (Input.GetMouseButtonUp(0) && clicking)
        {
            clicking = false;
        }
    }

    private void Clicking()
    {
        RaycastHit hit;
        Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 50);

        if (hit.collider == null) return;

        onTouching?.Invoke(hit.point);

    }
}
