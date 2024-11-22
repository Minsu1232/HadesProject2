using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAttackBase : MonoBehaviour, ICharacterAttack
{
    protected IWeapon currentWeapon;
    protected CharacterAnimationController animationController; // CharacterAnimationController 사용
    public int comboStep;
    
    protected bool isAttacking;
    private bool isCharging = false;
    private int hashAttackCount = Animator.StringToHash("AttackCount");
    public int AttackCount
    {
        get => animationController.GetInteger(hashAttackCount);
        set => animationController.SetInteger(hashAttackCount, value);
    }

    protected virtual void Awake()
    {
        animationController = GetComponent<CharacterAnimationController>();
        if (animationController == null)
        {
            Debug.LogError("CharacterAnimationController가 연결되지 않았습니다.");
        }
    }

    protected virtual void Start()
    {
        currentWeapon = GameInitializer.Instance.GetCurrentWeapon();
        comboStep = 0;
    }

    public void EquipWeapon(IWeapon weapon)
    {
        currentWeapon = weapon;
        Animator animator = animationController != null ? animationController.GetAnimator() : null;
        currentWeapon?.InitializeWeapon(animator);
    }

    public virtual void BasicAttack()
    {
        animationController.SetTrigger("Attack"); // 애니메이션 호출
        currentWeapon?.OnAttack(transform, comboStep);
    }

    public virtual void ChargingAttack()
    {
        animationController.SetTrigger("ChargingAttack"); // 차징 애니메이션 호출
        Debug.Log("차징 시작");
        isCharging = true;
        currentWeapon?.StartCharge();
        currentWeapon?.OnAttack(transform, comboStep);
    }
    public void UpdateCharge(float deltaTime)
    {
        if (isCharging)
        {
            currentWeapon?.UpdateCharge(deltaTime);
        }
    }
    public virtual void ReleaseCharge()
    {
        if (isCharging)
        {
            isCharging = false;
            animationController.SetTrigger("ReleaseCharge"); // 차징 해제 애니메이션 호출
            currentWeapon?.ReleaseCharge();
        }
       
    }
    public virtual void SpecialAttack()
    {
        if (currentWeapon is WeaponManager weaponManager /*&& weaponManager.CanUseSpecialAttack)*/)
        { 
            
            // 조건을 만족하면 애니메이션 실행
            animationController.SetTrigger("SpecialAttack");
            

            // 무기 스킬 실행
            currentWeapon.SpecialAttack();
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
    
  
   

    

