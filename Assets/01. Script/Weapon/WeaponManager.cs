using RPGCharacterAnims.Lookups;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using static AttackData;
/// <summary>
/// ��� �������� ��� Ŭ����
/// </summary>
public abstract class WeaponManager : MonoBehaviour, IWeapon
{
    #region ������
    [SerializeField] public WeaponScriptableObject weaponData;
    protected WeaponChargeBase chargeComponent;
    protected SpecialAttackBase specialAttackComponent;

    // �ʱ�ȭ�� �Ӽ���
    protected GameObject _weaponInstance;
    protected Collider weaponCollider;
    //protected Animator weaponAnimator;


    public event Action<int> OnGageChanged; // ������ ���� �̺�Ʈ

    protected float gageRetentionRate = 0f;  // ������ ������ (0.0 ~ 1.0)
    private int currentGage;

    public int CurrentGage
    {
        get => currentGage;
        protected set
        {
            currentGage = Mathf.Clamp(value, 0, 100); // 0~100 ������ ����
            OnGageChanged?.Invoke(currentGage); // ������ ���� �� �̺�Ʈ ȣ��
        }
    }

    // �Ӽ��� ������
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


    
    public virtual void InitializeWeapon(Animator animator) // ���� �ʱ�ȭ
    {
        LoadAnimatorOverride(WeaponName, animator);
       
    }
    public AttackType GetAttackType() // ���� ����Ÿ�� ��ȯ
    {
        return IsChargeAttack ? AttackType.Charge : AttackType.Normal;
    }
    // ���� �� �ε�
    public virtual GameObject WeaponLoad(Transform parentTransform) // ���� ��ȯ �� ����
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
                Debug.Log($"���� �� �ε� �Ϸ�: {WeaponName}");
            }
        };
        return _weaponInstance;
    }
    public virtual void LoadAnimatorOverride(string weaponName, Animator animator) // ���⿡ �´� �ִϸ��̼���Ʈ�ѷ� ����
    {
        if (animator == null)
        {
            Debug.LogWarning($"Animator�� null�Դϴ�. {weaponName}�� �ִϸ��̼��� �ε����� �ʽ��ϴ�.");
            return; // null�̸� ����
        }
        Addressables.LoadAssetAsync<RuntimeAnimatorController>($"{weaponName}Controller").Completed += handle =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded && handle.Result != null)
            {
                animator.runtimeAnimatorController = handle.Result;
                Debug.Log($"�ִϸ����� �ε� �Ϸ�: {weaponName}Controller");
            }
            else
            {
                Debug.LogError($"�ִϸ����� �ε� ����: {weaponName}Controller");
            }
        };
    }
    // ���� ����
    public abstract void OnAttack(Transform origin, int comboStep); //�⺻����(�޺��� �ޱ�����)

    // ������ �ý���
    public virtual void GetGage(int amount) // ������ ȹ��
    {
        if (!specialAttackComponent.isSpecialAttack) // ��ų �����߿� ������ ȹ�� x
        {
            CurrentGage += amount; // CurrentGage�� ���� �� ����
            Debug.Log($"{WeaponName} ������ ����: {CurrentGage}/100");
        }
       
    }
    public abstract void SpecialAttack(); //���⺰ ���� ����

    #region ��¡

    // ��¡ �ý���
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
    protected abstract void InitializeComponents(); // ���⺰ �Ӽ�(�����̳� ���⸮ ��) ������Ʈ�� �ޱ� ���� �ż���
 

    public abstract int GetDamage(int _baseDamage, int comboStep); // ������ ��� �ż���

    public abstract int GetChargeDamage(); // ���� ������ �Ի� �ż���

  

    /// <summary>
    /// Ability
    /// </summary>
    /// <param name="rate"></param>
    // ������ ������ ���� �޼��� ����
    public virtual void SetGageRetentionRate(float rate)
    {
        gageRetentionRate = Mathf.Clamp01(rate);  // 0.0-1.0 ������ ����
        Debug.Log($"{WeaponName}�� ������ ������ ����: {gageRetentionRate * 100}%");
    }
    // ResetGage �޼��� ���� (������ ���� �� ���� ���� ����)
    public virtual void ResetGage(int currentGage_)
    {
        // ��ų ��� �� ������ ���� ȿ�� ����
        if (currentGage_ == 0 && gageRetentionRate > 0f)
        {
            // Ư�� ��ų ��� �� ������ ����
            int retainedGage = Mathf.RoundToInt(100 * gageRetentionRate);
            CurrentGage = retainedGage;
            Debug.Log($"������ ���� ȿ�� ����: {retainedGage}/100 ������ ������");
        }
        else
        {
            // �Ϲ����� ��� (������ ������ ������ ����)
            CurrentGage = currentGage_;
        }
    }

}
