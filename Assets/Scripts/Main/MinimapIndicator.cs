using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapIndicator : MonoBehaviour
{
    public RectTransform minimapBar;
    public RectTransform character;
    private Camera mainCam;
    private float viewHeightInWorld = 20f;
    float totalHeight;

    float startY, endY;


    // Start is called before the first frame update
    void Start()
    {
        mainCam = Camera.main;
        totalHeight = LevelManager.Instance.GetTilemapTotalHeight();
        viewHeightInWorld = mainCam.orthographicSize * 2f;

        startY = mainCam.transform.position.y;
        endY = startY - totalHeight;
    }

    // Update is called once per frame
    void Update()
    {
        float cameraY = mainCam.transform.position.y;
        float minimapHeight = minimapBar.rect.height;

        float normalizedY = Mathf.InverseLerp(startY, endY, cameraY);

        float indicatorY = -minimapHeight * normalizedY;

        character.anchoredPosition = new Vector2(character.anchoredPosition.x, indicatorY);
    }



}

