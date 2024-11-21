using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AttackData;

public abstract class MonsterClass : ICreature,IDamageable
{
    private MonsterData monsterData;
    
    public string MONSTERNAME { get; private set; }
    public int CurrentHealth { get; private set; }
    public int CurrentDeffense { get; private set; }
    public int CurrentAttackPower { get; private set; }
    public float CurrentAttackSpeed { get; private set; }
    public int CurrentSpeed { get; private set; }
    public float CurrentAttackRange {  get; private set; }  

    public int CurrentMoveRange {  get; private set; }
    public int CurrentChaseRange { get; private set; }

    public bool isBasicAttack { get; private set; }

    protected bool isDashing = false;
   
    public MonsterClass(MonsterData data)
    {
        monsterData = data;
        InitializeStats();
    }
    public bool IsBasicAttack()
    {
        return isBasicAttack;
    }
    private void InitializeStats()
    {
        CurrentHealth = monsterData.initialHp;
        CurrentAttackPower = monsterData.initialAttackPower;
        CurrentAttackSpeed = monsterData.initialAttackSpeed;
        CurrentSpeed = monsterData.initialSpeed;
        MONSTERNAME = monsterData.monsterName;
        CurrentAttackRange = monsterData.attackRange;
        CurrentAttackSpeed = monsterData.initialAttackSpeed;
        CurrentMoveRange = monsterData.moveRange;
        CurrentChaseRange = monsterData.chaseRange;
    }
    public MonsterData GetMonsterData()
    {
        return monsterData;
    }
    public string GetName()
    {
        return monsterData.monsterName;
    }
    
    protected virtual void Debuff()
    {
        GameInitializer.Instance.GetPlayerClass();
    }
    protected void ModifyPower(int healthAmount = 0, int defenseAmount = 0, int attackAmount = 0, int attackSpeedAmount = 0, int speedAmount = 0)
    {
        CurrentHealth += healthAmount;
        CurrentDeffense += defenseAmount;
        CurrentAttackPower += attackAmount;
        CurrentAttackSpeed += attackSpeedAmount;
        CurrentSpeed += speedAmount;
    }
    public abstract void Attack();  // ���� �޼��� ����
    public virtual void TakeDamage(int damage, AttackType attackType)
    {
        CurrentHealth -= damage;
        if (CurrentHealth <= 0)
        {
            CurrentHealth = 0;
            Die();
        }
    }
    public virtual void TakeDotDamage(int dotDamage)
    {
        CurrentHealth -= dotDamage;
        Debug.Log($"���� ����: {dotDamage}, ���� ü��: {CurrentHealth}");

        if (CurrentHealth <= 0)
        {
            Die();
        }
        // ���� ���ؿ��� �ִϸ��̼� Ʈ���� ����
    }
    public abstract void Die();
 
    

    public virtual void SetPosition(Vector3 spawnPosition)
    {
        throw new NotImplementedException();
    }
}
