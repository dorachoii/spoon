using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossGroundTilemap : MonoBehaviour
{
    int cnt = 0;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            cnt++;
            if (cnt > 3)
            {
                Destroy(gameObject);
            }
        }
    }
}
