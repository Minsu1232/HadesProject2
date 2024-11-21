using System.Collections.Generic;
using UnityEngine;
using static AttackData;

public class MeleeDamageDealer : MonoBehaviour, IDamageDealer
{
    private WeaponManager weapon;
    private int comboStep;
    private HashSet<Collider> damagedTargets = new HashSet<Collider>();

    public void Initialize(WeaponManager weapon, int comboStep)
    {  
        
        this.weapon = weapon;
        this.comboStep = comboStep;
        damagedTargets.Clear(); // ���ο� ���� �� �ʱ�ȭ
    }
    private void Update()
    {
        Debug.Log($"���� Ÿ�ӽ����� {Time.timeScale}");
    }
    public int GetDamage()
    {

        return weapon.IsChargeAttack
         ? weapon.GetChargeDamage()
         : weapon.GetDamage(weapon.BaseDamage, comboStep);


    }
    public AttackType GetAttackType()
    {
        return weapon.IsChargeAttack ? AttackType.Charge : AttackType.Normal;
    }
    public void DealDamage(IDamageable target)
    {
        
        target.TakeDamage(GetDamage(),GetAttackType());
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (weapon == null)
        {
            return;
        }
        if (other.CompareTag("Monster") && !damagedTargets.Contains(other) && weapon != null)
        {
            MonsterStatus monster = other.GetComponent<MonsterStatus>();
            if (monster != null)
            {
                DealDamage(monster);
                damagedTargets.Add(other); // ���� ��� ���� �ٽ� �������� ���� �ʵ��� ����
                weapon.GetGage(weapon.GagePerHit);                
                Debug.Log($"���繫��{weapon.WeaponName} ���� ������ {weapon.GagePerHit} ");
                Debug.Log($"{monster.GetMonsterClass().MONSTERNAME} ���� ü��: {monster.GetMonsterClass().CurrentHealth}");
                Debug.Log($"{weapon.WeaponName}�� ������ {GetDamage()}");
            } 
        }
    }

    private void OnDisable()
    {
        damagedTargets.Clear(); // �ݶ��̴��� ��Ȱ��ȭ�� �� �ʱ�ȭ�Ͽ� ���� ���ݿ� ���� ����
    }
}