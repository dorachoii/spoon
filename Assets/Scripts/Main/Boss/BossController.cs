using System.Collections;
using UnityEngine;

[RequireComponent(typeof(BossHP))]
public class BossController : MonoBehaviour
{
    private Animator animator;
    private BossHP hp;

    [Header("Emergence")]
    [SerializeField] private Vector3 emergeOffset = new Vector3(10, 0, 0);
    [SerializeField] private float emergeTime = 1f;

    [Header("Phase Movement")]
    [SerializeField] private int hpThresholdForPatrol = 50;
    [SerializeField] private float patrolAmplitude = 2f; // 중심 기준 좌우 오프셋
    [SerializeField] private float patrolSpeed = 2f;     // 왕복 속도

    private Vector3 startPos;
    private Vector3 targetPos;
    private float timer = 0f;
    private bool emerging = true;

    // 왕복 내부
    private float patrolTime = 0f;

    void Start()
    {
        animator = GetComponent<Animator>();
        hp = GetComponent<BossHP>();

        targetPos = transform.position;
        startPos = targetPos + emergeOffset;
        transform.position = startPos;
        timer = 0f;
    }

    void Update()
    {
        if (emerging)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / emergeTime);
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            if (t >= 1f)
            {
                emerging = false;
            }
        }
        else
        {
            if (hp != null && !hp.IsDead && hp.CurrentHP <= hpThresholdForPatrol)
            {
                DoPatrol();
            }
        }
    }

    private void DoPatrol()
    {
        patrolTime += Time.deltaTime * patrolSpeed;
        float offsetX = Mathf.Sin(patrolTime) * patrolAmplitude;
        transform.position = targetPos + new Vector3(offsetX, 0f, 0f);
    }
}
