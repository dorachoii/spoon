using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossBodyPart : MonoBehaviour
{
    [SerializeField]
    private BossHP bossHP;

    private Animator animator;
    private float damageTriggerCooldown = 0.3f;
    private bool damagedTriggered = false;

    void Awake()
    {
        bossHP = GetComponentInParent<BossHP>();
        animator = GetComponent<Animator>();
    }
    void Start()
    {
        bossHP.OnDeath += HandleDeath;
    }

    void Oestroy()
    {
        bossHP.OnDeath -= HandleDeath;       
    }

    void HandleDeath()
    {
        animator.SetTrigger("Death");
    }

    public void Damage(int damage)
    {
        if (!damagedTriggered)
        {
            animator.SetTrigger("Damaged");
            StartCoroutine(DamagedCooldownRoutine());
        }

        bossHP.TakeDamage(damage);
    }

    private IEnumerator DamagedCooldownRoutine()
    {
        damagedTriggered = true;
        yield return new WaitForSeconds(damageTriggerCooldown);
        damagedTriggered = false;
    }
}
