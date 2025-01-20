using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaseMovementStrategy : IMovementStrategy
{
    private bool isChasing;

    public void Move(Transform transform, Transform target, IMonsterClass monsterData)
    {
        Vector3 direction = (target.position - transform.position).normalized;
        transform.position += direction * monsterData.CurrentSpeed * Time.deltaTime;
    }

    public void StartMoving(Transform transform)
    {
        isChasing = true;
    }

    public void StopMoving()
    {
        isChasing = false;
    }

    public bool ShouldChangeState(float distanceToTarget, IMonsterClass monsterData)
    {
        if (distanceToTarget <= monsterData.CurrentAttackRange)
            return true;  // Attack으로 전환

        if (distanceToTarget > monsterData.CurrentAggroDropRange)
            return true;  // Idle로 전환

        return false;
    }
}
