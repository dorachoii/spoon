using UnityEngine;
using UnityEngine.Tilemaps;

public abstract class ItemBase : MonoBehaviour
{
    float maxHeight = 1.1f; // 자동삭제 기준(Auto Destroyの基準)

    [Header("Effect Settings")]
    [SerializeField] protected GameObject effectPrefab;

    protected Tilemap tilemap;
    protected Camera mainCamera;

    protected virtual void Awake()
    {
        if (TileGenerator.Instance != null)
        {
            tilemap = TileGenerator.Instance.tilemap;
        }

        mainCamera = Camera.main;
    }

    protected virtual void Update()
    {
        if (mainCamera == null) return;
        Vector3 viewportPos = mainCamera.WorldToViewportPoint(transform.position);

        // 화면 밖으로 나가면 삭제(画面外に出たらAuto Destroy)
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

    // 삭제 시 효과 생성(エフェクト生成)
    protected virtual void InstantiateFX()
    {
        if (effectPrefab != null)
        {
            GameObject fx = Instantiate(effectPrefab, transform.position, Quaternion.identity);
            Destroy(fx, 1f);
        }
    }

    // 플레이어 상태 텍스트 표시(ステータステキスト表示)
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

    // 아이템 효과 (各アイテム固有の効果 )
    protected abstract void ApplyEffect(GameObject player);
}
