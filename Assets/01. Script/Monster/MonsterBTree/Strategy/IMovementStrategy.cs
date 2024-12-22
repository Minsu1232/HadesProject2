// �̵� ����
using UnityEngine;

public interface IMovementStrategy
{
    void Move(Transform transform, Transform target, MonsterClass monsterData);
    void StartMoving(Transform transform);  // �̵� ���۽� ȣ��
    void StopMoving();  // �̵� �ߴܽ� ȣ��
    bool ShouldChangeState(float distanceToTarget, MonsterClass monsterData);
}