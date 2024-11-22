using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAttackBase : MonoBehaviour, ICharacterAttack
{
    protected IWeapon currentWeapon;
    protected CharacterAnimationController animationController; // CharacterAnimationController ���
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
            Debug.LogError("CharacterAnimationController�� ������� �ʾҽ��ϴ�.");
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
        animationController.SetTrigger("Attack"); // �ִϸ��̼� ȣ��
        currentWeapon?.OnAttack(transform, comboStep);
    }

    public virtual void ChargingAttack()
    {
        animationController.SetTrigger("ChargingAttack"); // ��¡ �ִϸ��̼� ȣ��
        Debug.Log("��¡ ����");
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
            animationController.SetTrigger("ReleaseCharge"); // ��¡ ���� �ִϸ��̼� ȣ��
            currentWeapon?.ReleaseCharge();
        }
       
    }
    public virtual void SpecialAttack()
    {
        if (currentWeapon is WeaponManager weaponManager /*&& weaponManager.CanUseSpecialAttack)*/)
        { 
            
            // ������ �����ϸ� �ִϸ��̼� ����
            animationController.SetTrigger("SpecialAttack");
            

            // ���� ��ų ����
            currentWeapon.SpecialAttack();
        }
        else
        {
            Debug.Log("����");
        }
    }
  
    public void ComboStepUpdate(int step)
    {
        comboStep = step;
        currentWeapon?.OnAttack(transform, comboStep);
    }

}
    
  
   

    

