using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomMovementStrategy : IMovementStrategy
{
    private bool isMoving;
    private float currentMoveTime;
    private Vector3 randomDirection;

    public void Move(Transform transform, Transform target, IMonsterClass monsterData)
    {
        if (currentMoveTime > 0)
        {
            transform.position += randomDirection * monsterData.CurrentSpeed * Time.deltaTime;
            currentMoveTime -= Time.deltaTime;
        }
        else
        {
            StartMoving(transform);
        }
    }

    public void StartMoving(Transform transform)
    {
        isMoving = true;
        randomDirection = GetRandomDirection();
        currentMoveTime = Random.Range(1f, 3f);
    }

    public void StopMoving()
    {
        isMoving = false;
    }

    private Vector3 GetRandomDirection()
    {
        Vector2 randomDirection2D = Random.insideUnitCircle.normalized;
        return new Vector3(randomDirection2D.x, 0, randomDirection2D.y);
    }

    public bool ShouldChangeState(float distanceToTarget, IMonsterClass monsterData)
    {
        return distanceToTarget <= monsterData.CurrentChaseRange;  // Chase·Î ÀüÈ¯
    }
}
