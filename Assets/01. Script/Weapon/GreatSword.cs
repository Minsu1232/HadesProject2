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

        // ��¡ ������Ʈ �ʱ�ȭ
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
        Debug.Log("����� Ư�� ��ų �ߵ�!");
        if (specialAttackComponent != null)
        {
            
            specialAttackComponent.Execute();
           
        }
        else
        {
            Debug.LogError("���ε��");
        }
        
    }
  //�ִϸ��̼� �̺�Ʈ
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
