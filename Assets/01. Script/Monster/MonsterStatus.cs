using RPGCharacterAnims.Lookups;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static AttackData;

public class MonsterStatus : MonoBehaviour, IDamageable
{
    private MonsterClass monsterClass; // 데이터 관리용
    private bool isDie = false;
    private HitEffectsManager hitEffectsManager; // 연출 담당 컴포넌트
    private Transform skillSpawnPoint;

    private void Awake()
    {
        hitEffectsManager = GetComponent<HitEffectsManager>(); // HitEffectsManager 연결
        skillSpawnPoint = GetComponentsInChildren<Transform>()
          .FirstOrDefault(t => t.name == "SkillSpawnPoint");

        if (skillSpawnPoint == null)
        {
            Debug.LogWarning($"{gameObject.name}에서 SkillSpawnPoint를 찾지 못했습니다. 몬스터 위치를 사용합니다.");
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
        monsterClass.TakeDamage(damage,GetAttackType()); // MonsterClass에 데미지 적용h}");
        Debug.Log($"맞기전 체력 {monsterClass.CurrentHealth + damage} 맞은 후 체력 {monsterClass.CurrentHealth} ");// 공격 타입에 따라 연출 트리거
        if (attackType == AttackType.Normal)
        {
            hitEffectsManager.TriggerHitStop(0.1f); // 기본 공격 히트스탑
            hitEffectsManager.TriggerShake(0.2f, 0.2f); // 기본 공격 흔들림
                                                        // 카메라 흔들림 호출
            CameraShakeManager.TriggerShake(0.1f, 0.1f); // 기본 공격
        }
        else if (attackType == AttackType.Charge)
        {
            hitEffectsManager.TriggerHitStop(0.3f); // 강공격 히트스탑
            hitEffectsManager.TriggerShake(0.5f, 0.5f); // 강공격 흔들림
                                                        // 더 강한 카메라 흔들림 호출
            CameraShakeManager.TriggerShake(0.3f, 0.2f); // 강공격
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
            {    // 히트스탑과 같은 연출 중단
                if (hitEffectsManager != null)
                {
                    StopAllCoroutines(); // 모든 Coroutine 중단
                }
                Die();
            }
        }

    }
    public Transform GetSkillSpawnPoint()
    {
        return skillSpawnPoint;
    }
    // 지속 피해에는 애니메이션 트리거 없음
    private void Die()
    {
        if (!isDie)
        {
            isDie = true;
            Destroy(gameObject); // 몬스터 오브젝트 삭제
        }

    }
    public MonsterClass GetMonsterClass()
    {
        return monsterClass;
    }
}
