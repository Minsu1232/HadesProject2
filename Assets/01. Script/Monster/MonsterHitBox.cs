using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterHitBox : MonoBehaviour
{
    private MonsterStatus monsterStatus;
    private Collider hitBoxCollider;
    [SerializeField] private bool isBackHitBox = false;  // Inspector에서 설정
    [SerializeField] private float backAttackMultiplier = 1.5f;  // 백어택 데미지 배율

    private void Start()
    {
        monsterStatus = GetComponentInParent<MonsterStatus>();
        hitBoxCollider = GetComponent<Collider>();

        if (monsterStatus == null)
        {
            Debug.LogError($"HitBox {gameObject.name}에서 MonsterStatus를 찾을 수 없습니다.");
        }
        if (hitBoxCollider == null)
        {
            Debug.LogError($"HitBox {gameObject.name}에서 Collider를 찾을 수 없습니다.");
        }
    }

    public bool IsBackAttack(Vector3 attackerPosition)
    {
        if (!isBackHitBox) return false;
        // 몬스터의 후방 각도 계산 (예: 60도)
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

    // 최상위 오브젝트에서 모든 HitBox를 제어하기 위한 정적 메서드들
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
