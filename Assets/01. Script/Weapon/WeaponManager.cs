using RPGCharacterAnims.Lookups;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using static AttackData;
/// <summary>
/// 모든 무가들의 기반 클래스
/// </summary>
public abstract class WeaponManager : MonoBehaviour, IWeapon
{
    #region 변수들
    [SerializeField] public WeaponScriptableObject weaponData;
    protected WeaponChargeBase chargeComponent;
    protected SpecialAttackBase specialAttackComponent;

    // 초기화된 속성값
    protected GameObject _weaponInstance;
    protected Collider weaponCollider;
    //protected Animator weaponAnimator;


    public event Action<int> OnGageChanged; // 게이지 변경 이벤트

    protected float gageRetentionRate = 0f;  // 게이지 보존율 (0.0 ~ 1.0)
    private int currentGage;

    public int CurrentGage
    {
        get => currentGage;
        protected set
        {
            currentGage = Mathf.Clamp(value, 0, 100); // 0~100 범위로 제한
            OnGageChanged?.Invoke(currentGage); // 게이지 변경 시 이벤트 호출
        }
    }

    // 속성값 접근자
    public string WeaponName => weaponData?.weaponName ?? "Unknown";
    public int BaseDamage => weaponData != null ? weaponData.baseDamage + weaponData.additionalDamage : 0;
    public int GagePerHit => weaponData != null ? weaponData.baseGagePerHit + weaponData.additionalGagePerHit : 0;
    public float MaxChargeTime => weaponData?.maxChargeTime ?? 0;
    public float ChargeMultiplier => weaponData?.chargeMultiplier ?? 0;   

    public bool IsChargeAttack { get; private set; } = false;
    public bool IsSpecialAttack { get; set; } = false;

    public bool CanUseSpecialAttack => currentGage >= 100;

    public Color GageColor => weaponData?.gageColor ?? Color.yellow;



    #endregion


    
    public virtual void InitializeWeapon(Animator animator) // 무기 초기화
    {
        LoadAnimatorOverride(WeaponName, animator);
       
    }
    public AttackType GetAttackType() // 무기 어택타입 반환
    {
        return IsChargeAttack ? AttackType.Charge : AttackType.Normal;
    }
    // 무기 모델 로드
    public virtual GameObject WeaponLoad(Transform parentTransform) // 무기 소환 및 장착
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
    public virtual void LoadAnimatorOverride(string weaponName, Animator animator) // 무기에 맞는 애니메이션컨트롤러 장착
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
    public abstract void OnAttack(Transform origin, int comboStep); //기본공격(콤보를 받기위해)

    // 게이지 시스템
    public virtual void GetGage(int amount) // 게이지 획득
    {
        if (!specialAttackComponent.isSpecialAttack) // 스킬 실행중엔 게이지 획득 x
        {
            CurrentGage += amount; // CurrentGage를 통해 값 변경
            Debug.Log($"{WeaponName} 게이지 증가: {CurrentGage}/100");
        }
       
    }
    public abstract void SpecialAttack(); //무기별 고유 공격

    #region 차징

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
    #endregion 
    protected abstract void InitializeComponents(); // 무기별 속성(근접이나 원기리 등) 컴포넌트를 받기 위한 매서드
 

    public abstract int GetDamage(int _baseDamage, int comboStep); // 데미지 계산 매서드

    public abstract int GetChargeDamage(); // 차지 데미지 게산 매서드

  

    /// <summary>
    /// Ability
    /// </summary>
    /// <param name="rate"></param>
    // 게이지 보존율 설정 메서드 구현
    public virtual void SetGageRetentionRate(float rate)
    {
        gageRetentionRate = Mathf.Clamp01(rate);  // 0.0-1.0 범위로 제한
        Debug.Log($"{WeaponName}의 게이지 보존율 설정: {gageRetentionRate * 100}%");
    }
    // ResetGage 메서드 수정 (게이지 리셋 시 보존 로직 적용)
    public virtual void ResetGage(int currentGage_)
    {
        // 스킬 사용 시 게이지 보존 효과 적용
        if (currentGage_ == 0 && gageRetentionRate > 0f)
        {
            // 특수 스킬 사용 후 게이지 보존
            int retainedGage = Mathf.RoundToInt(100 * gageRetentionRate);
            CurrentGage = retainedGage;
            Debug.Log($"게이지 보존 효과 적용: {retainedGage}/100 게이지 보존됨");
        }
        else
        {
            // 일반적인 경우 (지정된 값으로 게이지 설정)
            CurrentGage = currentGage_;
        }
    }

}
