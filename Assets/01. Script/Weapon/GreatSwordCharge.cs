using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GreatSwordCharge : WeaponChargeBase
{   
    public GreatSwordCharge(WeaponManager manager) : base(manager) { }
    protected override void PerformChargeAttack(float chargeRatio)
    {
        int baseDamage = weaponManager.BaseDamage;
        int totalDamage = Mathf.RoundToInt(baseDamage * (1 + chargeRatio * weaponManager.weaponData.chargeMultiplier));
        Damage = totalDamage;        
        Debug.Log($"Greatsword ��¡ ����! ������: {totalDamage}");
        // �߰����� ����(���� ����, ����Ʈ ��) ���� ����
    }

    
   
}
