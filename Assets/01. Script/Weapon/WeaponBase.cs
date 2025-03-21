

using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public abstract class WeaponBase : WeaponManager, IWeaponCollider
{

    protected float comboDamageMultiplier = 1.0f; // �޺� ������ ���� ���� (�⺻��: 1.0 = 100%)
    protected float gageChargeMultiplier = 1.0f;
    protected abstract override void InitializeComponents();
    // IWeapon ����
    public abstract override void OnAttack(Transform origin, int comboStep);


    public override int GetDamage(int _baseDamage, int comboStep)
    {
        int playerDamage = GameInitializer.Instance.GetPlayerClass().PlayerStats.AttackPower;

        // �޺� ������ 1���� Ŭ ���� ������ ���� (ù ���ݿ��� �������� ����)
        if (comboStep > 1)
        {
            return Mathf.RoundToInt(comboStep * (BaseDamage + playerDamage) * comboDamageMultiplier);
        }
        else
        {
            return comboStep * (BaseDamage + playerDamage);
        }
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
            MeleeDamageDealer damageDealer = GetComponentInChildren<MeleeDamageDealer>();
            if (damageDealer != null)
            {
                damageDealer.ClearDamagedMonsters();
            }
        }
    }
    public void OnChargeAttackEnd()
    {       
       
            EndChargeAttack();          
        

    }
    // WeaponBase Ŭ������ �߰��ؾ� �� �޼���
    public void SetGaugeChargeMultiplier(float multiplier)
    {
        // GagePerHit ���� �����ϴ� ��� ������ ȹ�淮�� �����ϴ� ���� ���� �߰� �ʿ�
        gageChargeMultiplier = Mathf.Max(1.0f, multiplier); // �ּ� 1.0 (100%)
        Debug.Log($"���� {GetType().Name}�� ������ ���� ���� ����: {gageChargeMultiplier * 100}%");
    }

    // GetGage �޼��� �������̵� (WeaponManager�� GetGage �޼��带 �������̵�)
    public override void GetGage(int amount)
    {
        // ������ �����Ͽ� ������ ����
        int boostedAmount = Mathf.RoundToInt(amount * gageChargeMultiplier);
        base.GetGage(boostedAmount);
    }
    public void SetComboDamageMultiplier(float multiplier)
    {
        comboDamageMultiplier = Mathf.Max(1.0f, multiplier); // �ּ� 1.0 (100%)
        Debug.Log($"���� {GetType().Name}�� �޺� ������ ���� ����: {comboDamageMultiplier * 100}%");
    }

    // 4. �޺� ������ ���� ��ȯ �޼��� �߰�
    public float GetComboDamageMultiplier()
    {
        return comboDamageMultiplier;
    }

}
  