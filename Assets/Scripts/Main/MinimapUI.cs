using UnityEngine;

public class MinimapUI : MonoBehaviour
{
    public RectTransform minimapBar;
    public RectTransform character;

    private Camera mainCam;
    private float viewHeightInWorld;
    private float totalHeight;

    private float startY, endY;
    private bool initialized = false;

    void Start()
    {
        TryInitialize();
    }

    void Update()
    {
        if (!initialized)
            TryInitialize();

        if (!initialized) return;
        if (mainCam == null) return;
        if (minimapBar == null || character == null) return;

        float cameraY = mainCam.transform.position.y;
        float minimapHeight = minimapBar.rect.height;

        float normalizedY = Mathf.InverseLerp(startY, endY, cameraY);
        float indicatorY = -minimapHeight * normalizedY;

        character.anchoredPosition = new Vector2(character.anchoredPosition.x, indicatorY);
    }

    private void TryInitialize()
    {
        if (mainCam == null)
            mainCam = Camera.main;
        if (mainCam == null) return;

        if (LayerManager.Instance == null) return;

        totalHeight = LayerManager.Instance.GetTilemapTotalHeight();
        viewHeightInWorld = mainCam.orthographicSize * 2f;

        startY = mainCam.transform.position.y;
        endY = startY - totalHeight;

        initialized = true;
    }
}
