using UnityEngine;

public class RetreatMovementStrategy : BasicMovementStrategy
{
    //private Vector3 retreatDirection;
    //private bool isRetreating;

    //public void Move(Transform transform, Transform target, IMonsterClass monsterData)
    //{
    //    if (!isRetreating)
    //        CalculateRetreatDirection(transform, target);

    //    // Ÿ��(�÷��̾�)�� �ݴ� �������� ����
    //    transform.position += retreatDirection * monsterData.CurrentSpeed * 1.5f * Time.deltaTime;
    //}

    //public void StartMoving(Transform transform)
    //{
    //    isRetreating = true;
    //}

    //public void StopMoving()
    //{
    //    isRetreating = false;
    //}

    //private void CalculateRetreatDirection(Transform transform, Transform target)
    //{
    //    retreatDirection = (transform.position - target.position).normalized;
    //}

    //public bool ShouldChangeState(float distanceToTarget, IMonsterClass monsterData)
    //{
    //    // ����� �־����ų�, ������ ��ġ�� �������� ��
    //    return distanceToTarget >= monsterData.CurrentAggroDropRange;
    //}
}