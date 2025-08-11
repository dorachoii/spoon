using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum DigDirection
{
    Down = 0,
    Left = 1,
    Right = 2
}

public enum PlayerState
{
    Idle,
    Jump,
    Dig,
    Damaged,
    Die
}

public class PlayerContoller : MonoBehaviour
{
    // Constants
    private const float DIG_OFFSET_DISTANCE = 0.5f;
    private const float SCREEN_PADDING = 1f;
    
    // Player State
    public PlayerState currentState { get; private set; }
    private bool isStateLocked = false;

    // Player Movement
    public FloatingJoystick floatingJoystick;
    private Rigidbody2D rb;
    private float speed;
    private float jumpForce;
  
    // Player Visual Effect
    private Animator animator;
    private SpriteRenderer sr;
    private SpriteColorEffector effector;
    public GameObject floatingText;
    private Coroutine coRainbow;
    private Coroutine coFlicker;

    // Digging & Tilemap
    public Tilemap tilemap;
    private int radius = 14;
    private HashSet<Vector3Int> TilesAlreadyDigged = new HashSet<Vector3Int>();
    private List<Vector3Int> TilesToDig = new List<Vector3Int>();
    private bool isDigging = false;
    private DigDirection digDir;
    private Coroutine coDig;

    private Vector3Int[] TilesNowDigged;
    private TileBase[] nullTiles;


    #region Initialize
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        effector = GetComponent<SpriteColorEffector>();

        TilesNowDigged = new Vector3Int[LayerManager.Instance.GetMaxTile()];
        nullTiles = new TileBase[LayerManager.Instance.GetMaxTile()];

        for (int i = 0; i < nullTiles.Length; i++)
        {
            nullTiles[i] = null;
        }

        if (floatingText != null)
        {
            floatingText.SetActive(false);
        }
    }

    private void Start()
    {
        // 플레이어 준비 이벤트 구독
        GameManager.OnPlayerReady += OnPlayerReady;
        
        // null 체크 후 동적으로 찾기
        FindMissingReferences();
    }
    
    private void OnPlayerReady()
    {
        // 플레이어가 준비된 후에 이벤트 구독
        if (PlayerStat.Instance != null)
        {
            PlayerStat.Instance.OnDamaged += HandleDamaged;
            PlayerStat.Instance.OnDied += HandleDied;
            PlayerStat.Instance.OnInvincibleStarted += StartInvincibleVisualEffect;
            PlayerStat.Instance.OnInvincibleEnded += StopInvincibleVisualEffect;
            PlayerStat.Instance.OnPoisonedStarted += StartPoisonVisualEffect;
            PlayerStat.Instance.OnPoisonedEnded += StopPoisonVisualEffect;

            speed = PlayerStat.Instance.Speed;
            jumpForce = PlayerStat.Instance.JumpForce;
        }
    }

    private void FindMissingReferences()
    {
        // FloatingJoystick 찾기
        if (floatingJoystick == null)
        {
            floatingJoystick = FindObjectOfType<FloatingJoystick>();
            if (floatingJoystick == null)
            {
                Debug.LogError("[PlayerController] FloatingJoystick not found in scene!");
            }
            else
            {
                Debug.Log("[PlayerController] FloatingJoystick found dynamically");
            }
        }
        
        // Tilemap 찾기
        if (tilemap == null)
        {
            if (TileGenerator.Instance != null)
            {
                tilemap = TileGenerator.Instance.tilemap;
                Debug.Log("[PlayerController] Tilemap found from TileGenerator");
            }
            else
            {
                tilemap = FindObjectOfType<Tilemap>();
                if (tilemap == null)
                {
                    Debug.LogError("[PlayerController] Tilemap not found in scene!");
                }
                else
                {
                    Debug.Log("[PlayerController] Tilemap found dynamically");
                }
            }
        }
        
        // FloatingText 찾기
        if (floatingText == null)
        {
            // 자식 오브젝트에서 찾기
            floatingText = transform.Find("FloatingText")?.gameObject;
            if (floatingText == null)
            {
                // 씬에서 찾기
                floatingText = GameObject.Find("FloatingText");
                if (floatingText == null)
                {
                    Debug.LogWarning("[PlayerController] FloatingText not found - status text will not work");
                }
                else
                {
                    Debug.Log("[PlayerController] FloatingText found dynamically");
                }
            }
        }
    }

    private void OnDestroy()
    {
        // 플레이어 준비 이벤트 구독 해제
        GameManager.OnPlayerReady -= OnPlayerReady;
        
        if (PlayerStat.Instance != null)
        {
            PlayerStat.Instance.OnDamaged -= HandleDamaged;
            PlayerStat.Instance.OnDied -= HandleDied;
            PlayerStat.Instance.OnInvincibleStarted -= StartInvincibleVisualEffect;
            PlayerStat.Instance.OnInvincibleEnded -= StopInvincibleVisualEffect;
            PlayerStat.Instance.OnPoisonedStarted -= StartPoisonVisualEffect;
            PlayerStat.Instance.OnPoisonedEnded -= StopPoisonVisualEffect;
        }
    }

    #endregion

    private void LateUpdate()
    {
        ClampPlayerPosToScreenBounds();
    }


    #region Player State
    public void ChangeState(PlayerState newState)
    {
        if (currentState == newState || isStateLocked) return;

        currentState = newState;

        switch (newState)
        {
            case PlayerState.Idle:
                animator.SetBool("IsDigging", false);
                break;
            case PlayerState.Jump:
                animator.SetTrigger("JumpTrigger");
                rb.velocity = new Vector2(rb.velocity.x, 0);
                rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
                break;
            case PlayerState.Dig:
                animator.SetBool("IsDigging", true);
                break;
            case PlayerState.Damaged:
                animator.SetBool("IsDigging", false);
                StartCoroutine(IDamageFlicker());
                break;
            case PlayerState.Die:
                animator.SetTrigger("Die");
                break;
        }
    }
    private void HandleDamaged()
    {
        ChangeState(PlayerState.Damaged);
    }

    private void HandleDied()
    {
        ChangeState(PlayerState.Die);
        isStateLocked = true;
    }

    #endregion

    #region Player Movement & Digging

     public void FixedUpdate()
    {
        // floatingJoystick이 null이면 입력 처리하지 않음
        if (floatingJoystick == null)
        {
            return;
        }
        
        Vector2 inputDirection = new Vector2(floatingJoystick.Horizontal, floatingJoystick.Vertical);

        Debug.Log("PlayerPosition- inputDirection: " + inputDirection);
        if (PlayerStat.Instance != null && PlayerStat.Instance.isPoisoned)
        {
            inputDirection = -inputDirection;
        }

        if (inputDirection.magnitude < 0.1f)
        {
            ChangeState(PlayerState.Idle);
            return;
        }

        // 시계방향으로 0~360도 각도 계산
        // 時計回りの0~360度の角度を計算
        float angle = Vector2.Angle(Vector2.up, inputDirection);
        bool isLeft = inputDirection.x < 0;
        float signedAngle = isLeft ? 360f - angle : angle;

        bool shouldJump = true;

        // up → Jump
        if (signedAngle < 45f || signedAngle > 315f)
        {
            ChangeState(PlayerState.Jump);
            shouldJump = true;
        }
        // down → Dig
        else if (signedAngle >= 135f && signedAngle <= 225f)
        {
            digDir = DigDirection.Down;
            shouldJump = false;
        }
        // left, right → Dig
        else if (signedAngle >= 45f || signedAngle <= 315f)
        {
            digDir = isLeft ? DigDirection.Left : DigDirection.Right;
            shouldJump = false;
        }

        if (!shouldJump)
        {
            animator.SetInteger("DigDirection", (int)digDir);
            ChangeState(PlayerState.Dig);
            // player move
            rb.AddForce(inputDirection.normalized * speed * Time.fixedDeltaTime, ForceMode2D.Force);
            StartDig();
        }
    }

    public void StartDig()
    {
        if (currentState != PlayerState.Dig || isDigging) return;

        if (coDig != null)
        {
            StopCoroutine(coDig);
        }
        coDig = StartCoroutine(DigCoroutine());
    }

    private IEnumerator DigCoroutine()
    {
        isDigging = true;
        TilesToDig.Clear();

        CalculateDiggingArea();
        yield return StartCoroutine(DigTiles());
        
        isDigging = false;
    }

    // 파는 유효범위를 측정하고 파야 할 타일들을 찾아서 담는 함수 
    // 掘削有効範囲を測定 + 掘るべきタイルを見つけて格納する関数
    private void CalculateDiggingArea()
    {
        // 방향에 따른 파는 중앙점, 파는 범위 설정 (方向に応じた掘削中心点、掘削範囲設定)
        Vector2 diggingCenter = Vector2.zero;
        Func<int, int, bool> isWithinDigArea = (x, y) => false;

        switch (digDir)
        {
            case DigDirection.Down:
                diggingCenter = Vector2.down * 0.5f;
                isWithinDigArea = (x, y) => (x * x + y * y) <= radius * radius;
                break;
            case DigDirection.Left:
                diggingCenter = Vector2.left * 0.5f;
                isWithinDigArea = (x, y) => (x <= 0) && (x * x + y * y) <= radius * radius;
                break;
            case DigDirection.Right:
                diggingCenter = Vector2.right * 0.5f;
                isWithinDigArea = (x, y) => (x >= 0) && (x * x + y * y) <= radius * radius;
                break;
        }

        // 파야하는 타일 저장 (掘るべきタイル格納庫)
        Vector2 centerPos = (Vector2)transform.position + diggingCenter;
        Vector3Int centerCell = tilemap.WorldToCell(centerPos);

        for (int y = radius; y >= -radius; y--)
        {
            for (int x = -radius; x <= radius; x++)
            {
                if (!isWithinDigArea(x, y)) continue;

                Vector3Int cellPos = centerCell + new Vector3Int(x, y, 0);

                if (!tilemap.cellBounds.Contains(cellPos)) {
                    Debug.Log($"[PlayerController] 타일맵 범위 밖: {cellPos}");
                    continue;
                }
                if (TilesAlreadyDigged.Contains(cellPos))   {
                    Debug.Log($"[PlayerController] 이미 제거된 타일: {cellPos}");
                    continue;
                }
                if (!tilemap.HasTile(cellPos)) {
                    Debug.Log($"[PlayerController] 타일맵에 타일이 없음: {cellPos}");
                    continue;
                }

                TilesToDig.Add(cellPos);
            }
        }
    }
    private IEnumerator DigTiles()
    {
        int total = TilesToDig.Count;
        int current = 0;

        if (total > 0)
        {
            //현재 층이 팔 수 있는 힘보다 강하면 튕겨 나간다. (現在の層が掘れる力より強い場合は跳ね飛ばす)
            float hardness = LayerManager.Instance.GetCurrentHardness();
            float digPower = PlayerStat.Instance.CurrentPower;

            if (digPower < hardness)
            {
                ChangeState(PlayerState.Jump);
                isDigging = false;
                yield break;
            }
        }

        while (current < total)
        {
            int count = Mathf.Min(LayerManager.Instance.GetMaxTile(), total - current);

            for (int i = 0; i < count; i++)
            {
                TilesNowDigged[i] = TilesToDig[current + i];
            }

            tilemap.SetTiles(TilesNowDigged, nullTiles);

            for (int i = 0; i < count; i++)
            {
                TilesAlreadyDigged.Add(TilesNowDigged[i]);
            }

            current += count;

            yield return new WaitForSeconds(PlayerStat.Instance.GetDigDelay());
        }
    }


    private void ClampPlayerPosToScreenBounds()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        Vector3 min = cam.ViewportToWorldPoint(new Vector3(0, 0, cam.nearClipPlane));
        Vector3 max = cam.ViewportToWorldPoint(new Vector3(1, 1, cam.nearClipPlane));

        float minX = min.x + SCREEN_PADDING;
        float maxX = max.x - SCREEN_PADDING;
        float minY = min.y + SCREEN_PADDING;
        float maxY = max.y - SCREEN_PADDING;

        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);
        transform.position = pos;
    }
    #endregion

    #region Player Visual Effect
    public void ShowStatusText(string text, Color color)
    {
        if (floatingText == null)
        {
            Debug.LogWarning("[PlayerController] Cannot show status text - floatingText is null");
            return;
        }
        
        floatingText.SetActive(true);
        floatingText.GetComponent<StatusTextAnimator>().Initialize(text, color);
    }

    private IEnumerator IDamageFlicker()
    {
        isStateLocked = true;

        yield return StartCoroutine(effector.IFlicker(GetComponent<SpriteRenderer>()));
        isStateLocked = false;

        ChangeState(PlayerState.Idle);
    }

    private void StartInvincibleVisualEffect()
    {
        if (coRainbow != null) StopCoroutine(coRainbow);

        Color tint = Color.white;

        // poision + invincible
        if (PlayerStat.Instance.isPoisoned)
        {
            tint = Color.green * 0.6f + Color.white * 0.4f;
        }
        coRainbow = StartCoroutine(effector.IRainbow(GetComponent<SpriteRenderer>(), loop: true, hueSpeed: 2f, tint: tint));
    }

    private void StopInvincibleVisualEffect()
    {
        if (coRainbow != null)
        {
            StopCoroutine(coRainbow);
            sr.color = Color.white;
            coRainbow = null;
        }

        // poision + invincible
        if (PlayerStat.Instance.isPoisoned)
        {
            coFlicker = StartCoroutine(effector.IFlicker(sr, PlayerColor.Green, loop: true));
        }
    }

    private void StartPoisonVisualEffect()
    {
        if (coFlicker != null) return;

        if (coRainbow != null)
        {
             // invincible + poision
            StopCoroutine(coRainbow);
            Color tint = Color.green * 0.6f + Color.white * 0.4f;
            coRainbow = StartCoroutine(effector.IRainbow(sr, loop: true, hueSpeed: 2f, tint: tint));
        }
        else
        {
            coFlicker = StartCoroutine(GetComponent<SpriteColorEffector>().IFlicker(GetComponent<SpriteRenderer>(), PlayerColor.Green, loop: true));
        }
    }

    private void StopPoisonVisualEffect()
    {
        if (coFlicker != null)
        {
            StopCoroutine(coFlicker);
            coFlicker = null;
        }

        if (coRainbow == null)
        {
            sr.color = Color.white;
        }
        else
        {
            StopCoroutine(coRainbow);
            coRainbow = StartCoroutine(effector.IRainbow(sr, loop: true, hueSpeed: 2f, tint: Color.white));
        }
    }
    #endregion

    #region Save & Load
    // 타일맵이 재시작될 때 호출되어 removedTiles 캐시를 초기화
    public void ClearDiggedTiles()
    {
        TilesAlreadyDigged.Clear();
    }

    #endregion
    
    #region Digging Range Gizmos
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector2 playerPos = transform.position + Vector3.down * DIG_OFFSET_DISTANCE;
        float worldRadius = radius * (tilemap != null ? tilemap.cellSize.x : 1f);

        Gizmos.DrawWireSphere(playerPos, worldRadius);
    }
    #endregion
 
}
