using System;
using UnityEngine;

[Serializable]
public class WeaponData
{
    [Header("Basic Info")]
    public string weaponName;
    public int baseDamage;
    public int baseGagePerHit;

    [Header("Transform Info")]
    public Vector3 defaultPosition;
    public Vector3 defaultRotation;

    [Header("Charge Info")]
    public float maxChargeTime;
    public float chargeMultiplier;

    [Header("Upgrade Info")]
    public int damageUpgradeCount;
    public int gageUpgradeCount;
    public int additionalDamage;
    public int additionalGagePerHit;
}