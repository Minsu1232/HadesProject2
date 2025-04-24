using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GreatSwordCharge : WeaponChargeBase
{   
    public GreatSwordCharge(WeaponManager manager) : base(manager) { }
    protected override void PerformChargeAttack(float chargeRatio)
    {
        int baseDamage = weaponManager.BaseDamage;
        int playerDamage = GameInitializer.Instance.GetPlayerClass().GetStats().AttackPower;


        int totalDamage = Mathf.RoundToInt((baseDamage * ((1 + chargeRatio) * weaponManager.weaponData.chargeMultiplier)) + playerDamage);
        Damage = totalDamage;        
        Debug.Log($"Greatsword 차징 공격! 데미지: {totalDamage},{baseDamage},{Damage},{weaponManager.weaponData.chargeMultiplier}");
        // 추가적인 로직(범위 공격, 이펙트 등) 구현 가능
    }

    
   
}
