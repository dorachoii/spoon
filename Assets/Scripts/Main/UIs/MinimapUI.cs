using UnityEngine;

public class MinimapUI : MonoBehaviour
{
    [Header("Minimap Components")]
    [SerializeField] private RectTransform minimapBar;
    [SerializeField] private RectTransform character;

    private Camera mainCamera;
    private float startY;
    private float endY;
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

        float totalHeight = LayerManager.Instance.GetTilemapTotalHeight();
        startY = mainCamera.transform.position.y;
        endY = startY - totalHeight;

        miniBarHeight = minimapBar.rect.height + minimapBar.rect.y;
    }

    private void UpdateCharacterPosition()
    {
        float cameraY = mainCamera.transform.position.y;
        float normalizedProgress = Mathf.InverseLerp(startY, endY, cameraY);
        float characterY = -miniBarHeight * normalizedProgress;

        Vector2 currentPosition = character.anchoredPosition;
        character.anchoredPosition = new Vector2(currentPosition.x, characterY);
    }
}
