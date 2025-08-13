using UnityEngine;

public class MinimapUI : MonoBehaviour
{
    [Header("Minimap Components")]
    [SerializeField] private RectTransform minimapBar;
    [SerializeField] private RectTransform character;

    private Camera mainCamera;
    private float startY;
    private float miniBarHeight;

    private void Start()
    {
        InitializeMinimap();
    }

    private void Update()
    {       
        UpdateCharacterPosition();
    }

    private void InitializeMinimap()
    {
        if (minimapBar == null || character == null || LayerManager.Instance == null) return;
        mainCamera = Camera.main;

        startY = mainCamera.transform.position.y;

        miniBarHeight = minimapBar.rect.height + minimapBar.rect.y;
    }

    private void UpdateCharacterPosition()
    {
        if (mainCamera == null || character == null || LayerManager.Instance == null) return;

        float currentTotalHeight = LayerManager.Instance.GetTilemapTotalHeight();
        float cameraY = mainCamera.transform.position.y;
        
        float currentEndY = mainCamera.transform.position.y - currentTotalHeight;
        
        float normalizedProgress = Mathf.InverseLerp(startY, currentEndY, cameraY);
        float progressY = -miniBarHeight * normalizedProgress;

        Vector2 currentPosition = character.anchoredPosition;
        Vector2 newPosition = new Vector2(currentPosition.x, progressY);
   
        character.anchoredPosition = newPosition;
        
        // 디버깅 로그
        Debug.Log($"Minimap Update - CameraY: {cameraY}, Progress: {normalizedProgress:F2}, NewY: {progressY:F2}");
    }
}
