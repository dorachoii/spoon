using System;
using UnityEngine;

public class BossHP : MonoBehaviour
{
    [SerializeField] private int maxHP = 100;

    public int CurrentHP { get; private set; }
    public bool IsDead { get; private set; }

    public event Action OnDeath;

    void Awake()
    {
        CurrentHP = maxHP;
        IsDead = false;
    }

    public void TakeDamage(int amount)
    {
        if (IsDead) return;

        CurrentHP = Mathf.Max(0, CurrentHP - amount);

        if (CurrentHP <= 0)
        {
            IsDead = true;
            OnDeath?.Invoke();
            Destroy(gameObject, 2.5f);
        }

    }
}
