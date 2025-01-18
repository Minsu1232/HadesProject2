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

    public void ApplyAbility(MonsterStatus monsterStatus) { }
    public void OnAttack(MonsterStatus monsterStatus) { }
    public void OnHit(MonsterStatus monsterStatus, int damage, AttackType attackType) { }

    public void OnUpdate(MonsterStatus monsterStatus)
    {
        regenTimer += Time.deltaTime;
        if (regenTimer >= REGEN_INTERVAL)
        {
            regenTimer = 0f;

            // �ִ� ü�� ��������
            int maxHealth = monsterStatus.GetMonsterClass().MaxHealth;

            // ȸ���� ���
            int regenAmount = Mathf.Max(1, (int)(maxHealth * REGEN_PERCENT));

            // ���� ü�� + ȸ������ �ִ� ü���� ���� �ʵ��� ����
            int currentHealth = monsterStatus.GetMonsterClass().CurrentHealth;
            int newHealth = Mathf.Min(currentHealth + regenAmount, maxHealth);

            // ü�� ����
            monsterStatus.ModifyHealth(newHealth - currentHealth);
        }
    }
}
