using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Debris : MonoBehaviour
{
    private Camera mainCam;
    private float offscreenMargin = 1f;

    // Start is called before the first frame update
    void Start()
    {
        mainCam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        CheckOffScreen();
    }

    private void CheckOffScreen()
    {
        Vector3 viewportPos = mainCam.WorldToViewportPoint(transform.position);

        if (viewportPos.x < -offscreenMargin || viewportPos.x > 1 + offscreenMargin || viewportPos.y < -offscreenMargin || viewportPos.y > 1 + offscreenMargin) Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log($"부딪 - {collision.name}");
        if (collision.CompareTag("Boss"))
        {
            var part = collision.GetComponent<BossBodyPart>();

            if (part != null)
            {
                part.Damage(1);
                Destroy(gameObject);
            }

        }

        else if (collision.CompareTag("Player"))
        {
            Debug.Log("부딪 - 플레에어");
            PlayerStat.Instance.DamageHP(3);
        }
    }
}
