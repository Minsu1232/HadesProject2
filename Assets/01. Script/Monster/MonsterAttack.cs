using RPGCharacterAnims.Lookups;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AttackData;

public class MonsterAttack : MonoBehaviour
{
    private float attackRange;
    private float attackSpeed;
    private float lastAttackTime;
    private Transform player;
    private PlayerClass playerClass;
    private MonsterClass monsterClass;
    private bool isAttacking = false;
    MonsterData md;
    private void Start()
    {
        playerClass = GameInitializer.Instance.GetPlayerClass();
        monsterClass = DungeonManager.Instance.GetMonsterClass();
        player = DungeonManager.Instance.GetPlayerTransform();  // 플레이어 참조 가져오기

        md = monsterClass.GetMonsterData();
        attackRange = monsterClass.CurrentAttackRange;
        attackSpeed = monsterClass.CurrentAttackSpeed;

        Debug.Log($"{attackSpeed} = 어택스피트");


   
    }

    private void Update()
    {
        if (player == null) return;
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        // 공격 범위 내에 있고 쿨다운이 끝났으면 공격
        if (distanceToPlayer <= attackRange && Time.time >= lastAttackTime + attackSpeed && !isAttacking)
        {
            Attack();
        }
    }
    public AttackType GetAttackType()
    {
        return monsterClass.IsBasicAttack() ? AttackType.Normal : AttackType.Charge;
    }
    private void Attack()
    {
        lastAttackTime = Time.time;
        
        
        isAttacking = true;
        ApplyDamage(playerClass);
    }

    public void ApplyDamage(IDamageable target)
    {
        if (target != null)
        {
            int damageAmount = monsterClass.CurrentAttackPower;
            target.TakeDamage(damageAmount, GetAttackType());
            Debug.Log($"{monsterClass.GetName()}이 {damageAmount}만큼의 데미지를 {target}에게 입혔습니다.");

            isAttacking = false;
        }
        else
        {
            Debug.LogWarning("ApplyDamage 호출 시 대상이 null입니다.");
        }
    }
}
