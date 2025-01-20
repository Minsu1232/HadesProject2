using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AttackData;

public class RegenerationAbility : IEliteAbility
{
    private const float REGEN_INTERVAL = 1f;
    private const float REGEN_PERCENT = 0.01f;
    private float regenTimer = 0f;
    public string AbilityName => "���";
    public string Description => "�� �ʸ��� �ִ� ü���� 1% ȸ��";
    public Color OutlineColor => Color.green;

    public void ApplyAbility(ICreatureStatus creatureStatus) { }
    public void OnAttack(ICreatureStatus creatureStatus) { }
    public void OnHit(ICreatureStatus creatureStatus, int damage, AttackType attackType) { }
    public void OnUpdate(ICreatureStatus creatureStatus)
    {
        regenTimer += Time.deltaTime;
        if (regenTimer >= REGEN_INTERVAL)
        {
            regenTimer = 0f;
            int maxHealth = creatureStatus.GetMonsterClass().MaxHealth;
            int regenAmount = Mathf.Max(1, (int)(maxHealth * REGEN_PERCENT));
            int currentHealth = creatureStatus.GetMonsterClass().CurrentHealth;
            int newHealth = Mathf.Min(currentHealth + regenAmount, maxHealth);
            creatureStatus.ModifyHealth(newHealth - currentHealth);
        }
    }
}
