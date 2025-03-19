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

    // Clear �� ���� �ð� ���� OnTriggerEnter�� �����ϱ� ���� �÷���
    private bool isClearingDamaged = false;

    // ������ ��� �� �߻��ϴ� �̺�Ʈ ����
    // ���� ������ ��� �� �߻��ϴ� �̺�Ʈ ����
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
        // ClearDamagedMonsters() ȣ�� ���� Ư�� �ð� ������ OnTriggerEnter ����
        if (isClearingDamaged)
        {
            Debug.Log("Clear ���̹Ƿ� OnTriggerEnter ����");
            return;
        }

        Debug.Log("Ÿ�� : " + damagedMonsters.Count);

        if (weapon == null)
        {
            Debug.Log("Sanrl����");
            return;
        }

        MonsterHitBox hitBox = other.GetComponent<MonsterHitBox>();
        if (hitBox == null)
        {
            Debug.Log("HixBOS����");
            return;
        }

        ICreatureStatus monster = hitBox.GetMonsterStatus();
        Debug.Log($"Monster HashCode: {monster?.GetHashCode()}");
        Debug.Log($"Monster Equals (in HashSet): {damagedMonsters.Contains(monster)}");

        if (monster == null)
        {
            Debug.Log("Status����");
            return;
        }

        // �̹� ���� ���͸� ����
        if (damagedMonsters.Contains(monster))
        {
            Debug.Log(monster.ToString());
            Debug.Log("Hashset");
            return;
        }

        float damageMultiplier = hitBox.GetDamageMultiplier(transform.position);
        int finalDamage = Mathf.RoundToInt(GetDamage() * damageMultiplier);
        // ���� �������� ���� �� �̺�Ʈ �ߵ�
        OnFinalDamageCalculated?.Invoke(finalDamage, monster);
        // ������ �Ϲ� ���� ��� ó��
        if (monster is IDamageable damageable)
        {
            if (damageMultiplier > 1f)
            {
                Debug.Log("�����!");
            }

            damageable.TakeDamage(finalDamage);

            string monsterType = monster is BossStatus ? "����" : "�Ϲݸ�";
            Debug.Log($"{monsterType} : {monster.GetMonsterClass().CurrentHealth}");

            damagedMonsters.Add(monster);
            weapon.GetGage(weapon.GagePerHit);
            Debug.Log($"damagedMonsters�� �߰���. ���� ��: {damagedMonsters.Count}");
        }
    }

    public void ClearDamagedMonsters()
    {
        Debug.Log("Ŭ������ : " + damagedMonsters.Count);
        damagedMonsters.Clear();
        Debug.Log("damagedMonsters Ŭ�����" + damagedMonsters.Count);

        // Clear ����, ���� �ð� ���� �浹�� �����ϵ��� ����
        isClearingDamaged = true;
        StartCoroutine(ResetClearingFlag());
    }

    private IEnumerator ResetClearingFlag()
    {
        // ���ϴ� �ð�(��: 0.1��) ���� OnTriggerEnter�� ����
        yield return new WaitForSeconds(0.3f);
        isClearingDamaged = false;
        Debug.Log("ClearingDamaged time ended. OnTriggerEnter is active again.");
    }
}
