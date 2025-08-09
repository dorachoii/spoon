using System;
using System.Collections;
using UnityEngine;

// bodypart여러가지가 bossHP하나를 구독하고 있는 형식
// 각각 animator를 관리하기 위하여 그렇게 구성성
public class BossHP : MonoBehaviour
{

    private int maxHP = 100;

    public int CurrentHP { get; private set; }
    public bool IsDead { get; private set; }

    public event Action OnDeath;
    private Animator animator;

    void Awake()
    {
        CurrentHP = maxHP;
        IsDead = false;
        animator = GetComponentInChildren<Animator>();
    }
    
    public void TakeDamage(int amount)
    {
        if (IsDead) return;

        CurrentHP = Mathf.Max(0, CurrentHP - amount);

        if (CurrentHP <= 0)
        {
            IsDead = true;
            OnDeath?.Invoke();
            StartCoroutine(DestroyAfterAnimation());
        }
    }
    IEnumerator DestroyAfterAnimation()
    {
        // 현재 재생중인 애니메이션(Death)의 길이를 가져옴
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        float animationLength = stateInfo.length;
        yield return new WaitForSeconds(animationLength );
        Destroy(gameObject);
    }

}
