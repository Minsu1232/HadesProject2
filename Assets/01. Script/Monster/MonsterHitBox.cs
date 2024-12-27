using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterHitBox : MonoBehaviour
{
    private MonsterStatus monsterStatus;
    [SerializeField] private bool isBackHitBox = false;  // Inspector에서 설정
    [SerializeField] private float backAttackMultiplier = 1.5f;  // 백어택 데미지 배율

    private void Start()
    {
        monsterStatus = GetComponentInParent<MonsterStatus>();
        if (monsterStatus == null)
        {
            Debug.LogError($"HitBox {gameObject.name}에서 MonsterStatus를 찾을 수 없습니다.");
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
}
