

using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public abstract class WeaponBase : WeaponManager, IWeaponCollider
{
    

    protected abstract override void InitializeComponents();


    // IWeapon ����
    public abstract override void OnAttack(Transform origin, int comboStep);


    public override int GetDamage(int _baseDamage, int comboStep)
    {
        int playerDamage = GameInitializer.Instance.GetPlayerClass().CurrentAttackPower;
        return comboStep * (BaseDamage + playerDamage);
    }
    public override int GetChargeDamage()
    {
        int playerDamage = chargeComponent.GetChargeDamage();
        
        return playerDamage;
    }
    // ��¡ ����

    public abstract override void SpecialAttack();

    // IWeaponCollider ����
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
        }
    }
    public void OnChargeAttackEnd()
    {       
       
            EndChargeAttack();          
        

    }



}
  