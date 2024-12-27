using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterHitBox : MonoBehaviour
{
    private MonsterStatus monsterStatus;
    [SerializeField] private bool isBackHitBox = false;  // Inspector���� ����
    [SerializeField] private float backAttackMultiplier = 1.5f;  // ����� ������ ����

    private void Start()
    {
        monsterStatus = GetComponentInParent<MonsterStatus>();
        if (monsterStatus == null)
        {
            Debug.LogError($"HitBox {gameObject.name}���� MonsterStatus�� ã�� �� �����ϴ�.");
        }
    }

    public bool IsBackAttack(Vector3 attackerPosition)
    {
        if (!isBackHitBox) return false;

        // ������ �Ĺ� ���� ��� (��: 60��)
        float backAngle = 60f;
        Vector3 toAttacker = (attackerPosition - transform.position).normalized;
        float angle = Vector3.Angle(-transform.forward, toAttacker);

        return angle < backAngle * 0.5f;
    }

    public float GetDamageMultiplier(Vector3 attackerPosition)
    {
        return IsBackAttack(attackerPosition) ? backAttackMultiplier : 1f;
    }

    public MonsterStatus GetMonsterStatus()
    {
        return monsterStatus;
    }
}
