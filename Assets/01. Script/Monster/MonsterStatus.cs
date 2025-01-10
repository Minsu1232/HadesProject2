using RPGCharacterAnims.Lookups;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static AttackData;
using static IMonsterState;

public class MonsterStatus : MonoBehaviour,IDamageable
{
    private MonsterClass monsterClass; // 데이터 관리용
    private bool isDie = false;
    
    [SerializeField] private Transform skillSpawnPoint;
    private MonsterUIManager uiManager; // MonsterUIManager 캐싱용 변수 추가
    private void Awake()
    {
        uiManager = GetComponent<MonsterUIManager>(); // 시작 시 한 번만 가져오기
       
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
        if (isDie) return;

        monsterClass.TakeDamage(damage, GetAttackType());
        Debug.Log($"맞기전 체력 {monsterClass.CurrentHealth + damage} 맞은 후 체력 {monsterClass.CurrentHealth}");

        // 캐싱된 uiManager 사용
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
