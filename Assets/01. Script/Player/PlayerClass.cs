using Sirenix.OdinInspector;
using System;
using UnityEngine;
using static AttackData;
[System.Serializable]
public class PlayerClass : ICreature, IDamageable
{
    #region 변수
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
    public bool isInvicible = false;
    public PlayerClassData _playerClassData;

    public Transform playerTransform;
    [ShowInInspector]
    public Stats PlayerStats { get; private set; }

    private bool isDead = false;

    Animator animator;
    #endregion
    #region 생성자 및 초기화
    public PlayerClass(PlayerClassData playerClassData, ICharacterAttack characterAttack, Transform playerTransform, Animator animator)
    {
        _playerClassData = playerClassData;
        this.playerTransform = playerTransform;
        this.animator = animator;

        // Stats 인스턴스 초기화 및 스탯 설정

        InitializeStats();

        // 기본 스탯 저장
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
            _playerClassData.characterStats.baseCriticalCance,
            _playerClassData.characterStats.damageReceiveRate
        );
    }
    #endregion
    public Stats GetStats() { return PlayerStats; }
    #region 테스트용
    public void Test()
    {
        Debug.Log($"현재 플레이어 스탯 : 체력 {PlayerStats.Health}, 마나: {PlayerStats.Mana}, 파워: {PlayerStats.AttackPower}, 공속: {PlayerStats.AttackSpeed}, 이속: {PlayerStats.Speed}, 치확: {PlayerStats.CriticalChance}, 데미지배율: {PlayerStats.DamageReceiveRate}");
    }

    // 지울 가능성 있음 현재 사용 x
    public void ChangeWeapon(IWeapon newWeapon)
    {
        currentWeapon = newWeapon;
        Debug.Log($"Player가 {currentWeapon}로 무기를 변경했습니다!");

        SelectWeapon(currentWeapon);
    }
    // 지울 가능성 있음 현재 사용 x
    public void SelectWeapon(IWeapon weapon)
    {
        currentWeapon = weapon;
        if (Enum.TryParse(weapon.GetType().Name, out WeaponType parsedWeaponType))
        {
            weaponType = parsedWeaponType;
            Debug.Log($"무기가 변경되었습니다: {weaponType}");
        }
        OnWeaponSelected?.Invoke(currentWeapon); // 무기 선택 이벤트 발생
    }
    public virtual void LevelUp()
    {
        // LevelUp 구현
    }

    public IWeapon GetCurrentWeapon()
    {
        return currentWeapon;
    }
    #endregion

    #region 스탯 변동 매서드
    public void ModifyPower(int healthAmount = 0,int maxHealth = 0, int manaAmount = 0, int attackAmount = 0, int attackSpeedAmount = 0, float speedAmount = 0, float criticalChanceAmount = 0, float damageReceive = 0)
    {
        PlayerStats.Health = Mathf.Clamp(PlayerStats.Health + healthAmount, 0, PlayerStats.MaxHealth);
        PlayerStats.MaxHealth = Mathf.Max(PlayerStats.MaxHealth + maxHealth, 1);
        PlayerStats.Mana = Mathf.Max(PlayerStats.Mana + manaAmount, 0);
        PlayerStats.AttackPower = Mathf.Max(PlayerStats.AttackPower + attackAmount, 0);
        PlayerStats.AttackSpeed = Mathf.Max(PlayerStats.AttackSpeed + attackSpeedAmount, 1);
        PlayerStats.Speed = Mathf.Max(PlayerStats.Speed + speedAmount, 0);
        PlayerStats.CriticalChance = Mathf.Clamp(PlayerStats.CriticalChance + criticalChanceAmount, 0, 100);
        PlayerStats.DamageReceiveRate = Mathf.Max(PlayerStats.DamageReceiveRate + damageReceive, 0);
    }

    public void ResetPower(bool health,bool maxHealth ,bool mana,bool attackPw,bool attackSp,bool speed,bool critical,bool damageReceive)
    {
        PlayerStats.ResetStats(health,maxHealth,mana,attackPw,attackSp,speed,critical, damageReceive);
    }
    #endregion


    #region 공격 및 피격 관리 매서드
    public virtual void Attack()
    {
        characterAttack?.BasicAttack();
    }

    public virtual void TakeDamage(int damage)
    {if (isInvicible) return;
       
        PlayerStats.Health -= damage;
       
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
        Debug.Log($"지속 피해: {dotDamage}, 남은 체력: {PlayerStats.Health}");

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
            Debug.Log("죽음");
            ResetPower(true, true, true, true, true, true, true, true);
        }
    }

    public DamageType GetDamageType()
    {
        Debug.Log("호출");
        return DamageType.Player;
        
    }
    #endregion

}
