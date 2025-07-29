using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOffScreen : MonoBehaviour
{
    private Camera cam;

    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
         if (cam.WorldToViewportPoint(transform.position).y > 1.1f)
        {
            Destroy(gameObject);
        }
    }
}
