using RPGCharacterAnims.Lookups;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static AttackData;

public class MonsterStatus : MonoBehaviour, IDamageable
{
    private MonsterClass monsterClass; // ������ ������
    private bool isDie = false;
    private HitEffectsManager hitEffectsManager; // ���� ��� ������Ʈ
    private Transform skillSpawnPoint;

    private void Awake()
    {
        hitEffectsManager = GetComponent<HitEffectsManager>(); // HitEffectsManager ����
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
        monsterClass.TakeDamage(damage,GetAttackType()); // MonsterClass�� ������ ����h}");
        Debug.Log($"�±��� ü�� {monsterClass.CurrentHealth + damage} ���� �� ü�� {monsterClass.CurrentHealth} ");// ���� Ÿ�Կ� ���� ���� Ʈ����
        if (attackType == AttackType.Normal)
        {
            hitEffectsManager.TriggerHitStop(0.1f); // �⺻ ���� ��Ʈ��ž
            hitEffectsManager.TriggerShake(0.2f, 0.2f); // �⺻ ���� ��鸲
                                                        // ī�޶� ��鸲 ȣ��
            CameraShakeManager.TriggerShake(0.1f, 0.1f); // �⺻ ����
        }
        else if (attackType == AttackType.Charge)
        {
            hitEffectsManager.TriggerHitStop(0.3f); // ������ ��Ʈ��ž
            hitEffectsManager.TriggerShake(0.5f, 0.5f); // ������ ��鸲
                                                        // �� ���� ī�޶� ��鸲 ȣ��
            CameraShakeManager.TriggerShake(0.3f, 0.2f); // ������
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
            {    // ��Ʈ��ž�� ���� ���� �ߴ�
                if (hitEffectsManager != null)
                {
                    StopAllCoroutines(); // ��� Coroutine �ߴ�
                }
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
