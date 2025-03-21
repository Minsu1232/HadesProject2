using UnityEngine;

public class RetreatMovementStrategy : BasicMovementStrategy
{
    //private Vector3 retreatDirection;
    //private bool isRetreating;

    //public void Move(Transform transform, Transform target, IMonsterClass monsterData)
    //{
    //    if (!isRetreating)
    //        CalculateRetreatDirection(transform, target);

    //    // 타겟(플레이어)의 반대 방향으로 도망
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
    //    // 충분히 멀어졌거나, 안전한 위치에 도달했을 때
    //    return distanceToTarget >= monsterData.CurrentAggroDropRange;
    //}
}