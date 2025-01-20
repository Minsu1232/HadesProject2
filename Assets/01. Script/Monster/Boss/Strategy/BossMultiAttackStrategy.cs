using System;
using System.Collections.Generic;
using UnityEngine;

public class BossMultiAttackStrategy : BasePhysicalAttackStrategy
{
    // 공격 전략과 가중치를 함께 저장하는 클래스
    [Serializable]
    public class WeightedAttackStrategy
    {
        public IAttackStrategy Strategy;
        public float Weight;

        public WeightedAttackStrategy(IAttackStrategy strategy, float weight)
        {
            Strategy = strategy;
            Weight = weight;
        }
    }

    // 공격 전략 리스트
    private List<WeightedAttackStrategy> attackStrategies;

    // 현재 선택된 공격 전략
    private IAttackStrategy currentAttackStrategy;

    // 패턴 변경 쿨타임 관련 변수
    private float patternChangeCooldown = 5f;
    private float lastPatternChangeTime;

    // 랜덤 생성기
    private System.Random random = new System.Random();

    public BossMultiAttackStrategy(List<WeightedAttackStrategy> strategies)
    {
        attackStrategies = strategies;
        ValidateWeights();
    }

    // 가중치 합이 1이 되도록 조정
    private void ValidateWeights()
    {
        float totalWeight = 0;
        foreach (var strategy in attackStrategies)
        {
            totalWeight += strategy.Weight;
        }

        if (Mathf.Approximately(totalWeight, 0))
        {
            // 기본값으로 균등 분배
            float defaultWeight = 1f / attackStrategies.Count;
            foreach (var strategy in attackStrategies)
            {
                strategy.Weight = defaultWeight;
            }
        }
        else if (!Mathf.Approximately(totalWeight, 1f))
        {
            // 비율에 맞게 정규화
            for (int i = 0; i < attackStrategies.Count; i++)
            {
                attackStrategies[i].Weight /= totalWeight;
            }
        }
    }

    // 가중치 기반 랜덤 공격 전략 선택
    private IAttackStrategy SelectRandomAttackStrategy()
    {
        float randomValue = (float)random.NextDouble();
        float cumulativeWeight = 0f;

        foreach (var strategyEntry in attackStrategies)
        {
            cumulativeWeight += strategyEntry.Weight;
            if (randomValue <= cumulativeWeight)
            {
                return strategyEntry.Strategy;
            }
        }

        // 만약 무언가 잘못되었다면 마지막 전략 반환
        return attackStrategies[attackStrategies.Count - 1].Strategy;
    }

    public override void Attack(Transform transform, Transform target, IMonsterClass monsterData)
    {
        // 패턴 변경 시간 체크
        if (Time.time >= lastPatternChangeTime + patternChangeCooldown)
        {
            currentAttackStrategy = SelectRandomAttackStrategy();
            lastPatternChangeTime = Time.time;
        }

        // 현재 선택된 공격 전략으로 공격 실행
        currentAttackStrategy?.Attack(transform, target, monsterData);
    }

    public override bool CanAttack(float distanceToTarget, IMonsterClass monsterData)
    {
        // 현재 선택된 전략의 공격 가능 여부 확인
        return currentAttackStrategy?.CanAttack(distanceToTarget, monsterData) ?? false;
    }

    // 다른 메서드들도 현재 선택된 전략에 위임
    public override void StartAttack()
    {
        currentAttackStrategy?.StartAttack();
    }

    public override void StopAttack()
    {
        currentAttackStrategy?.StopAttack();
    }

    public override void OnAttackAnimationEnd()
    {
        currentAttackStrategy?.OnAttackAnimationEnd();
    }

    public override PhysicalAttackType AttackType =>
        currentAttackStrategy?.AttackType ?? PhysicalAttackType.Basic;
}