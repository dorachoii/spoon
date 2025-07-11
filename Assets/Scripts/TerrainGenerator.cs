using System.Collections;
using System.Collections.Generic;


#if UNITY_EDITOR
using UnityEditor;  // 폰이나 다른 플랫폼에서 테스트 할 때는 지워줘야 한다!
#endif

using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    
    [Header(" Elements ")]
    [SerializeField] private MeshFilter filter;

    [Header(" Brush Settings ")]
    [SerializeField] private int brushRadius;
    [SerializeField] private float brushStrength;

    [Header(" Data ")]
    [SerializeField] private int gridSize;
    [SerializeField] private float gridScale;
    [SerializeField] private float isoValue;

    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();

    private SquareGrid squareGrid;

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
                grid[x, y] = isoValue + 0.1f;
            }
        }

        squareGrid = new SquareGrid(gridSize - 1, gridScale, isoValue);

        GenerateMesh();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void TouchingCallback(Vector3 worldPosition)
    {
        Debug.Log($"tg - World Position : {worldPosition}");
        worldPosition.z = 0;

        // world -> grid pos
        Vector2Int gridPosition = GetGridPositionFromWorldPosition(worldPosition);

        for (int y = gridPosition.y - brushRadius; y <= gridPosition.y + brushRadius; y++)
        {
            for (int x = gridPosition.x - brushRadius; x <= gridPosition.x + brushRadius; x++)
            {
                Vector2Int currentGridPosition = new Vector2Int(x, y);

                if (!IsValidGridPosition(currentGridPosition))
                {
                    Debug.LogError("Invalid grid position");
                    continue;
                }
                grid[gridPosition.x, gridPosition.y] -= brushStrength;
            }
        }

        GenerateMesh();
    }

    private void GenerateMesh()
    {
        Mesh mesh = new Mesh();

        vertices.Clear();
        triangles.Clear();

        squareGrid.Update(grid);

        mesh.vertices = squareGrid.GetVertices();
        mesh.triangles = squareGrid.GetTriangles();

        filter.mesh = mesh;
    }

    private bool IsValidGridPosition(Vector2Int gridPosition)
    {
        return gridPosition.x >= 0 && gridPosition.x < gridSize && gridPosition.y < gridSize;
    }

    private Vector2 GetWorldPositionFromGridPosition(int x, int y)
    {
        Vector2 worldPosition = new Vector2(x, y) * gridScale;
        worldPosition.x -= (gridSize * gridScale) / 2 - gridScale / 2;
        worldPosition.y -= (gridSize * gridScale) / 2 - gridScale / 2;

        return worldPosition;
    }

    private Vector2Int GetGridPositionFromWorldPosition(Vector2 worldPosition)
    {
        Vector2Int gridPosition = new Vector2Int();

        gridPosition.x = Mathf.FloorToInt(worldPosition.x / gridScale + gridSize / 2 - gridScale / 2);
        gridPosition.y = Mathf.FloorToInt(worldPosition.y / gridScale + gridSize / 2 - gridScale / 2);

        return gridPosition;
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
                Debug.Log($"tg - Gizmo - {x},{y} : {worldPosition}");
                Gizmos.DrawSphere(worldPosition, gridScale / 4);

                Handles.Label(worldPosition + Vector2.up * gridScale / 3, grid[x, y].ToString());
            }
        }
    }
#endif
}
