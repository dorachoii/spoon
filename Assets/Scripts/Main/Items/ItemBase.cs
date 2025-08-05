using UnityEngine;
using UnityEngine.Tilemaps;

public abstract class ItemBase : MonoBehaviour
{
    float maxHeight = 1.1f; // Auto Destroyの基準

    [Header("Effect Settings")]
    [SerializeField] protected GameObject effectPrefab;

    protected Tilemap tilemap;
    protected Camera mainCamera;

    protected virtual void Awake()
    {
        if (TileMaker.Instance != null)
        {
            tilemap = TileMaker.Instance.tilemap;
        }

        mainCamera = Camera.main;
    }

    protected virtual void Update()
    {
        if (mainCamera == null) return;
        Vector3 viewportPos = mainCamera.WorldToViewportPoint(transform.position);

        // 画面外に出たらAuto Destroy
        if (viewportPos.y > maxHeight)
        {
            Destroy(gameObject);
            return;
        }
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            InstantiateFX();
            ApplyEffect(collision.gameObject);
            Destroy(gameObject);
        }
    }

    // エフェクト生成
    protected virtual void InstantiateFX()
    {
        if (effectPrefab != null)
        {
            GameObject fx = Instantiate(effectPrefab, transform.position, Quaternion.identity);
            Destroy(fx, 1f);
        }
    }

    // ステータステキスト表示
    protected void ShowStatusText(string text, Color color)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerContoller controller = player.GetComponent<PlayerContoller>();
            if (controller != null)
            {
                controller.ShowStatusText(text, color);  
            }
        }
    }

    // 各アイテム固有の効果 abstract method
    protected abstract void ApplyEffect(GameObject player);
}
