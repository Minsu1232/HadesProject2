using RPGCharacterAnims.Lookups;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using static AttackData;

public abstract class WeaponManager : MonoBehaviour, IWeapon
{
    
    [SerializeField] public WeaponScriptableObject weaponData;
    protected WeaponChargeBase chargeComponent;
    protected SpecialAttackBase specialAttackComponent;

    // 초기화된 속성값
    protected GameObject _weaponInstance;
    protected Collider weaponCollider;
    //protected Animator weaponAnimator;
    protected int currentGage; // 현재 게이지

    // 속성값 접근자
    public string WeaponName => weaponData?.weaponName ?? "Unknown";
    public int BaseDamage => weaponData != null ? weaponData.baseDamage + weaponData.additionalDamage : 0;
    public int GagePerHit => weaponData != null ? weaponData.baseGagePerHit + weaponData.additionalGagePerHit : 0;
    public float MaxChargeTime => weaponData?.maxChargeTime ?? 0;
    public float ChargeMultiplier => weaponData?.chargeMultiplier ?? 0;   

    public bool IsChargeAttack { get; private set; } = false;
    public bool IsSpecialAttack { get; set; } = false;

    public bool CanUseSpecialAttack => currentGage >= 100;
    // 초기화
    public virtual void InitializeWeapon(Animator animator)
    {
        LoadAnimatorOverride(WeaponName, animator);
       
    }
    public AttackType GetAttackType()
    {
        return IsChargeAttack ? AttackType.Charge : AttackType.Normal;
    }
    // 무기 모델 로드
    public virtual GameObject WeaponLoad(Transform parentTransform)
    {
        Addressables.LoadAssetAsync<GameObject>(weaponData.weaponName).Completed += handle =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded && handle.Result != null)
            {
                _weaponInstance = Instantiate(handle.Result, parentTransform);
                _weaponInstance.transform.localPosition = weaponData.defaultPosition;
                _weaponInstance.transform.localEulerAngles = weaponData.defaultRotation;

                weaponCollider = _weaponInstance.GetComponent<Collider>();

                if (weaponCollider != null)
                {
                    weaponCollider.enabled = false;
                    weaponCollider.isTrigger = true;
                }
                InitializeComponents();
                Debug.Log($"무기 모델 로드 완료: {WeaponName}");
            }
        };
        return _weaponInstance;
    }
    public virtual void LoadAnimatorOverride(string weaponName, Animator animator)
    {
        if (animator == null)
        {
            Debug.LogWarning($"Animator가 null입니다. {weaponName}의 애니메이션을 로드하지 않습니다.");
            return; // null이면 종료
        }
        Addressables.LoadAssetAsync<RuntimeAnimatorController>($"{weaponName}Controller").Completed += handle =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded && handle.Result != null)
            {
                animator.runtimeAnimatorController = handle.Result;
                Debug.Log($"애니메이터 로드 완료: {weaponName}Controller");
            }
            else
            {
                Debug.LogError($"애니메이터 로드 실패: {weaponName}Controller");
            }
        };
    }
    // 공격 로직
    public abstract void OnAttack(Transform origin, int comboStep);

    // 게이지 시스템
    public virtual void GetGage(int amount)
    {
        currentGage += amount;
        if (currentGage > 100) currentGage = 100;
        Debug.Log($"{WeaponName} 게이지 증가: {currentGage}/100");
    }

    public abstract void SpecialAttack();

    // 차징 시스템
    public virtual void StartCharge()
    {
        IsChargeAttack = true;
        chargeComponent?.StartCharge();
    }

    public virtual void UpdateCharge(float deltaTime)
    {
        chargeComponent?.UpdateCharge(deltaTime);
    }

    public virtual void ReleaseCharge()
    {        
        chargeComponent?.ReleaseCharge();
    }
    public void EndChargeAttack()
    {
        IsChargeAttack = false;
    }
    protected abstract void InitializeComponents();
 

    public abstract int GetDamage(int _baseDamage, int comboStep);

    public abstract int GetChargeDamage();

    public void ResetGage()
    {
        currentGage = 0;
    }

}
