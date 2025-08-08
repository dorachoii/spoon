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
            
            // CrumbleTilemap 태그를 가진 타일맵 컴포넌트들을 찾아서 isBossDead를 true로 설정
            SetCrumblingTilemapsBossDead();
            
            Destroy(gameObject, 2.5f);
        }
    }
    
    private void SetCrumblingTilemapsBossDead()
    {
        // CrumblingTilemap 컴포넌트를 가진 모든 게임오브젝트 찾기
        CrumblingTilemap[] crumblingTilemaps = FindObjectsOfType<CrumblingTilemap>();
        
        foreach (CrumblingTilemap crumblingTilemap in crumblingTilemaps)
        {
            if (crumblingTilemap != null)
            {
                // isBossDead를 true로 설정
                crumblingTilemap.SetBossDead(true);
                Debug.Log($"[BossHP] {crumblingTilemap.name}의 isBossDead를 true로 설정했습니다.");
            }
        }
    }
}
