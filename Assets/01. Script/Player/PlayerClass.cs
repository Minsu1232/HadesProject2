using Sirenix.OdinInspector;
using System;
using UnityEngine;
using static AttackData;
[System.Serializable]
public class PlayerClass : ICreature, IDamageable
{
    #region ����
    public event Action<IWeapon> OnWeaponSelected;
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

    public Transform playerTransform;
    [ShowInInspector]
    public Stats PlayerStats { get; private set; }

    private bool isDead = false;

    Animator animator;
    #endregion
    #region ������ �� �ʱ�ȭ
    public PlayerClass(PlayerClassData playerClassData, ICharacterAttack characterAttack, Transform playerTransform, Animator animator)
    {
        _playerClassData = playerClassData;
        this.playerTransform = playerTransform;
        this.animator = animator;

        // Stats �ν��Ͻ� �ʱ�ȭ �� ���� ����

        InitializeStats();

        // �⺻ ���� ����
        PlayerStats.SaveDefaultStats();
    }

    private void InitializeStats()
    {
        PlayerStats = new Stats(
            _playerClassData.characterStats.baseHp,
            _playerClassData.characterStats.baseGage,
            _playerClassData.characterStats.baseAttackPower,
            (int)_playerClassData.characterStats.baseAttackSpeed,
            _playerClassData.characterStats.baseSpeed,
            _playerClassData.characterStats.baseCriticalCance
        );
    }
    #endregion

    #region �׽�Ʈ��
    public void Test()
    {
        Debug.Log($"���� �÷��̾� ���� : ü�� {PlayerStats.Health}, ����: {PlayerStats.Mana}, �Ŀ�: {PlayerStats.AttackPower}, ����: {PlayerStats.AttackSpeed}, �̼�: {PlayerStats.Speed}, ġȮ: {PlayerStats.CriticalChance}");
    }

    // ���� ���ɼ� ���� ���� ��� x
    public void ChangeWeapon(IWeapon newWeapon)
    {
        currentWeapon = newWeapon;
        Debug.Log($"Player�� {currentWeapon}�� ���⸦ �����߽��ϴ�!");

        SelectWeapon(currentWeapon);
    }
    // ���� ���ɼ� ���� ���� ��� x
    public void SelectWeapon(IWeapon weapon)
    {
        currentWeapon = weapon;
        if (Enum.TryParse(weapon.GetType().Name, out WeaponType parsedWeaponType))
        {
            weaponType = parsedWeaponType;
            Debug.Log($"���Ⱑ ����Ǿ����ϴ�: {weaponType}");
        }
        OnWeaponSelected?.Invoke(currentWeapon); // ���� ���� �̺�Ʈ �߻�
    }
    public virtual void LevelUp()
    {
        // LevelUp ����
    }

    public IWeapon GetCurrentWeapon()
    {
        return currentWeapon;
    }
    #endregion

    #region ���� ���� �ż���
    public void ModifyPower(int healthAmount = 0,int maxHealth = 0, int manaAmount = 0, int attackAmount = 0, int attackSpeedAmount = 0, float speedAmount = 0, float criticalChanceAmount = 0)
    {
        PlayerStats.Health += healthAmount;
        PlayerStats.MaxHealth += maxHealth;
        PlayerStats.Mana += manaAmount;
        PlayerStats.AttackPower += attackAmount;
        PlayerStats.AttackSpeed += attackSpeedAmount;
        PlayerStats.Speed += speedAmount;
        PlayerStats.CriticalChance += criticalChanceAmount;
    }

    public void ResetPower(bool health,bool maxHealth ,bool mana,bool attackPw,bool attackSp,bool speed,bool critical)
    {
        PlayerStats.ResetStats(health,maxHealth,mana,attackPw,attackSp,speed,critical);
    }
    #endregion


    #region ���� �� �ǰ� ���� �ż���
    public virtual void Attack()
    {
        characterAttack?.BasicAttack();
    }

    public virtual void TakeDamage(int damage, AttackType attackType)
    {
        PlayerStats.Health -= damage;
        Debug.Log($"���� �� ü�� : {PlayerStats.Health}");
        if (PlayerStats.Health <= 0)
        {
            PlayerStats.Health = 0;
            Die();
        }
        else
        {
            // animator?.SetTrigger("Hit");
        }
    }

    public virtual void TakeDotDamage(int dotDamage)
    {
        PlayerStats.Health -= dotDamage;
        Debug.Log($"���� ����: {dotDamage}, ���� ü��: {PlayerStats.Health}");

        if (PlayerStats.Health <= 0)
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
    #endregion
   
}
