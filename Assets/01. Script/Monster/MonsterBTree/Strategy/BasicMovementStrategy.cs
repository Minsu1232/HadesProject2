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
        Debug.Log("�����̵�");
        
        // ȸ��
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 5f);

        // �̵� �� �浹 üũ
        float moveDistance = monsterData.CurrentSpeed * Time.deltaTime;
        float checkDistance = moveDistance + 0.5f; // �ణ �� �ָ� üũ

        // �ٹ��� ����ĳ��Ʈ�� �� �����ϰ� �浹 Ȯ��
        RaycastHit hit;
        bool frontBlocked = Physics.Raycast(transform.position, direction, out hit, checkDistance,
                                          LayerMask.GetMask("Wall", "Obstacle"));

        if (!frontBlocked)
        {
            // �浹�� ������ �̵�
            transform.position += direction * moveDistance;
            return;
        }

        // ���� ������ �� - ȸ�� �˰��� ����
        Debug.Log("�� �浹 ����! ȸ�� ����");

        // 1. �پ��� ���� üũ (8����)
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

        // 2. Ÿ�� ����� ���� �����鼭 ������ ���� ���� ã��
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

        // 3. ������ ���� ã������ �� �������� �̵�
        if (bestDirIndex != -1)
        {
            transform.position += directions[bestDirIndex] * moveDistance * 0.8f;
            // ã�� �������� �ε巴�� ȸ��
            Quaternion newRotation = Quaternion.LookRotation(directions[bestDirIndex]);
            transform.rotation = Quaternion.Lerp(transform.rotation, newRotation, Time.deltaTime * 3f);
        }
        else
        {
            // ��� ������ �����ٸ�, �ణ �ڷ� �̵�
            transform.position -= direction * moveDistance;
        }
    }

    private void RandomMove(Transform transform, IMonsterClass monsterData)
    {
        Debug.Log("�����̵�");
        if (currentMoveTime > 0)
        {
            // ȸ��
            Quaternion targetRotation = Quaternion.LookRotation(randomDirection);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 5f);

            // �̵� �� �浹 üũ - ����ĳ��Ʈ ��� ������ üũ ���
            float moveDistance = monsterData.CurrentSpeed * Time.deltaTime;
            Vector3 newPosition = transform.position + randomDirection * moveDistance;

            // ���� ��ġ���� �̵��� �������� ������ ���Ǿ� üũ
            // ���̾� ����ũ�� ���� ��ֹ��� üũ
            int layerMask = LayerMask.GetMask("Wall", "Obstacle");
            // ���� �ݶ��̴� ũ�⿡ �°� ������ ���� (��: 0.5f)
            float checkRadius = 0.5f;

            // �̵� �������� �ణ ���ʿ��� üũ
            Vector3 checkPosition = transform.position + randomDirection * (moveDistance + checkRadius);
            Collider[] hitColliders = Physics.OverlapSphere(checkPosition, checkRadius, layerMask, QueryTriggerInteraction.Ignore);

            if (hitColliders.Length == 0)
            {
                // �浹�� ������ �̵�
                transform.position = newPosition;
            }
            else
            {
                Debug.Log("�� �浹 ���� - ���� �̵� �� (OverlapSphere)");

                // �̵� ���� ����
                bool foundNewDirection = false;
                int maxAttempts = 8;

                // ���� �������� �õ�
                for (int i = 0; i < maxAttempts; i++)
                {
                    // 45���� ȸ���ϸ鼭 üũ
                    float angle = 45f * i;
                    Vector3 testDirection = Quaternion.Euler(0, angle, 0) * Vector3.forward;
                    testDirection.y = 0;
                    testDirection = testDirection.normalized;

                    // �� �������� üũ
                    Vector3 testPosition = transform.position + testDirection * (moveDistance + checkRadius);
                    Collider[] testColliders = Physics.OverlapSphere(testPosition, checkRadius, layerMask, QueryTriggerInteraction.Ignore);

                    if (testColliders.Length == 0)
                    {
                        // ������ ���� ã��
                        randomDirection = testDirection;
                        transform.position += randomDirection * moveDistance * 0.8f; // �ణ ������ �̵�
                        foundNewDirection = true;
                        break;
                    }
                }

                if (!foundNewDirection)
                {
                    // �� ������ ã�� �������� �ٸ� ���� ���� ����
                    StartRandomMove(transform);
                    currentMoveTime = Random.Range(0.5f, 1.5f); // ª�� �ð����� �缳��
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
        Debug.Log("�� ���� ���� ����");
        isRandomMoving = true;

        // ������ ���� ���� ã��
        Vector3 safeDirection = FindSafeDirection(transform);
        randomDirection = safeDirection;
        currentMoveTime = Random.Range(1.5f, 3f);
    }

    // ���� �浹���� �ʴ� ������ ���� ã��
    private Vector3 FindSafeDirection(Transform transform)
    {
        int layerMask = LayerMask.GetMask("Wall", "Obstacle");
        float checkDistance = 1.5f; // üũ �Ÿ�
        float checkRadius = 0.5f;   // üũ �ݰ�

        // �ִ� 8���� üũ
        for (int i = 0; i < 8; i++)
        {
            // 45���� ȸ���ϸ� üũ
            float angle = Random.Range(0, 360f);
            Vector3 testDirection = Quaternion.Euler(0, angle, 0) * Vector3.forward;
            testDirection.y = 0;
            testDirection = testDirection.normalized;

            // �ش� �������� �浹 üũ
            Vector3 testPosition = transform.position + testDirection * checkDistance;
            Collider[] hitColliders = Physics.OverlapSphere(testPosition, checkRadius, layerMask, QueryTriggerInteraction.Ignore);

            if (hitColliders.Length == 0)
            {
                // ������ ���� ã��
                return testDirection;
            }
        }

        // ��� ������ �������� ���� GetRandomDirection ���
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
                return true;  // Attack���� ��ȯ
            if (distanceToTarget > monsterData.CurrentAggroDropRange)
                return true;  // Idle�� ��ȯ
        }
        return false;
    }
}
