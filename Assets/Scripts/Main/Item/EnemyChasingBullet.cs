using UnityEngine;

public class EnemyChasingBullet : ItemBase
{
    [Header("Lifetime & Movement")]
    [SerializeField] private float lifeTime = 4f;
    [SerializeField] private float speed = 5f;

    [Header("Homing")]
    [SerializeField, Range(0f, 1f)]
    private float homingResponsiveness = 0.1f;
    private float homingDuration = 0.5f; // 호밍 지속 시간

    [Header("Wave Oscillation")]
    [SerializeField] private float oscillationAmplitude = 0.3f;
    [SerializeField] private float oscillationFrequency = 6f; // 얼마나 빠르게 흔들릴지

    private Vector2 currentHeading; // 현재 진행 방향
    private Transform target;
    private Vector2 fixedHeading; // 고정된 진행 방향 (호밍 끝난 시점)
    private bool isHomingPhase = true; // 호밍 단계인지 여부
    private float elapsed = 0f;
    private Transform _tf;

    void Start()
    {
        _tf = transform;
        Destroy(gameObject, lifeTime);
        AcquireTarget();

        if (target != null)
            currentHeading = ((Vector2)(target.position - _tf.position)).normalized;
        else
            currentHeading = _tf.up; // 기본: 위쪽
    }

    protected override void Update()
    {
        base.Update();

        elapsed += Time.deltaTime;

        // 타깃이 없다면 재탐색
        if (target == null)
            AcquireTarget();

        // 호밍 단계
        if (isHomingPhase && target != null && elapsed < homingDuration)
        {
            Vector2 toTarget = ((Vector2)(target.position - _tf.position)).normalized;
            currentHeading = Vector2.Lerp(currentHeading, toTarget, homingResponsiveness).normalized;
        }
        // 호밍 단계가 끝나면 현재 방향을 고정
        else if (isHomingPhase && elapsed >= homingDuration)
        {
            isHomingPhase = false;
            fixedHeading = currentHeading; // 현재 진행 방향을 고정
            Debug.Log($"[EnemyChasingBullet] 호밍 단계 종료, 고정 방향 설정: {fixedHeading}");
        }
        // 고정 방향 단계 (파동 움직임만)
        else if (!isHomingPhase)
        {
            currentHeading = fixedHeading; // 고정된 방향 유지
        }

        Vector2 perp = new Vector2(-currentHeading.y, currentHeading.x);
        float wave = Mathf.Sin(elapsed * oscillationFrequency) * oscillationAmplitude;
        Vector2 velocity = (currentHeading + perp * wave).normalized * speed;

        _tf.position += (Vector3)(velocity * Time.deltaTime);

        // 진행 방향으로 회전 (스프라이트 기준 조정)
        float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
        _tf.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
    }

    protected override void ApplyEffect(GameObject player)
    {
        if (PlayerStat.Instance != null)
        {
            bool damageApplied = PlayerStat.Instance.DamageHP(10);
            if (damageApplied)
            {
                ShowStatusText("Damaged!", Color.red);
            }
        }
    }

    public void SetTarget(Transform t)
    {
        target = t;
    }

    private void AcquireTarget()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
            target = playerObj.transform;
    }
}
