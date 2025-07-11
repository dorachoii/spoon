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


    // Start is called before the first frame update
    void Start()
    {
        grid = new float[gridSize, gridSize];
    }

    // Update is called once per frame
    void Update()
    {

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
            }
        }
    }
#endif
}
