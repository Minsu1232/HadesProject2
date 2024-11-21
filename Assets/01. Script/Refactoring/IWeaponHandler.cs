using UnityEngine;

public interface IWeaponHandler
{
    void OnAttack(IWeapon weapon, Transform origin, int comboStep);
    void OnAttackEnd(IWeapon weapon);
}