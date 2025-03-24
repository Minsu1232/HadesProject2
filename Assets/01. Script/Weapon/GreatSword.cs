using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class GreatSword : WeaponBase
// Start is called before the first frame update
{    
    private MeleeDamageDealer damageDealer;
    bool isSpecialAttacking = false;

    protected override void InitializeComponents()
    {
        if (_weaponInstance != null)
        {
            damageDealer = _weaponInstance.GetComponent<MeleeDamageDealer>();
            if (damageDealer == null)
            {
                damageDealer = _weaponInstance.AddComponent<MeleeDamageDealer>();
            }
            damageDealer.Initialize(this, 0);
            
        }

        // 차징 컴포넌트 초기화
            chargeComponent = new GreatSwordCharge(this);
            specialAttackComponent = new GreatSwordSpecialAttack(this);
        
    }

    public override void OnAttack(Transform origin, int comboStep)
    {
        if (damageDealer != null)
        {
            damageDealer.Initialize(this, comboStep);
        }
        else
        {
            Debug.LogError("DamageDealer is not initialized!");
        }
    }

    public override void SpecialAttack()
    {
        Debug.Log("대검의 특수 스킬 발동!");
        if (specialAttackComponent != null)
        {
            
            specialAttackComponent.Execute();
           
        }
        else
        {
            Debug.LogError("널인디요");
        }
        
    }
  //애니메이션 이벤트
    public void PlayVFX()
    {
        specialAttackComponent.PlayVFX();
        specialAttackComponent.PlaySound();
    }
    public MeleeDamageDealer ReturnDealer()
    {
        return damageDealer;
    }

}

// Update is called once per frame
