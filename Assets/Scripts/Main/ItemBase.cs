using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public abstract class ItemBase : MonoBehaviour
{
    protected Tilemap tilemap;
    protected Camera mainCamera;

    public float maxHeight = 30f;
    protected Vector3Int currentCell;

    protected virtual void Awake()
    {
        tilemap = FindObjectOfType<Tilemap>();
        mainCamera = Camera.main;
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        currentCell = tilemap.WorldToCell(transform.position);

        Vector3 viewportPos = mainCamera.WorldToViewportPoint(transform.position);

        if (viewportPos.y > 1.1f)
        {
            Destroy(gameObject);
            return;
        }
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log($"[BombItem] Triggered with {collision.name}");
        if (collision.CompareTag("Player"))
        {
            ApplyEffect(collision.gameObject);
            Destroy(gameObject);
        }
    }

    protected abstract void ApplyEffect(GameObject player);

}
