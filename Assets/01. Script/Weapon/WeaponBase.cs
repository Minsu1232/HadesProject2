

using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public abstract class WeaponBase : WeaponManager, IWeaponCollider
{

    protected float comboDamageMultiplier = 1.0f; // 콤보 데미지 증폭 배율 (기본값: 1.0 = 100%)
    protected float gageChargeMultiplier = 1.0f;
    protected abstract override void InitializeComponents();
    // IWeapon 구현
    public abstract override void OnAttack(Transform origin, int comboStep);


    public override int GetDamage(int _baseDamage, int comboStep)
    {
        int playerDamage = GameInitializer.Instance.GetPlayerClass().PlayerStats.AttackPower;

        // 콤보 스텝이 1보다 클 때만 배율을 적용 (첫 공격에는 적용하지 않음)
        if (comboStep > 1)
        {
            return Mathf.RoundToInt(comboStep * (BaseDamage + playerDamage) * comboDamageMultiplier);
        }
        else
        {
            return comboStep * (BaseDamage + playerDamage);
        }
    }

    public override int GetChargeDamage()
    {
        int playerDamage = chargeComponent.GetChargeDamage();
        
        return playerDamage;
    }
    // 차징 관련

    public abstract override void SpecialAttack();

    // IWeaponCollider 구현
    public virtual void ActivateCollider()
    {
        if (weaponCollider != null)
        {
            weaponCollider.enabled = true;

        }
    }

    public virtual void DeactivateCollider()
    {
        if (weaponCollider != null)
        {
            weaponCollider.enabled = false;
            MeleeDamageDealer damageDealer = GetComponentInChildren<MeleeDamageDealer>();
            if (damageDealer != null)
            {
                damageDealer.ClearDamagedMonsters();
            }
        }
    }
    public void OnChargeAttackEnd()
    {       
       
            EndChargeAttack();          
        

    }
    // WeaponBase 클래스에 추가해야 할 메서드
    public void SetGaugeChargeMultiplier(float multiplier)
    {
        // GagePerHit 값을 조정하는 대신 게이지 획득량을 조정하는 내부 변수 추가 필요
        gageChargeMultiplier = Mathf.Max(1.0f, multiplier); // 최소 1.0 (100%)
        Debug.Log($"무기 {GetType().Name}의 게이지 충전 배율 설정: {gageChargeMultiplier * 100}%");
    }

    // GetGage 메서드 오버라이딩 (WeaponManager의 GetGage 메서드를 오버라이드)
    public override void GetGage(int amount)
    {
        // 배율을 적용하여 게이지 증가
        int boostedAmount = Mathf.RoundToInt(amount * gageChargeMultiplier);
        base.GetGage(boostedAmount);
    }
    public void SetComboDamageMultiplier(float multiplier)
    {
        comboDamageMultiplier = Mathf.Max(1.0f, multiplier); // 최소 1.0 (100%)
        Debug.Log($"무기 {GetType().Name}의 콤보 데미지 배율 설정: {comboDamageMultiplier * 100}%");
    }

    // 4. 콤보 데미지 배율 반환 메서드 추가
    public float GetComboDamageMultiplier()
    {
        return comboDamageMultiplier;
    }

}
  