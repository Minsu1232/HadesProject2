using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using static AttackData;

public class MeleeDamageDealer : MonoBehaviour, IDamageDealer
{
    private WeaponManager weapon;
    private int comboStep;
    private HashSet<MonsterStatus> damagedMonsters = new HashSet<MonsterStatus>();

    public void Initialize(WeaponManager weapon, int comboStep)
    {
        
        this.weapon = weapon;
        this.comboStep = comboStep;
        
        
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
          

            if (monster != null && !damagedMonsters.Contains(monster))
            {
                
                float damageMultiplier = hitBox.GetDamageMultiplier(transform.position);
                int finalDamage = Mathf.RoundToInt(GetDamage() * damageMultiplier);

                if (damageMultiplier > 1f)
                {
                    Debug.Log("백어택!");
                }

                DealDamage(monster);

                damagedMonsters.Add(monster);  // 몬스터를 저장
                weapon.GetGage(weapon.GagePerHit);
                Debug.Log($"damagedMonsters에 추가됨. 현재 수: {damagedMonsters.Count}");
               
            }
         
        }
    }

 
    public void ClearDamagedMonsters()
    {
        damagedMonsters.Clear();
        Debug.Log("damagedMonsters 클리어됨");
    }
}