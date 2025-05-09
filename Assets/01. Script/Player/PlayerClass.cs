using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using UnityEngine;
using static AttackData;
[System.Serializable]
public class PlayerClass : ICreature, IDamageable
{ // 데미지 이벤트 선언 (데미지량과 공격자 정보 포함)
    public event System.Action<int, ICreatureStatus> OnDamageReceived;
    #region 변수
    public event Action<IWeapon> OnWeaponSelected;
    public enum WeaponType
    {
        None,
        GreatSword,
        Chronofracture,
        Staff
    }

    public WeaponType weaponType;
    protected IWeapon currentWeapon;
    protected ICharacterAttack characterAttack;
    public bool isInvicible = false;
    public PlayerClassData _playerClassData;

    private bool isStunned = false;
    public bool IsStunned => isStunned;
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
     _playerClassData.characterStats.GetCalculatedHP(),
     _playerClassData.characterStats.GetCalculatedGage(),
     _playerClassData.characterStats.GetCalculatedAttackPower(),
     (int)_playerClassData.characterStats.GetCalculatedAttackSpeed(),
     _playerClassData.characterStats.GetCalculatedSpeed(),
     _playerClassData.characterStats.GetCalculatedCriticalChance(),
     _playerClassData.characterStats.GetCalculatedDamageReceiveRate()
 );
    }
    #endregion
    public Stats GetStats() { return PlayerStats; }
    #region 테스트용
    public void Test()
    {
        Debug.Log($"현재 플레이어 스탯 : 체력 {PlayerStats.Health}, 마나: {PlayerStats.Mana}, 파워: {PlayerStats.AttackPower}, 공속: {PlayerStats.AttackSpeed}, 이속: {PlayerStats.Speed}, 치확: {PlayerStats.CriticalChance}, 데미지배율: {PlayerStats.DamageReceiveRate}");
    }

   
    public void ChangeWeapon(IWeapon newWeapon)
    {
        currentWeapon = newWeapon;
        Debug.Log($"Player가 {currentWeapon}로 무기를 변경했습니다!");

        SelectWeapon(currentWeapon);
    }
   
    public void SelectWeapon(IWeapon weapon)
    {
        currentWeapon = weapon;
      
        
            weaponType = weapon.weaponType;
            Debug.Log($"무기가 변경되었습니다: {weaponType}");
        
        OnWeaponSelected?.Invoke(currentWeapon); // 무기 선택 이벤트 발생
    }
    public void ClearWeapon()
    {
        currentWeapon = null;
        OnWeaponSelected?.Invoke(null); // null을 전달하여 무기가 해제되었음을 알림
        weaponType = WeaponType.None;
        Debug.Log("플레이어의 무기가 해제되었습니다.");
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
    public void ModifyPower(int healthAmount = 0, int maxHealth = 0, int manaAmount = 0, int attackAmount = 0, int attackSpeedAmount = 0, float speedAmount = 0, float criticalChanceAmount = 0, float damageReceive = 0)
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

    public void ResetPower(bool health, bool maxHealth, bool mana, bool attackPw, bool attackSp, bool speed, bool critical, bool damageReceive)
    {
        PlayerStats.ResetStats(health, maxHealth, mana, attackPw, attackSp, speed, critical, damageReceive);
        
    }
    #endregion
    // PlayerClass에 추가
    public void UpgradeHP(int count = 1)
    {
        _playerClassData.characterStats.hpUpgradeCount += count;
        // 스탯 재계산
        RefreshStats();
        // 저장
        SaveManager.Instance.UpdatePlayerStats(PlayerStats);
    }

    public void UpgradeAttackPower(int count = 1)
    {
        _playerClassData.characterStats.attackPowerUpgradeCount += count;
        RefreshStats();
        SaveManager.Instance.UpdatePlayerStats(PlayerStats);
    }

    public void UpgradeGage(int count = 1)
    {
        _playerClassData.characterStats.gageUpgradeCount += count;
        RefreshStats();
        SaveManager.Instance.UpdatePlayerStats(PlayerStats);
    }

    public void UpgradeAttackSpeed(int count = 1)
    {
        _playerClassData.characterStats.attackSpeedUpgradeCount += count;
        RefreshStats();
        SaveManager.Instance.UpdatePlayerStats(PlayerStats);
    }

    public void UpgradeSpeed(int count = 1)
    {
        // 카운트 증가
        _playerClassData.characterStats.speedUpgradeCount += count;

        // RefreshStats()를 통해 새로운 스탯 계산 및 적용
        RefreshStats();

        // 저장
        SaveManager.Instance.UpdatePlayerStats(PlayerStats);
    }

    public void UpgradeCriticalChance(int count = 1)
    {
        _playerClassData.characterStats.criticalChanceUpgradeCount += count;
        RefreshStats();
        SaveManager.Instance.UpdatePlayerStats(PlayerStats);
    }

    public void UpgradeDamageReduce(int count = 1)
    {
        _playerClassData.characterStats.damageReduceUpgradeCount += count;
        RefreshStats();
        SaveManager.Instance.UpdatePlayerStats(PlayerStats);
    }

    // 스탯 새로고침 메서드
    private void RefreshStats()
    {
        PlayerStats.MaxHealth = _playerClassData.characterStats.GetCalculatedHP();
        PlayerStats.Health = PlayerStats.MaxHealth;

        //PlayerStats.MaxMana = _playerClassData.characterStats.GetCalculatedGage();
        PlayerStats.Mana = PlayerStats.MaxMana;

        PlayerStats.AttackPower = _playerClassData.characterStats.GetCalculatedAttackPower();
        PlayerStats.AttackSpeed = (int)_playerClassData.characterStats.GetCalculatedAttackSpeed();
        PlayerStats.Speed = _playerClassData.characterStats.GetCalculatedSpeed();
        PlayerStats.CriticalChance = _playerClassData.characterStats.GetCalculatedCriticalChance();
        PlayerStats.DamageReceiveRate = _playerClassData.characterStats.GetCalculatedDamageReceiveRate();
    }

    #region 공격 및 피격 관리 매서드
    public virtual void Attack()
    {
        characterAttack?.BasicAttack();
    }

    public virtual void TakeDamage(int damage)
    {
        if (isInvicible)
        {
            damage = 0;
            return;
        }

        // 반격 처리 (데미지 적용 전)
        // 데미지 적용 전 이벤트 발생

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
    // 공격자 정보가 있는 오버로드 메서드 추가
    public void PlayerGetAttacker(int damage, ICreatureStatus attacker)
    {
        if (isInvicible) return;

        // 데미지 적용 전 이벤트 발생 (공격자 정보 포함)
        OnDamageReceived?.Invoke(damage, attacker);

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
            isInvicible = true;
            animator?.SetTrigger("Die");            
            Debug.Log("죽음");
            DeathHandler.HandlePlayerDeath();            
            isDead = false;
        }
    }

    public DamageType GetDamageType()
    {
        Debug.Log("호출");
        return DamageType.Player;

    }
    public void ApplyStun(float stunDuration)
    {
        if (!isStunned)
        {
            isStunned = true;
            animator?.SetBool("IsStun", true);  // 먼저 bool 설정
            animator?.SetTrigger("Stun");       // 그 다음 트리거

            DOVirtual.DelayedCall(stunDuration, () =>
            {
                isStunned = false;
                animator?.SetBool("IsStun", false);
            });
        }
    }
    public void SetInvicibleToFalse()
    {
        isInvicible = false;
    }
    #endregion

}
