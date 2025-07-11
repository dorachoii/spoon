using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;// 폰이나 다른 플랫폼에서 테스트 할 때는 지워줘야 한다!
#endif

using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    [Header(" Data ")]
    [SerializeField] private int gridSize;
    [SerializeField] private float gridScale;
    private float[,] grid;

    void Awake()
    {
        InputManager.onTouching += TouchingCallback;
    }
    // Start is called before the first frame update
    void Start()
    {
        grid = new float[gridSize, gridSize];
        for (int y = 0; y < gridSize; y++)
        {
            for (int x = 0; x < gridSize; x++)
            {
                grid[x, y] = Random.Range(0f, 2f);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void TouchingCallback(Vector3 worldPosition)
    {
        Debug.Log($"World Position : {worldPosition}");
    }

    private Vector2 GetWorldPositionFromGridPosition(int x, int y)
    {
        Vector2 worldPosition = new Vector2(x, y) * gridScale;
        worldPosition.x -= (gridSize * gridScale) / 2 - gridScale / 2;
        worldPosition.y -= (gridSize * gridScale) / 2 - gridScale / 2;

        return worldPosition;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!EditorApplication.isPlaying) return;

        Gizmos.color = Color.red;

        for (int y = 0; y < grid.GetLength(1); y++)
        {
            for (int x = 0; x < grid.GetLength(0); x++)
            {
                Vector2 worldPosition = GetWorldPositionFromGridPosition(x, y);

                Gizmos.DrawSphere(worldPosition, gridScale / 4);

                Handles.Label(worldPosition + Vector2.up * gridScale / 3, grid[x,y].ToString());
            }
        }
    }
#endif
}
