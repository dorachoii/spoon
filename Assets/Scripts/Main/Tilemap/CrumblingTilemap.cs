using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// 점프 충돌로 부스러기를 생성하여 보스를 공격하는 타일맵
/// jumpで debrisを生成して bossを攻撃するTilemap
/// </summary>
public class CrumblingTilemap : MonoBehaviour
{
    [SerializeField] private Tilemap crumblingTilemap;

    [Header("Debris Settings")]
    [SerializeField] private GameObject debrisPrefab;
    [SerializeField] private float spread = 0.2f;
    [SerializeField] private float minForce = 0.5f;
    [SerializeField] private float maxForce = 1.5f;
    [SerializeField] private float minX = -0.3f;

    [Header("Cooldown Settings")]
    [SerializeField] private float cooldown = 0.3f;
    private HashSet<Vector3Int> recentlyCrumbled = new();  // 연속 충돌 시 중복 부서짐 방지 (連続衝突時の重複破壊防止)

    [Header("Boss Death Visual Effects")]
    [SerializeField] private Material crumblingMaterial; // 타일이 부서질 때 사용할 머티리얼
    [SerializeField] private float fadeOutDuration = 1.5f; // 페이드아웃 시간
    [SerializeField] private int maxDebrisPerBatch = 5; // 배치당 최대 데브리 수 (성능 최적화)

    private bool isBossDead = false;

    void Awake()
    {
        if(crumblingTilemap == null) crumblingTilemap = GetComponent<Tilemap>();
    }

    // 보스가 죽었을 때 호출되는 메서드
    public void SetBossDead(bool bossDead)
    {
        isBossDead = bossDead;
        if (bossDead)
        {
            Debug.Log("[CrumblingTilemap] 보스가 죽었습니다 - 대규모 부서짐 준비 완료!");
        }
    }

    private void OnCollisionEnter2D(Collision2D collision) => CrumbleCollision(collision);

    private void CrumbleCollision(Collision2D collision)
    {
        if(isBossDead) {
            // 보스가 죽고 난 다음 충돌이 발생하면!
            // 전체 타일 맵의 베이스들이 와르르 사라지고, debris가 와르르 쏟아지는 느낌.
            Destroy(gameObject);
            return;
        }
        
        HashSet<Vector3Int> toCrumble = new();
        Dictionary<Vector3Int, bool> debrisDir = new(); // true = up, false = down

        // 충돌 지점들을 순회하며 부딪힌 특정 타일 검출 (衝突ポイントを巡回して特定のタイルを検出)
        foreach (var contact in collision.contacts)
        {
            Vector3 worldPoint = contact.point;
            Vector3Int touchedCell = crumblingTilemap.WorldToCell(worldPoint);
            TileBase touchedTile = crumblingTilemap.GetTile(touchedCell);

            if (touchedTile == null) continue;

            Vector3Int crumbleTargetCell;
            bool debrisUp;
            
            ///<summary>
            /// 부서질 대상과 방향 결정 (破壊対象と方向を決定)
            /// </summary>
            
            // contact.normal.y: 양수면 아래에서 위로 충돌, 음수면 위에서 아래로 충돌
            // (contact.normal.y: 正数なら下から上への衝突、負数なら上から下への衝突)
            if (contact.normal.y > 0.1f)
            {
                crumbleTargetCell = GetHighestTile(touchedCell.x);
                debrisUp = true;
            }
            else
            {
                crumbleTargetCell = GetLowestTile(touchedCell.x);
                debrisUp = false;
            }

            if (crumbleTargetCell == Vector3Int.zero) continue;
            if (recentlyCrumbled.Contains(crumbleTargetCell)) continue;

            AddToCrumble(crumbleTargetCell, debrisUp, toCrumble, debrisDir);
        }

        if (toCrumble.Count == 0) return;
       
        // 부스러기 생성 (破片生成)
        foreach (var cell in toCrumble)
        {
            bool upwards = debrisDir[cell];
            StartCoroutine(ICrumble(cell, upwards, isBossDead));
        }
    }

    // 보스가 죽었을 때 전체 타일맵을 효율적으로 무너뜨리는 코루틴
 

    private IEnumerator ICrumble(Vector3Int cell, bool upwards, bool isBossDead)
    {
        // 보스가 죽었을 때는 recentlyCrumbled 체크를 건너뛰기
        if (!isBossDead)
        {
            recentlyCrumbled.Add(cell);
        }
        
        MakeDebris(cell, upwards, isBossDead);
        yield return new WaitForSeconds(cooldown);
        
        // 보스가 죽었을 때는 recentlyCrumbled에서 제거하지 않기
        if (!isBossDead)
        {
            recentlyCrumbled.Remove(cell);
        }
    }

    private void MakeDebris(Vector3Int cell, bool upwards, bool isBossDead)
    {
        TileBase tile = crumblingTilemap.GetTile(cell);
        if (tile == null) return;

        // 타일 제거 (이미 SetTiles로 제거되었을 수 있음)
        crumblingTilemap.SetTile(cell, null);

        // 보스가 죽었을 때는 데브리 생성 수를 더욱 제한하여 성능 최적화
        if (isBossDead && Random.Range(0f, 1f) > 0.15f) // 15% 확률로만 데브리 생성
        {
            return;
        }

        Vector3 origin = crumblingTilemap.GetCellCenterWorld(cell);
        Vector3 randomOffset = new Vector3(Random.Range(-spread, spread), 0f, 0f);

        GameObject debris = Instantiate(debrisPrefab, origin + randomOffset, Quaternion.identity);

        // Debris 컴포넌트에 보스 죽음 상태 전달
        Debris debrisComponent = debris.GetComponent<Debris>();
        if (debrisComponent != null)
        {
            debrisComponent.SetBossDead(isBossDead);
        }
        
        Rigidbody2D rb = debris.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            if (upwards)
            {
                rb.gravityScale = -Mathf.Abs(rb.gravityScale);  // upwards
            }
            else
            {
                rb.gravityScale = Mathf.Abs(rb.gravityScale);   // downwards
            }

            // 콜라이더에 끼지 않게 살짝 튀어나오게 (コライダーに挟まらないよう少し跳ね出す)
            float randomX = Random.Range(minX, -minX);
            float Y = upwards? 1f: -1f;

            Vector2 initialForce = new Vector2(randomX, Y).normalized * Random.Range(minForce, maxForce);
            rb.AddForce(initialForce, ForceMode2D.Impulse);
        }
    }
    
    // 충돌지점 x좌표의 가장 낮거나 높은 타일 찾는 함수 (衝突点x座標の最も低いまたは高いタイルを探す関数)
    private Vector3Int GetLowestTile(int columnX)
    {
        BoundsInt bounds = crumblingTilemap.cellBounds;
        for (int y = bounds.yMin; y <= bounds.yMax; y++)
        {
            Vector3Int cell = new Vector3Int(columnX, y, 0);
            if (crumblingTilemap.HasTile(cell))
                return cell;
        }
        return Vector3Int.zero;
    }

    private Vector3Int GetHighestTile(int columnX)
    {
        BoundsInt bounds = crumblingTilemap.cellBounds;
        for (int y = bounds.yMax; y >= bounds.yMin; y--)
        {
            Vector3Int cell = new Vector3Int(columnX, y, 0);
            if (crumblingTilemap.HasTile(cell))
                return cell;
        }
        return Vector3Int.zero;
    }

    private void AddToCrumble(Vector3Int centerCell, bool debrisUp, HashSet<Vector3Int> toCrumble, Dictionary<Vector3Int, bool> debrisDir)
    {
        if (!recentlyCrumbled.Contains(centerCell))
        {
            toCrumble.Add(centerCell);
            debrisDir[centerCell] = debrisUp;
        }

        // 인접한 8방향 타일들 추가 (상하좌우 + 대각선) 
        // 隣接する8方向タイルを追加 (上下左右 + 対角線))
        int[] dx = { -1, -1, -1,  0,  0,  1,  1,  1 };
        int[] dy = { -1,  0,  1, -1,  1, -1,  0,  1 };

        for (int i = 0; i < dx.Length; i++)
        {
            Vector3Int neighbor = centerCell + new Vector3Int(dx[i], dy[i], 0);
            
            if (crumblingTilemap.HasTile(neighbor) && !recentlyCrumbled.Contains(neighbor))
            {
                toCrumble.Add(neighbor);
                debrisDir[neighbor] = debrisUp;  
            }
        }
    }
}
