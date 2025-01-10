using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterHitBox : MonoBehaviour
{
    private MonsterStatus monsterStatus;
    private Collider hitBoxCollider;
    [SerializeField] private bool isBackHitBox = false;  // Inspector���� ����
    [SerializeField] private float backAttackMultiplier = 1.5f;  // ����� ������ ����

    private void Start()
    {
        monsterStatus = GetComponentInParent<MonsterStatus>();
        hitBoxCollider = GetComponent<Collider>();

        if (monsterStatus == null)
        {
            Debug.LogError($"HitBox {gameObject.name}���� MonsterStatus�� ã�� �� �����ϴ�.");
        }
        if (hitBoxCollider == null)
        {
            Debug.LogError($"HitBox {gameObject.name}���� Collider�� ã�� �� �����ϴ�.");
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

    // �ֻ��� ������Ʈ���� ��� HitBox�� �����ϱ� ���� ���� �޼����
    public static void EnableAllHitBoxes(GameObject rootObject)
    {
        foreach (var hitBox in rootObject.GetComponentsInChildren<MonsterHitBox>())
        {
            if (hitBox.hitBoxCollider != null)
                hitBox.hitBoxCollider.enabled = true;
        }
    }

    public static void DisableAllHitBoxes(GameObject rootObject)
    {
        foreach (var hitBox in rootObject.GetComponentsInChildren<MonsterHitBox>())
        {
            if (hitBox.hitBoxCollider != null)
                hitBox.hitBoxCollider.enabled = false;
        }
    }
}
