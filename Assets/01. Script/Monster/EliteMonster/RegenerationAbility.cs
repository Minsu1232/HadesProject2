using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AttackData;

public class RegenerationAbility : IEliteAbility
{
    private const float REGEN_INTERVAL = 1f;
    private const float REGEN_PERCENT = 0.01f;
    private float regenTimer = 0f;

    public string AbilityName => "재생";
    public string Description => "매 초마다 최대 체력의 1% 회복";
    public Color OutlineColor => Color.green;

    public void ApplyAbility(MonsterStatus monsterStatus) { }
    public void OnAttack(MonsterStatus monsterStatus) { }
    public void OnHit(MonsterStatus monsterStatus, int damage, AttackType attackType) { }

    public void OnUpdate(MonsterStatus monsterStatus)
    {
        regenTimer += Time.deltaTime;
        if (regenTimer >= REGEN_INTERVAL)
        {
            regenTimer = 0f;

            // 최대 체력 가져오기
            int maxHealth = monsterStatus.GetMonsterClass().MaxHealth;

            // 회복량 계산
            int regenAmount = Mathf.Max(1, (int)(maxHealth * REGEN_PERCENT));

            // 현재 체력 + 회복량이 최대 체력을 넘지 않도록 제한
            int currentHealth = monsterStatus.GetMonsterClass().CurrentHealth;
            int newHealth = Mathf.Min(currentHealth + regenAmount, maxHealth);

            // 체력 갱신
            monsterStatus.ModifyHealth(newHealth - currentHealth);
        }
    }
}
