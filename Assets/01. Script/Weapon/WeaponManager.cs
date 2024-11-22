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

    // �ʱ�ȭ�� �Ӽ���
    protected GameObject _weaponInstance;
    protected Collider weaponCollider;
    //protected Animator weaponAnimator;
    protected int currentGage; // ���� ������

    // �Ӽ��� ������
    public string WeaponName => weaponData?.weaponName ?? "Unknown";
    public int BaseDamage => weaponData != null ? weaponData.baseDamage + weaponData.additionalDamage : 0;
    public int GagePerHit => weaponData != null ? weaponData.baseGagePerHit + weaponData.additionalGagePerHit : 0;
    public float MaxChargeTime => weaponData?.maxChargeTime ?? 0;
    public float ChargeMultiplier => weaponData?.chargeMultiplier ?? 0;   

    public bool IsChargeAttack { get; private set; } = false;
    public bool IsSpecialAttack { get; set; } = false;

    public bool CanUseSpecialAttack => currentGage >= 100;
    // �ʱ�ȭ
    public virtual void InitializeWeapon(Animator animator)
    {
        LoadAnimatorOverride(WeaponName, animator);
       
    }
    public AttackType GetAttackType()
    {
        return IsChargeAttack ? AttackType.Charge : AttackType.Normal;
    }
    // ���� �� �ε�
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
                Debug.Log($"���� �� �ε� �Ϸ�: {WeaponName}");
            }
        };
        return _weaponInstance;
    }
    public virtual void LoadAnimatorOverride(string weaponName, Animator animator)
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
    public abstract void OnAttack(Transform origin, int comboStep);

    // ������ �ý���
    public virtual void GetGage(int amount)
    {
        currentGage += amount;
        if (currentGage > 100) currentGage = 100;
        Debug.Log($"{WeaponName} ������ ����: {currentGage}/100");
    }

    public abstract void SpecialAttack();

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
    protected abstract void InitializeComponents();
 

    public abstract int GetDamage(int _baseDamage, int comboStep);

    public abstract int GetChargeDamage();

    public void ResetGage()
    {
        currentGage = 0;
    }

}
