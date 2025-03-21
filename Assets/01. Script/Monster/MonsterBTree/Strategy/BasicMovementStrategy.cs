using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicMovementStrategy : IMovementStrategy
{
    private bool isChasing;
    private bool isRandomMoving;
    private float currentMoveTime;
    private Vector3 randomDirection;

    public void Move(Transform transform, Transform target, IMonsterClass monsterData)
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

    private void ChaseMove(Transform transform, Transform target, IMonsterClass monsterData)
    {
        Vector3 direction = (target.position - transform.position).normalized;
        Debug.Log("추적이동");
        
        // 회전
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 5f);

        // 이동 전 충돌 체크
        float moveDistance = monsterData.CurrentSpeed * Time.deltaTime;
        float checkDistance = moveDistance + 0.5f; // 약간 더 멀리 체크

        // 다방향 레이캐스트로 더 정밀하게 충돌 확인
        RaycastHit hit;
        bool frontBlocked = Physics.Raycast(transform.position, direction, out hit, checkDistance,
                                          LayerMask.GetMask("Wall", "Obstacle"));

        if (!frontBlocked)
        {
            // 충돌이 없으면 이동
            transform.position += direction * moveDistance;
            return;
        }

        // 벽에 막혔을 때 - 회피 알고리즘 시작
        Debug.Log("벽 충돌 감지! 회피 시작");

        // 1. 다양한 방향 체크 (8방향)
        Vector3[] directions = new Vector3[8];
        bool[] isBlocked = new bool[8];
        float angleStep = 45f;

        for (int i = 0; i < 8; i++)
        {
            float angle = i * angleStep;
            directions[i] = Quaternion.Euler(0, angle, 0) * Vector3.forward;
            isBlocked[i] = Physics.Raycast(transform.position, directions[i], checkDistance,
                                          LayerMask.GetMask("Wall", "Obstacle"));
        }

        // 2. 타겟 방향과 가장 가까우면서 막히지 않은 방향 찾기
        float minAngleDiff = 360f;
        int bestDirIndex = -1;

        for (int i = 0; i < 8; i++)
        {
            if (!isBlocked[i])
            {
                float angleDiff = Vector3.Angle(direction, directions[i]);
                if (angleDiff < minAngleDiff)
                {
                    minAngleDiff = angleDiff;
                    bestDirIndex = i;
                }
            }
        }

        // 3. 적합한 방향 찾았으면 그 방향으로 이동
        if (bestDirIndex != -1)
        {
            transform.position += directions[bestDirIndex] * moveDistance * 0.8f;
            // 찾은 방향으로 부드럽게 회전
            Quaternion newRotation = Quaternion.LookRotation(directions[bestDirIndex]);
            transform.rotation = Quaternion.Lerp(transform.rotation, newRotation, Time.deltaTime * 3f);
        }
        else
        {
            // 모든 방향이 막혔다면, 약간 뒤로 이동
            transform.position -= direction * moveDistance;
        }
    }

    private void RandomMove(Transform transform, IMonsterClass monsterData)
    {
        Debug.Log("랜덤이동");
        if (currentMoveTime > 0)
        {
            // 회전
            Quaternion targetRotation = Quaternion.LookRotation(randomDirection);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 5f);

            // 이동 전 충돌 체크 - 레이캐스트 대신 오버랩 체크 사용
            float moveDistance = monsterData.CurrentSpeed * Time.deltaTime;
            Vector3 newPosition = transform.position + randomDirection * moveDistance;

            // 현재 위치에서 이동할 방향으로 오버랩 스피어 체크
            // 레이어 마스크는 벽과 장애물만 체크
            int layerMask = LayerMask.GetMask("Wall", "Obstacle");
            // 몬스터 콜라이더 크기에 맞게 반지름 조절 (예: 0.5f)
            float checkRadius = 0.5f;

            // 이동 방향으로 약간 앞쪽에서 체크
            Vector3 checkPosition = transform.position + randomDirection * (moveDistance + checkRadius);
            Collider[] hitColliders = Physics.OverlapSphere(checkPosition, checkRadius, layerMask, QueryTriggerInteraction.Ignore);

            if (hitColliders.Length == 0)
            {
                // 충돌이 없으면 이동
                transform.position = newPosition;
            }
            else
            {
                Debug.Log("벽 충돌 감지 - 랜덤 이동 중 (OverlapSphere)");

                // 이동 방향 수정
                bool foundNewDirection = false;
                int maxAttempts = 8;

                // 여러 방향으로 시도
                for (int i = 0; i < maxAttempts; i++)
                {
                    // 45도씩 회전하면서 체크
                    float angle = 45f * i;
                    Vector3 testDirection = Quaternion.Euler(0, angle, 0) * Vector3.forward;
                    testDirection.y = 0;
                    testDirection = testDirection.normalized;

                    // 새 방향으로 체크
                    Vector3 testPosition = transform.position + testDirection * (moveDistance + checkRadius);
                    Collider[] testColliders = Physics.OverlapSphere(testPosition, checkRadius, layerMask, QueryTriggerInteraction.Ignore);

                    if (testColliders.Length == 0)
                    {
                        // 안전한 방향 찾음
                        randomDirection = testDirection;
                        transform.position += randomDirection * moveDistance * 0.8f; // 약간 느리게 이동
                        foundNewDirection = true;
                        break;
                    }
                }

                if (!foundNewDirection)
                {
                    // 새 방향을 찾지 못했으면 다른 랜덤 방향 설정
                    StartRandomMove(transform);
                    currentMoveTime = Random.Range(0.5f, 1.5f); // 짧은 시간으로 재설정
                }
            }

            currentMoveTime -= Time.deltaTime;
        }
        else
        {
            StartRandomMove(transform);
        }
    }

    private void StartRandomMove(Transform transform)
    {
        Debug.Log("새 랜덤 방향 설정");
        isRandomMoving = true;

        // 안전한 랜덤 방향 찾기
        Vector3 safeDirection = FindSafeDirection(transform);
        randomDirection = safeDirection;
        currentMoveTime = Random.Range(1.5f, 3f);
    }

    // 벽과 충돌하지 않는 안전한 방향 찾기
    private Vector3 FindSafeDirection(Transform transform)
    {
        int layerMask = LayerMask.GetMask("Wall", "Obstacle");
        float checkDistance = 1.5f; // 체크 거리
        float checkRadius = 0.5f;   // 체크 반경

        // 최대 8방향 체크
        for (int i = 0; i < 8; i++)
        {
            // 45도씩 회전하며 체크
            float angle = Random.Range(0, 360f);
            Vector3 testDirection = Quaternion.Euler(0, angle, 0) * Vector3.forward;
            testDirection.y = 0;
            testDirection = testDirection.normalized;

            // 해당 방향으로 충돌 체크
            Vector3 testPosition = transform.position + testDirection * checkDistance;
            Collider[] hitColliders = Physics.OverlapSphere(testPosition, checkRadius, layerMask, QueryTriggerInteraction.Ignore);

            if (hitColliders.Length == 0)
            {
                // 안전한 방향 찾음
                return testDirection;
            }
        }

        // 모든 방향이 막혔으면 원래 GetRandomDirection 사용
        return GetRandomDirection();
    }
    private Vector3 GetRandomDirection()
    {
        Vector2 randomDirection2D = Random.insideUnitCircle.normalized;
        return new Vector3(randomDirection2D.x, 0, randomDirection2D.y);
    }

    public bool ShouldChangeState(float distanceToTarget, IMonsterClass monsterData)
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
