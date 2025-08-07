using UnityEngine;

public class EnemyChasingBullet : ItemBase
{
    [Header("Lifetime & Movement")]
    [SerializeField] private float lifeTime = 4f;
    [SerializeField] private float speed = 5f;

    [Header("Homing")]
    [SerializeField, Range(0f, 1f)]
    private float homingResponsiveness = 0.1f;

    [Header("Wave Oscillation")]
    [SerializeField] private float oscillationAmplitude = 0.3f;
    [SerializeField] private float oscillationFrequency = 6f; // 얼마나 빠르게 흔들릴지

    private Vector2 currentHeading; // 현재 진행 방향
    private Transform target;
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

        if (target != null)
        {
            Vector2 toTarget = ((Vector2)(target.position - _tf.position)).normalized;
            currentHeading = Vector2.Lerp(currentHeading, toTarget, homingResponsiveness).normalized;
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
            PlayerStat.Instance.DamageHP(15);
            ShowStatusText("Damaged!", Color.red);
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
