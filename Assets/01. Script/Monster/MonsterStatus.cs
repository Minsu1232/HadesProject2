using RPGCharacterAnims.Lookups;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static AttackData;
using static IMonsterState;

public class MonsterStatus : MonoBehaviour,IDamageable
{
    private MonsterClass monsterClass; // ������ ������
    private bool isDie = false;
    
    [SerializeField] private Transform skillSpawnPoint;
    private MonsterUIManager uiManager; // MonsterUIManager ĳ�̿� ���� �߰�
    private void Awake()
    {
        uiManager = GetComponent<MonsterUIManager>(); // ���� �� �� ���� ��������
       
        skillSpawnPoint = GetComponentsInChildren<Transform>()
          .FirstOrDefault(t => t.name == "SkillSpawnPoint");

        if (skillSpawnPoint == null)
        {
            Debug.LogWarning($"{gameObject.name}���� SkillSpawnPoint�� ã�� ���߽��ϴ�. ���� ��ġ�� ����մϴ�.");
            skillSpawnPoint = transform;
        }
    }

    public void Initialize(MonsterClass data)
    {
        monsterClass = data;
    }
    public AttackType GetAttackType()
    {
        return monsterClass.IsBasicAttack() ? AttackType.Normal : AttackType.Charge;
    }
    public void TakeDamage(int damage, AttackType attackType)
    {
        if (isDie) return;

        monsterClass.TakeDamage(damage, GetAttackType());
        Debug.Log($"�±��� ü�� {monsterClass.CurrentHealth + damage} ���� �� ü�� {monsterClass.CurrentHealth}");

        // ĳ�̵� uiManager ���
        if (uiManager != null)
        {
            uiManager.UpdateHealthUI(monsterClass.CurrentHealth);
            uiManager.SpawnDamageText(damage, attackType);
        }

        var monsterAI = GetComponent<MonsterAI>();
        if (monsterAI != null)
        {
            monsterAI.OnDamaged(damage, attackType);
        }

        if (monsterClass.CurrentHealth <= 0 && gameObject != null)
        {
            Die();
        }
    }
    public void TakeDotDamage(int dotDamage)
    {
        if (!isDie)
        {
            monsterClass.TakeDotDamage(dotDamage);

            if (monsterClass.CurrentHealth <= 0)
            {    
                Die();
            }
        }

    }
    public Transform GetSkillSpawnPoint()
    {
        return skillSpawnPoint;
    }
    // ���� ���ؿ��� �ִϸ��̼� Ʈ���� ����
    private void Die()
    {
        if (!isDie)
        {
            isDie = true;
            Destroy(gameObject); // ���� ������Ʈ ����
        }

    }
    public MonsterClass GetMonsterClass()
    {
        return monsterClass;
    }
}
