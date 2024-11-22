using DG.Tweening;
using RPGCharacterAnims.Lookups;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AttackData;

public class PlayerClass : ICreature, IDamageable
{

    #region ������
    public enum WeaponType
    {
        None,
        GreatSword,
        Sword,
        Staff
    }

    public WeaponType weaponType;
    protected IWeapon currentWeapon;
    protected ICharacterAttack characterAttack;

    public PlayerClassData _playerClassData;

    public Rigidbody rb;
    public Transform playerTransform;
    public int CurrentHealth { get; private set; }
    public int CurrentMana { get; private set; }
    public int CurrentAttackPower { get; private set; }
    public int CurrentAttackSpeed { get; private set; }
    public float CurrentSpeed { get; private set; }
    public float CurrentCriticalChance { get; private set; }
    public float CurrentUpgradeCount { get; private set; }
    private int defaultHealth;
    private int defaultMana;
    private int defaultAttackPower;
    private int defaultAttackSpeed;
    private float defaultSpeed;
    private float defaultCriticalChance;
    public int Level { get; private set; }

    protected bool isDashing = false;
    protected bool isDead = false;

    Animator animator;
    #endregion

    public PlayerClass(PlayerClassData playerClassData, ICharacterAttack characterAttack, Rigidbody rb, Transform playerTransform, Animator animator)
    {
        _playerClassData = playerClassData;
        InitializeStats();
        this.rb = rb;
        this.playerTransform = playerTransform;
        this.animator = animator;

        // �ʱⰪ ����
        SaveDefaultStats();
    }
    public void Test()
    {
        Debug.Log($"���� �÷��̾� ���� :ü��{CurrentHealth} ���� : {CurrentMana} �Ŀ� {CurrentAttackPower} ���� {CurrentAttackSpeed} �̼� {CurrentSpeed} ġȮ {CurrentCriticalChance} ");
    }
    public void ChangeWeapon(IWeapon newWeapon)
    {
        currentWeapon = newWeapon;
        Debug.Log($"Player�� {currentWeapon}�� ���⸦ �����߽��ϴ�!");

        SelectWeapon(currentWeapon);
    }

    public void SelectWeapon(IWeapon weapon)
    {
        currentWeapon = weapon;
        if (Enum.TryParse(weapon.GetType().Name, out WeaponType parsedWeaponType))
        {
            weaponType = parsedWeaponType;
            Debug.Log($"���Ⱑ ����Ǿ����ϴ�: {weaponType}");
        }
       
    }

    private void InitializeStats() // �ʱ�ȭ
    {
        CurrentHealth = _playerClassData.characterStats.baseHp;
        CurrentMana = _playerClassData.characterStats.baseGage;
        CurrentAttackPower = _playerClassData.characterStats.baseAttackPower;
        CurrentAttackSpeed = (int)_playerClassData.characterStats.baseAttackSpeed;
        CurrentSpeed = _playerClassData.characterStats.baseSpeed;
        CurrentCriticalChance = _playerClassData.characterStats.baseCriticalCance;
        CurrentUpgradeCount = _playerClassData.characterStats.upgradeCount;
    }
    public void SaveDefaultStats() // ���� ����� ��������
    {
        defaultHealth = CurrentHealth;
        defaultMana = CurrentMana;
        defaultAttackPower = CurrentAttackPower;
        defaultAttackSpeed = CurrentAttackSpeed;
        defaultSpeed = CurrentSpeed;
        defaultCriticalChance = CurrentCriticalChance;
    }
    // �ɷ�ġ ���� �޼���
    public void ResetPower()
    {
        CurrentHealth = defaultHealth;
        CurrentMana = defaultMana;
        CurrentAttackPower = defaultAttackPower;
        CurrentAttackSpeed = defaultAttackSpeed;
        CurrentSpeed = defaultSpeed;
        CurrentCriticalChance = defaultCriticalChance;

        
    }
    public void ModifyPower(int healthAmount = 0, int manaAmount = 0, int attackAmount = 0, int attackSpeedAmount = 0, int speedAmount = 0, float criticalChance = 0)
    {
        CurrentHealth += healthAmount;
        CurrentMana += manaAmount;
        CurrentAttackPower += attackAmount;
        CurrentAttackSpeed += attackSpeedAmount;
        CurrentSpeed += speedAmount;
        CurrentCriticalChance += criticalChance;
    }

    public void Dash(Vector3 direction)
    {
        if (isDashing) return;
        isDashing = true;

        Vector3 dashTarget = rb.position + direction; // ���� ��� ��ǥ ����


        rb.DOMove(dashTarget, 0.2f)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {

                isDashing = false;
            });
    }

    public void LevelUp()
    {
        // LevelUp ����
    }

    public virtual void Attack()
    {
        characterAttack?.BasicAttack();
    }  

    public virtual void TakeDamage(int damage, AttackType attackType)
    {
        CurrentHealth -= damage;
        if (CurrentHealth <= 0)
        {
            CurrentHealth = 0;
            Die();
        }
        else
        {
            //animator?.SetTrigger("Hit");
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
    }

    public virtual void Die()
    {
        if (!isDead)
        {
            isDead = true;
            animator?.SetTrigger("Die");
            Debug.Log("����");
        }
    }
}
