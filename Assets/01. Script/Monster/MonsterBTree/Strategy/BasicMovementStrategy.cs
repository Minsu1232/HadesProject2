using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicMovementStrategy : IMovementStrategy
{
    private bool isChasing;
    private bool isRandomMoving;
    private float currentMoveTime;
    private Vector3 randomDirection;

    public void Move(Transform transform, Transform target, MonsterClass monsterData)
    {
        float distanceToTarget = Vector3.Distance(transform.position, target.position);
        if (distanceToTarget <= monsterData.CurrentChaseRange)
        {
            if (!isChasing)
            {
                StartMoving(transform);
            }
            ChaseMove(transform, target, monsterData);
        }
        else
        {
            if (isChasing)
            {
                StopMoving();
                StartRandomMove(transform);
            }
            RandomMove(transform, monsterData);
        }
    }

    public void StartMoving(Transform transform)
    {
        isChasing = true;
        isRandomMoving = false;
    }

    public void StopMoving()
    {
        isChasing = false;
    }

    private void ChaseMove(Transform transform, Transform target, MonsterClass monsterData)
    {
        Vector3 direction = (target.position - transform.position).normalized;
        transform.position += direction * monsterData.CurrentSpeed * Time.deltaTime;
    }

    private void RandomMove(Transform transform, MonsterClass monsterData)
    {
        if (currentMoveTime > 0)
        {
            transform.position += randomDirection * monsterData.CurrentSpeed * Time.deltaTime;
            currentMoveTime -= Time.deltaTime;
        }
        else
        {
            StartRandomMove(transform);
        }
    }

    private void StartRandomMove(Transform transform)
    {
        isRandomMoving = true;
        randomDirection = GetRandomDirection();
        currentMoveTime = Random.Range(1f, 3f);
    }

    private Vector3 GetRandomDirection()
    {
        Vector2 randomDirection2D = Random.insideUnitCircle.normalized;
        return new Vector3(randomDirection2D.x, 0, randomDirection2D.y);
    }

    public bool ShouldChangeState(float distanceToTarget, MonsterClass monsterData)
    {
        if (isChasing)
        {
            if (distanceToTarget <= monsterData.CurrentAttackRange)
                return true;  // Attack으로 전환
            if (distanceToTarget > monsterData.CurrentAggroDropRange)
                return true;  // Idle로 전환
        }
        return false;
    }
}
