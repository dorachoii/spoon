using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ItemScroll : MonoBehaviour
{
    private Tilemap tilemap;
    private Vector3Int currentCell;

    void Awake()
    {
        tilemap = FindObjectOfType<Tilemap>(); 
    }

    // Update is called once per frame
    void Update()
    {
        currentCell = tilemap.WorldToCell(transform.position);

        if (!tilemap.HasTile(currentCell))
        {
            Destroy(gameObject);
        }
    }
}
