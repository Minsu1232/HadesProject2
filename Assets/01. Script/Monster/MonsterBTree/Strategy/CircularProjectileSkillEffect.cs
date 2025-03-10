using UnityEngine;
using System;

public class CircularProjectileSkillEffect : ProjectileSkillEffect
{
    // 원형 배치 관련 파라미터
    private int projectileCount;        // 실제 사용될 변수
    private int baseProjectileCount;    // 원본값 보관용 

    private float radius;        // 원의 반지름
    private float safeZoneRadius;  // 안전 구역 반경
    private float dangerRadiusMultiplier; // 위험 영역 배수
    private float explosionDelay;  // 폭발 지연 시간
    private float randomVariation; // 각도 랜덤 변화량(도)
    private float essenceAmount;
    // 인디케이터 관련
    private GameObject indicatorPrefab; // 인디케이터 프리팹

    // 생성자는 부모 클래스의 생성자 호출
    public CircularProjectileSkillEffect(GameObject prefab, GameObject indicatorPrefab, float speed,
        IProjectileMovement moveStrategy, IProjectileImpact impactEffect,
        GameObject hitEffect, float heightFactor)
        : base(prefab, speed, moveStrategy, impactEffect, hitEffect, heightFactor)
    {
        this.indicatorPrefab = indicatorPrefab;
    }

    // 추가 설정 메서드
    public void SetCircleParameters(int count, float radius, float safeZoneRadius,
                                   float dangerRadiusMultiplier, float explosionDelay, float essenceAmount = 0)
    {
        baseProjectileCount = count;
        this.projectileCount = count;
        this.radius = radius;
        this.safeZoneRadius = safeZoneRadius;
        this.dangerRadiusMultiplier = dangerRadiusMultiplier;
        this.explosionDelay = explosionDelay;
        this.essenceAmount = essenceAmount;
    }

    // Initialize 메서드 오버라이드
    public override void Initialize(ICreatureStatus status, Transform target)
    {
        base.Initialize(status, target);

        // 항상 원본값에서 시작 (이 부분이 핵심 해결 코드)
        projectileCount = baseProjectileCount;

        if (status.GetMonsterClass() is IBossWithEssenceSystem alexBoss)
        {
            IBossEssenceSystem essenceSystem = alexBoss.GetEssenceSystem();
            if (essenceSystem != null)
            {
                float essenceRatio = essenceSystem.CurrentEssence / essenceSystem.MaxEssence;

                int additionalProjectiles = Mathf.RoundToInt(3 * essenceRatio);
                projectileCount += additionalProjectiles;

                Debug.Log($"Essence 수치: {essenceRatio:P0}, 총 발사체 수: {projectileCount}개");

                if (essenceRatio > 0.9f)
                {
                    explosionDelay *= 0.7f;
                    Debug.Log($"광기 최대치 접근! 폭발 대기 시간: {explosionDelay}초");
                }
            }
            else
            {
                // 에센스 시스템 없을 때 원본값 유지
                projectileCount = baseProjectileCount;
            }
        }
    }


    // Execute 메서드 오버라이드
    public override void Execute()
    {
        try
        {
            if (projectilePrefab == null || monsterStatus == null) return;

            Transform source = monsterStatus.GetMonsterTransform();
            Vector3 centerPos = source.position;

            // 원형으로 프로젝타일 생성
            for (int i = 0; i < projectileCount; i++)
            {
                // 균등하게 분포된 각도 + 랜덤 변화
                float angle = (360f / projectileCount) * i + UnityEngine.Random.Range(-randomVariation, randomVariation);
                // 약간의 랜덤 반경
                float rad = UnityEngine.Random.Range(radius * 0.8f, radius * 1.2f);

                // 목표 위치 계산 (바닥)
                Vector3 targetPos = centerPos + new Vector3(
                    Mathf.Cos(angle * Mathf.Deg2Rad) * rad,
                    0.1f,
                    Mathf.Sin(angle * Mathf.Deg2Rad) * rad
                );

                // 발사 시작 위치 (보스 위치에서 약간 위)
                Vector3 spawnPos = centerPos + Vector3.up * 2f;

                // Alexander 보스의 에센스 시스템 연동
                bool isRingShaped = true; // 기본값: 링형(빨간색)
                if (monsterStatus.GetMonsterClass() is IBossWithEssenceSystem alexBoss)
                {
                    IBossEssenceSystem essenceSystem = alexBoss.GetEssenceSystem();
                    if (essenceSystem != null)
                    {
                        float essenceRatio = essenceSystem.CurrentEssence / essenceSystem.MaxEssence;
                        // 광기 수치에 따라 파란색 발톱 확률 증가 (10% ~ 30%)
                        float blueClawChance = 0.1f + (0.2f * essenceRatio);
                        isRingShaped = (UnityEngine.Random.value > blueClawChance);
                    }
                    else
                    {
                        isRingShaped = (UnityEngine.Random.value <= 0.1f); // 기본: 90% 빨간색
                    }
                }
                else
                {
                    isRingShaped = (UnityEngine.Random.value <= 0.1f); // 기본: 90% 빨간색
                }

       

                // 임시 타겟 생성 (프로젝타일이 향할 방향)
                GameObject tempTarget = new GameObject($"GroundTarget_{i}");
                tempTarget.transform.position = targetPos;
                Debug.Log(isRingShaped + "현재 발톱 상태");
                // 각 발사체마다 새로운 DelayedExplosionImpact 인스턴스 생성
                DelayedExplosionImpact customImpact = new DelayedExplosionImpact(
                    safeZoneRadius,
                    dangerRadiusMultiplier,
                    explosionDelay,
                    isRingShaped,
                    hitEffect
                );

                // 발사체 생성
                GameObject projectile = GameObject.Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
                InvokeEffectCompleted();
                if (projectile.TryGetComponent<GroundExplosionProjectile>(out var explosionProj))
                {
                    // 기본 초기화
                    explosionProj.Initialize(spawnPos, tempTarget.transform, projectileSpeed, skillDamage,
                                            moveStrategy, customImpact, hitEffect, heightFactor);

                    // 추가 파라미터 설정 (인디케이터 프리팹 포함)
                    explosionProj.SetExplosionParameters(safeZoneRadius, dangerRadiusMultiplier,
                                                       explosionDelay, isRingShaped, indicatorPrefab, monsterStatus, essenceAmount);

                    // 땅을 향해 떨어지게 설정
                    explosionProj.targetPosition = new Vector3(spawnPos.x, 0f, spawnPos.z);

                    // 발사
                    explosionProj.Launch();
                }

                // 임시 타겟 제거 (일정 시간 후)
                GameObject.Destroy(tempTarget, explosionDelay + 2f);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"CircularProjectileSkillEffect.Execute 오류: {e.Message}\n{e.StackTrace}");
        }
    }

    // 폭발 영역을 미리 보여주는 인디케이터 생성
   
}