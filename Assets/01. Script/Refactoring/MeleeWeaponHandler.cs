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
        damagedTargets.Clear(); // 새로운 공격 시 초기화
    }
    private void Update()
    {
        Debug.Log($"현재 타임스케일 {Time.timeScale}");
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
                damagedTargets.Add(other); // 같은 대상에 대해 다시 데미지를 주지 않도록 저장
                weapon.GetGage(weapon.GagePerHit);                
                Debug.Log($"현재무기{weapon.WeaponName} 현재 게이지 {weapon.GagePerHit} ");
                Debug.Log($"{monster.GetMonsterClass().MONSTERNAME} 남은 체력: {monster.GetMonsterClass().CurrentHealth}");
                Debug.Log($"{weapon.WeaponName}의 데미지 {GetDamage()}");
            } 
        }
    }

    private void OnDisable()
    {
        damagedTargets.Clear(); // 콜라이더가 비활성화될 때 초기화하여 다음 공격에 재사용 가능
    }
}