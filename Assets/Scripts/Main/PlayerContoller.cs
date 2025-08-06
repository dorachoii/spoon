using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;
using UnityEngine.Rendering;
using UnityEngine.Lumin;
using TMPro;

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
    public FloatingJoystick floatingJoystick;
    private Rigidbody2D rb;
    public Tilemap tilemap;

    [SerializeField]
    private float speed;
    private float jumpForce;

    [SerializeField]
    private float verticalThreshold = 0.2f;

    private Animator animator;

    public PlayerState currentState { get; private set; }

    public int brushRadius = 10;

    SpriteRenderer spriteRenderer;

    private bool isStateLocked = false;

    private float screenPadding = 1f;

    private DigDirection dir;
    private SpriteColorEffect effector;
    private Coroutine digCoroutine;
    private Coroutine rainbowCoroutine;
    private Coroutine poisonFlickerCoroutine;

    public GameObject floatingText;

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


    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        effector = GetComponent<SpriteColorEffect>();

        tilePositions = new Vector3Int[LayerManager.Instance.GetMaxTile()];
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
        if (PlayerStat.Instance != null)
        {
            PlayerStat.Instance.OnDamaged += HandleDamaged;
            PlayerStat.Instance.OnDied += HandleDied;
            PlayerStat.Instance.OnInvincibleStarted += HandleInvincibleStart;
            PlayerStat.Instance.OnInvincibleEnded += HandleInvincibleEnd;
            PlayerStat.Instance.OnPoisonedStarted += HandlePoisonStart;
            PlayerStat.Instance.OnPoisonedEnded += HandlePoisonEnd;

            //PlayerStat.Instance.OnPowerUp += HandlePowerUp;
        }
        jumpForce = PlayerStat.Instance.JumpForce;

        if (floatingJoystick == null) floatingJoystick = FindAnyObjectByType<FloatingJoystick>();
        if (tilemap == null) tilemap = FindObjectOfType<Tilemap>();
    }

    private void LateUpdate()
    {
        ClampPositionToCameraView();
    }


    private void OnDestroy()
    {
        if (PlayerStat.Instance != null)
        {
            PlayerStat.Instance.OnDamaged -= HandleDamaged;
            PlayerStat.Instance.OnDied -= HandleDied;
            PlayerStat.Instance.OnInvincibleStarted -= HandleInvincibleStart;
            PlayerStat.Instance.OnInvincibleEnded -= HandleInvincibleEnd;
            PlayerStat.Instance.OnPoisonedStarted -= HandlePoisonStart;
            PlayerStat.Instance.OnPoisonedEnded -= HandlePoisonEnd;

            //PlayerStat.Instance.OnPowerUp -= HandlePowerUp;
        }
    }

    private void OnEnable()
    {
        if (PlayerStat.Instance != null)
        {
            PlayerStat.Instance.OnDamaged += HandleDamaged;
            PlayerStat.Instance.OnDied += HandleDied;
        }
    }

    private void OnDisable()
    {
        if (PlayerStat.Instance != null)
        {
            PlayerStat.Instance.OnDamaged -= HandleDamaged;
            PlayerStat.Instance.OnDied -= HandleDied;
        }
    }



    private void HandlePoisonStart()
    {
        if (poisonFlickerCoroutine != null) return;

        if (rainbowCoroutine != null)
        {
       
            StopCoroutine(rainbowCoroutine);
            Color tint = Color.green * 0.6f + Color.white * 0.4f;
            rainbowCoroutine = StartCoroutine(effector.IRainbowEffect(spriteRenderer, -1, 2f, tint));
        }
        else
        {
            
            poisonFlickerCoroutine = StartCoroutine(GetComponent<SpriteColorEffect>().IFlicker(GetComponent<SpriteRenderer>(), SpriteEffectColor.Green, -1));
        }



    }

    private void HandlePoisonEnd()
    {
        Debug.Log("poison end");
        if (poisonFlickerCoroutine != null)
        {
            StopCoroutine(poisonFlickerCoroutine);
            poisonFlickerCoroutine = null;
        }

        if (rainbowCoroutine == null)
        {
            spriteRenderer.color = Color.white;
        }
        else
        {
            StopCoroutine(rainbowCoroutine);
            rainbowCoroutine = StartCoroutine(effector.IRainbowEffect(spriteRenderer, -1, 2f, Color.white));
        }
    }


    private void HandleDamaged()
    {
        
        ChangeState(PlayerState.Damaged);
    }

    private void HandleDied()
    {
        ChangeState(PlayerState.Die);
    }

    private void HandleInvincibleStart()
    {
        if (rainbowCoroutine != null) StopCoroutine(rainbowCoroutine);
      

        Color tint = Color.white;

        if (PlayerStat.Instance != null && PlayerStat.Instance.isPoisoned)
        {
            tint = Color.green * 0.6f + Color.white * 0.4f;
        }
        rainbowCoroutine = StartCoroutine(effector.IRainbowEffect(GetComponent<SpriteRenderer>(), -1, 2f, tint));
    }

    private void HandleInvincibleEnd()
    {
        if (rainbowCoroutine != null)
        {
            StopCoroutine(rainbowCoroutine);
            rainbowCoroutine = null;
        }

        if (PlayerStat.Instance != null && PlayerStat.Instance.isPoisoned)
        {
            poisonFlickerCoroutine = StartCoroutine(effector.IFlicker(spriteRenderer, SpriteEffectColor.Green, -1));
        }
        else
        {
            GetComponent<SpriteRenderer>().color = Color.white;
        }
    }


    public void FixedUpdate()
    {
        Vector2 inputDirection = new Vector2(floatingJoystick.Horizontal, floatingJoystick.Vertical);

        if (PlayerStat.Instance != null && PlayerStat.Instance.isPoisoned)
        {
            inputDirection = -inputDirection;
        }

        if (inputDirection.magnitude < 0.1f)
        {
            ChangeState(PlayerState.Idle);
            return;
        }

        float angle = Vector2.Angle(Vector2.up, inputDirection);
        bool isLeft = inputDirection.x < 0;
        float signedAngle = isLeft ? 360f - angle : angle;

        bool isDigging = false;

        if (signedAngle < 45f || signedAngle > 315f)
        {
            // 위 방향 → Jump
            ChangeState(PlayerState.Jump);
            isDigging = false;
        }
        else if (signedAngle >= 135f && signedAngle <= 225f)
        {
            dir = DigDirection.Down;
            isDigging = true;
        }
        else if (signedAngle >= 45f || signedAngle <= 315f)
        {
            dir = isLeft ? DigDirection.Left : DigDirection.Right;
            isDigging = true;
        }


        if (isDigging)
        {
            animator.SetInteger("DigDirection", (int)dir);
            ChangeState(PlayerState.Dig);
            rb.AddForce(inputDirection.normalized * speed * Time.fixedDeltaTime, ForceMode2D.Force);
            StartDig();
        }

    }



    private HashSet<Vector3Int> removedTiles = new HashSet<Vector3Int>();
    private List<Vector3Int> positionsToDig = new List<Vector3Int>();


    private Vector3Int[] tilePositions;
    private TileBase[] nullTiles;



    private bool isDigging = false;
    public void StartDig()
    {
        if (currentState != PlayerState.Dig || isDigging) return;

        if (digCoroutine != null)
        {
            StopCoroutine(digCoroutine);
        }
        digCoroutine = StartCoroutine(DigCoroutine());
    }

    private IEnumerator DigCoroutine()
    {
        isDigging = true;
        positionsToDig.Clear();

        Vector2 centerOffset = Vector2.zero;
        Func<int, int, bool> isWithinDigArea = (x, y) => false;

        switch (dir)
        {
            case DigDirection.Down:
                centerOffset = Vector2.down * 0.5f;
                isWithinDigArea = (x, y) => (x * x + y * y) <= brushRadius * brushRadius;
                break;
            case DigDirection.Left:
                centerOffset = Vector2.left * 0.5f;
                isWithinDigArea = (x, y) => (x <= 0) && (x * x + y * y) <= brushRadius * brushRadius;
                break;
            case DigDirection.Right:
                centerOffset = Vector2.right * 0.5f;
                isWithinDigArea = (x, y) => (x >= 0) && (x * x + y * y) <= brushRadius * brushRadius;
                break;
        }

        Vector2 playerPos = (Vector2)transform.position + centerOffset;
        Vector3Int centerCell = tilemap.WorldToCell(playerPos);

        for (int y = brushRadius; y >= -brushRadius; y--)
        {
            for (int x = -brushRadius; x <= brushRadius; x++)
            {
                if (!isWithinDigArea(x, y)) continue;

                Vector3Int cellPos = centerCell + new Vector3Int(x, y, 0);

                if (!tilemap.cellBounds.Contains(cellPos)) continue;
                if (removedTiles.Contains(cellPos)) continue;
                if (!tilemap.HasTile(cellPos)) continue;

                positionsToDig.Add(cellPos);
            }
        }

        int total = positionsToDig.Count;
        int current = 0;

        if (total > 0)
        {
            float hardness = LayerManager.Instance.GetCurrentHardness();
            float digPower = PlayerStat.Instance.DigPower;

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
                tilePositions[i] = positionsToDig[current + i];
            }

            tilemap.SetTiles(tilePositions, nullTiles);

            for (int i = 0; i < count; i++)
            {
                removedTiles.Add(tilePositions[i]);
            }

            current += count;

            yield return new WaitForSeconds(PlayerStat.Instance.GetDigDelay());
        }
        isDigging = false;
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector2 playerPos = transform.position + Vector3.down * 0.5f;
        float worldRadius = brushRadius * (tilemap != null ? tilemap.cellSize.x : 1f);

        Gizmos.DrawWireSphere(playerPos, worldRadius);
    }

    public IEnumerable<Vector3Int> GetRemovedTiles() => removedTiles;
    public void LoadRemovedTiles(IEnumerable<Vector3IntSerializable> savedPositions)
    {
        removedTiles.Clear();
        foreach (var pos in savedPositions)
        {
            removedTiles.Add(pos.ToVector3Int());
        }
    }

    private IEnumerator IDamageFlicker()
    {
        isStateLocked = true;

        yield return StartCoroutine(effector.IFlicker(GetComponent<SpriteRenderer>()));
        isStateLocked = false;

        ChangeState(PlayerState.Idle);
    }



    private void ClampPositionToCameraView()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        // Viewport (0,0) ~ (1,1) 은 카메라의 좌하단 ~ 우상단
        Vector3 min = cam.ViewportToWorldPoint(new Vector3(0, 0, cam.nearClipPlane));
        Vector3 max = cam.ViewportToWorldPoint(new Vector3(1, 1, cam.nearClipPlane));

        float minX = min.x + screenPadding;
        float maxX = max.x - screenPadding;
        float minY = min.y + screenPadding;
        float maxY = max.y - screenPadding;

        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);
        transform.position = pos;
    }

    public void ShowStatusText(string text, Color color)
    {
        Debug.Log($"***{text}를 {color}로 띄우자!");
        floatingText.SetActive(true);
        floatingText.GetComponent<StatusTextAnimator>().Initialize(text, color);
    }
}
