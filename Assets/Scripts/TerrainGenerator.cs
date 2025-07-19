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
    [SerializeField] private float brushFallback;

    [Header(" Data ")]
    [SerializeField] private int gridSize;
    [SerializeField] private float gridScale;
    [SerializeField] private float isoValue;

    private SquareGrid squareGrid;

    Mesh mesh;
    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();


    private float[,] grid;

    void Awake()
    {
        InputManager.onTouching += TouchingCallback;
    }
    
    void OnDestroy()
    {
        // 이벤트 구독 해제
        InputManager.onTouching -= TouchingCallback;
        
        // 메시 정리
        if (mesh != null)
        {
            mesh.Clear();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 60;

        mesh = new Mesh();

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
        //Debug.Log($"tg - World Position : {worldPosition}");
        worldPosition.z = 0;
        worldPosition = transform.InverseTransformPoint(worldPosition);

        // world -> grid pos
        Vector2Int gridPosition = GetGridPositionFromWorldPosition(worldPosition);

        bool shouldGenerate = false;

        for (int y = gridPosition.y - brushRadius; y <= gridPosition.y + brushRadius; y++)
        {
            for (int x = gridPosition.x - brushRadius; x <= gridPosition.x + brushRadius; x++)
            {
                Vector2Int currentGridPosition = new Vector2Int(x, y);

                if (!IsValidGridPosition(currentGridPosition)) continue;

                float distance = Vector2.Distance(currentGridPosition, gridPosition);
                
                // 원형 브러시를 위해 거리 체크 추가
                if (distance > brushRadius) continue;
                
                float factor = brushStrength * Mathf.Exp(-distance * brushFallback / brushRadius);

                // 올바른 그리드 위치에 적용
                grid[currentGridPosition.x, currentGridPosition.y] -= factor;
                shouldGenerate = true;
            }
        }

        if(shouldGenerate) GenerateMesh();
    }

    private void GenerateMesh()
    {
        // 기존 메시 정리
        if (mesh != null)
        {
            mesh.Clear();
        }
        
        mesh = new Mesh();

        vertices.Clear();
        triangles.Clear();

        squareGrid.Update(grid);

        mesh.vertices = squareGrid.GetVertices();
        mesh.triangles = squareGrid.GetTriangles();

        filter.mesh = mesh;

        GenerateCollider();
    }

    private void GenerateCollider()
    {
        if (filter.TryGetComponent(out MeshCollider meshCollider))
        {
            meshCollider.sharedMesh = mesh;
        }
        else
        {
            filter.gameObject.AddComponent<MeshCollider>().sharedMesh = mesh;
        }
    }

    private bool IsValidGridPosition(Vector2Int gridPosition)
    {
        return gridPosition.x >= 0 && gridPosition.x < gridSize && 
               gridPosition.y >= 0 && gridPosition.y < gridSize;
    }

    private Vector2 GetWorldPositionFromGridPosition(int x, int y)
    {
        Vector2 worldPosition = new Vector2(x, y) * gridScale;
        
        // 그리드 중심을 0으로 맞추기 위한 오프셋 계산
        float offset = (gridSize * gridScale) / 2 - gridScale / 2;
        worldPosition.x -= offset;
        worldPosition.y -= offset;

        return worldPosition;
    }

    private Vector2Int GetGridPositionFromWorldPosition(Vector2 worldPosition)
    {
        Vector2Int gridPosition = new Vector2Int();

        // 그리드 중심을 0으로 맞추기 위한 오프셋 계산
        float offset = (gridSize * gridScale) / 2 - gridScale / 2;
        
        gridPosition.x = Mathf.FloorToInt((worldPosition.x + offset) / gridScale);
        gridPosition.y = Mathf.FloorToInt((worldPosition.y + offset) / gridScale);

        return gridPosition;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!EditorApplication.isPlaying || grid == null) return;


        Gizmos.color = Color.red;

        // 디버그용으로 일부만 표시 (메모리 절약)
        int step = Mathf.Max(1, gridSize / 20); // 20개씩 건너뛰기
        
        for (int y = 0; y < grid.GetLength(1); y += step)
        {
            for (int x = 0; x < grid.GetLength(0); x += step)
            {
                Vector2 worldPosition = GetWorldPositionFromGridPosition(x, y);
                Gizmos.DrawSphere(worldPosition, gridScale / 4);

                // 라벨도 일부만 표시
                if (x % (step * 2) == 0 && y % (step * 2) == 0)
                {
                    Handles.Label(worldPosition + Vector2.up * gridScale / 3, grid[x, y].ToString("F1"));
                }
            }
        }
    }
#endif
}
