using UnityEngine;

public abstract class WeaponChargeBase
{   
    protected int Damage { get; set; }
    protected float currentChargeTime;
    protected bool isCharging;
    protected WeaponManager weaponManager;

    public WeaponChargeBase(WeaponManager manager)
    {
        weaponManager = manager;
    }

    public int GetChargeDamage()
    {        
        return Damage;
    }
    public void StartCharge()
    {
        isCharging = true;
        currentChargeTime = 0f;
        Debug.Log($"{weaponManager.WeaponName}: Â÷Â¡ ½ÃÀÛ!");
    }

    public void UpdateCharge(float deltaTime)
    {
        if (!isCharging) return;

        currentChargeTime += deltaTime;
        if (currentChargeTime >= weaponManager.weaponData.maxChargeTime)
        {
            currentChargeTime = weaponManager.weaponData.maxChargeTime;
            Debug.Log($"{weaponManager.WeaponName}: ÃÖ´ë Â÷Â¡ ¿Ï·á!");
        }
    }

    public void ReleaseCharge()
    {
        if (!isCharging) return;
        
        isCharging = false;
        float chargeRatio = currentChargeTime / weaponManager.weaponData.maxChargeTime;
        PerformChargeAttack(chargeRatio);
        Debug.Log($"{weaponManager.WeaponName}: Â÷Â¡ ÇØÁ¦! Â÷Â¡ ºñÀ²: {chargeRatio}");
    }

    protected abstract void PerformChargeAttack(float chargeRatio);

}