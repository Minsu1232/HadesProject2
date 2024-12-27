using System.Collections.Generic;
using UnityEngine;
using static AttackData;

public class MeleeDamageDealer : MonoBehaviour, IDamageDealer
{
    private WeaponManager weapon;
    private int comboStep;
    private HashSet<MonsterStatus> damagedMonsters = new HashSet<MonsterStatus>();  // 콜라이더가 아닌 몬스터 기준으로 변경

    public void Initialize(WeaponManager weapon, int comboStep)
    {
        this.weapon = weapon;
        this.comboStep = comboStep;
        damagedMonsters.Clear(); // 새로운 공격 시 초기화
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
            if (monster != null && !damagedMonsters.Contains(monster))  // 몬스터 기준으로 체크
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

                Debug.Log($"현재무기{weapon.WeaponName} 현재 게이지 {weapon.GagePerHit} ");
                Debug.Log($"{monster.GetMonsterClass().MONSTERNAME} 남은 체력: {monster.GetMonsterClass().CurrentHealth}");
                Debug.Log($"{weapon.WeaponName}의 데미지 {GetDamage()}");
            }
        }
    }

    private void OnDisable()
    {
        damagedMonsters.Clear();
    }
}