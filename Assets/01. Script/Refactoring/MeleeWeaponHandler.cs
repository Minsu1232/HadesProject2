using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AttackData;

public class MeleeDamageDealer : MonoBehaviour, IDamageDealer, IWeaponDamageDealer
{
    private WeaponManager weapon;
    private int comboStep;
    private HashSet<ICreatureStatus> damagedMonsters = new HashSet<ICreatureStatus>();

    // Clear 후 일정 시간 동안 OnTriggerEnter를 무시하기 위한 플래그
    private bool isClearingDamaged = false;

    // 데미지 계산 시 발생하는 이벤트 정의
    // 최종 데미지 계산 시 발생하는 이벤트 정의
    public event Action<int, ICreatureStatus> OnFinalDamageCalculated;
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
        target.TakeDamage(GetDamage());
    }

    private void OnTriggerEnter(Collider other)
    {
        // ClearDamagedMonsters() 호출 직후 특정 시간 동안은 OnTriggerEnter 무시
        if (isClearingDamaged)
        {
            Debug.Log("Clear 중이므로 OnTriggerEnter 무시");
            return;
        }

        Debug.Log("타격 : " + damagedMonsters.Count);

        if (weapon == null)
        {
            Debug.Log("Sanrl없음");
            return;
        }

        MonsterHitBox hitBox = other.GetComponent<MonsterHitBox>();
        if (hitBox == null)
        {
            Debug.Log("HixBOS없음");
            return;
        }

        ICreatureStatus monster = hitBox.GetMonsterStatus();
        Debug.Log($"Monster HashCode: {monster?.GetHashCode()}");
        Debug.Log($"Monster Equals (in HashSet): {damagedMonsters.Contains(monster)}");

        if (monster == null)
        {
            Debug.Log("Status없음");
            return;
        }

        // 이미 때린 몬스터면 리턴
        if (damagedMonsters.Contains(monster))
        {
            Debug.Log(monster.ToString());
            Debug.Log("Hashset");
            return;
        }

        float damageMultiplier = hitBox.GetDamageMultiplier(transform.position);
        int finalDamage = Mathf.RoundToInt(GetDamage() * damageMultiplier);
        // 최종 데미지가 계산된 후 이벤트 발동
        OnFinalDamageCalculated?.Invoke(finalDamage, monster);
        // 보스와 일반 몬스터 모두 처리
        if (monster is IDamageable damageable)
        {
            if (damageMultiplier > 1f)
            {
                Debug.Log("백어택!");
            }

            damageable.TakeDamage(finalDamage);

            string monsterType = monster is BossStatus ? "보스" : "일반몹";
            Debug.Log($"{monsterType} : {monster.GetMonsterClass().CurrentHealth}");

            damagedMonsters.Add(monster);
            weapon.GetGage(weapon.GagePerHit);
            Debug.Log($"damagedMonsters에 추가됨. 현재 수: {damagedMonsters.Count}");
        }
    }

    public void ClearDamagedMonsters()
    {
        Debug.Log("클리어전 : " + damagedMonsters.Count);
        damagedMonsters.Clear();
        Debug.Log("damagedMonsters 클리어됨" + damagedMonsters.Count);

        // Clear 직후, 일정 시간 동안 충돌을 무시하도록 설정
        isClearingDamaged = true;
        StartCoroutine(ResetClearingFlag());
    }

    private IEnumerator ResetClearingFlag()
    {
        // 원하는 시간(예: 0.1초) 동안 OnTriggerEnter를 무시
        yield return new WaitForSeconds(0.3f);
        isClearingDamaged = false;
        Debug.Log("ClearingDamaged time ended. OnTriggerEnter is active again.");
    }
}
