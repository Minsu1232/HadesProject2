using System.Collections.Generic;
using UnityEngine;
using static AttackData;

public class MeleeDamageDealer : MonoBehaviour, IDamageDealer
{
    private WeaponManager weapon;
    private int comboStep;
    private HashSet<MonsterStatus> damagedMonsters = new HashSet<MonsterStatus>();  // �ݶ��̴��� �ƴ� ���� �������� ����

    public void Initialize(WeaponManager weapon, int comboStep)
    {
        this.weapon = weapon;
        this.comboStep = comboStep;
        damagedMonsters.Clear(); // ���ο� ���� �� �ʱ�ȭ
    }
    public int GetDamage()
    {

        return weapon.IsChargeAttack
         ? weapon.GetChargeDamage()
         : weapon.GetDamage(weapon.BaseDamage, comboStep);


    }

    public void DealDamage(IDamageable target)
    {
        
        target.TakeDamage(GetDamage(),weapon.GetAttackType());
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (weapon == null) return;

        MonsterHitBox hitBox = other.GetComponent<MonsterHitBox>();
        if (hitBox != null)
        {
            MonsterStatus monster = hitBox.GetMonsterStatus();
            if (monster != null && !damagedMonsters.Contains(monster))  // ���� �������� üũ
            {
                float damageMultiplier = hitBox.GetDamageMultiplier(transform.position);
                int finalDamage = Mathf.RoundToInt(GetDamage() * damageMultiplier);

                if (damageMultiplier > 1f)
                {
                    Debug.Log("�����!");
                }

                DealDamage(monster);
                damagedMonsters.Add(monster);  // ���͸� ����
                weapon.GetGage(weapon.GagePerHit);

                Debug.Log($"���繫��{weapon.WeaponName} ���� ������ {weapon.GagePerHit} ");
                Debug.Log($"{monster.GetMonsterClass().MONSTERNAME} ���� ü��: {monster.GetMonsterClass().CurrentHealth}");
                Debug.Log($"{weapon.WeaponName}�� ������ {GetDamage()}");
            }
        }
    }

    private void OnDisable()
    {
        damagedMonsters.Clear();
    }
}