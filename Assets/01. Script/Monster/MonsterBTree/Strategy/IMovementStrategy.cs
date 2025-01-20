// �̵� ����
using UnityEngine;

public interface IMovementStrategy
{
    void Move(Transform transform, Transform target, IMonsterClass monsterData);
    void StartMoving(Transform transform);  // �̵� ���۽� ȣ��
    void StopMoving();  // �̵� �ߴܽ� ȣ��
    bool ShouldChangeState(float distanceToTarget, IMonsterClass monsterData);
}