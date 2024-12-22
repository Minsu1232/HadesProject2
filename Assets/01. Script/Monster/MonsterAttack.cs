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
        player = DungeonManager.Instance.GetPlayerTransform();  // �÷��̾� ���� ��������

        md = monsterClass.GetMonsterData();
        attackRange = monsterClass.CurrentAttackRange;
        attackSpeed = monsterClass.CurrentAttackSpeed;

        Debug.Log($"{attackSpeed} = ���ý���Ʈ");


   
    }

    private void Update()
    {
        if (player == null) return;
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        // ���� ���� ���� �ְ� ��ٿ��� �������� ����
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
            Debug.Log($"{monsterClass.GetName()}�� {damageAmount}��ŭ�� �������� {target}���� �������ϴ�.");

            isAttacking = false;
        }
        else
        {
            Debug.LogWarning("ApplyDamage ȣ�� �� ����� null�Դϴ�.");
        }
    }
}
