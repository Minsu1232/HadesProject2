using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CharacterAttackBase : MonoBehaviour, ICharacterAttack
{
    public IWeapon currentWeapon;
    private Animator animator;
    public int comboStep;

    public bool isCharging = false;
    private float currentChargeTime = 0f;
    public event Action<float> OnChargeTimeUpdated;

    private static readonly int HashAttackCount = Animator.StringToHash("AttackCount");
    private static readonly int HashAttack = Animator.StringToHash("Attack");
    private static readonly int HashChargingAttack = Animator.StringToHash("ChargingAttack");
    private static readonly int HashReleaseCharge = Animator.StringToHash("ReleaseCharge");
    private static readonly int HashSpecialAttack = Animator.StringToHash("SpecialAttack");

    public int AttackCount
    {
        get => animator ? animator.GetInteger(HashAttackCount) : 0;
        set
        {
            if (animator) animator.SetInteger(HashAttackCount, value);
        }
    }

    protected virtual void Awake()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError($"Animator missing on {gameObject.name}");
        }
    }

    public void EquipWeapon(IWeapon weapon)
    {
        currentWeapon = weapon;
        currentWeapon?.InitializeWeapon(animator);
    }
    public void UnequipWeapon()
    {
        // 무기 참조 제거
        currentWeapon = null;
        comboStep = 0;
        isCharging = false;
        currentChargeTime = 0f;

        // 차지 상태 업데이트 이벤트 호출
        OnChargeTimeUpdated?.Invoke(0f);

        Debug.Log("CharacterAttackBase: 무기 참조가 제거되었습니다.");
    }
    public virtual void BasicAttack()
    {
        animator?.SetTrigger(HashAttack);
        currentWeapon?.OnAttack(transform, comboStep);
    }

    public virtual void ChargingAttack()
    {
        animator?.SetTrigger(HashChargingAttack);
        Debug.Log("차징 시작");
        isCharging = true;
        currentWeapon?.StartCharge();
        currentWeapon?.OnAttack(transform, comboStep);
    }

    public void UpdateCharge(float deltaTime)
    {
        if (isCharging)
        {
            currentChargeTime += deltaTime;
            OnChargeTimeUpdated?.Invoke(currentChargeTime);
            currentWeapon?.UpdateCharge(deltaTime);
        }
    }

    public virtual void ReleaseCharge()
    {
        if (isCharging)
        {
            isCharging = false;
            currentChargeTime = 0f;
            OnChargeTimeUpdated?.Invoke(0f);
            animator?.SetTrigger(HashReleaseCharge);
            currentWeapon?.ReleaseCharge();
        }
    }

    public virtual void SpecialAttack()
    {
        if (currentWeapon is WeaponManager weaponManager && weaponManager.CurrentGage == 100)
        {

            animator?.SetTrigger(HashSpecialAttack);
            currentWeapon.SpecialAttack();
            LightningJudgmentComponent lightningComp = GetComponent<LightningJudgmentComponent>();
            if (lightningComp != null)
            {
                lightningComp.ExecuteLightningJudgment();
                Debug.Log("번개 심판 발동!");
            }
        }
        else
        {
            Debug.Log("허허");
        }
    }

    public void ComboStepUpdate(int step)
    {
        comboStep = step;
        currentWeapon?.OnAttack(transform, comboStep);
    }
}






