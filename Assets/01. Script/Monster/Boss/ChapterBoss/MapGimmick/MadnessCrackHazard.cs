using UnityEngine;

public class MadnessCrackHazard : IBossEssenceHazard
{
    // 속성
    public string HazardName => "광기의 균열";
    public float ActivationThreshold { get; private set; }
    public float DamageMultiplier { get; private set; }

    // 프리팹 및 설정
    private GameObject crackPrefab;
    private GameObject indicatorPrefab;
    private GameObject explosionPrefab;
    private float warningDuration;
    private float explosionRadius;
    private float explosionDamage;
    private IBossEssenceSystem essenceSystem;
    private ICreatureStatus monster;
    private float essenceAmount = 10f;  // 폭발 시 증가할 에센스량

    // 이펙트 및 사운드
    private Color crackColor = Color.red;

    public MadnessCrackHazard(
        GameObject prefab,
        GameObject indicatorPrefab,
        GameObject explosionPrefab,
        ICreatureStatus monster,
        float threshold = 70f,
        float warningTime = 1.5f,
        float radius = 3f,
        float damage = 20f,
        float dmgMultiplier = 1.2f)
    {
        crackPrefab = prefab;
        this.indicatorPrefab = indicatorPrefab;
        this.explosionPrefab = explosionPrefab;
        this.monster = monster;
        ActivationThreshold = threshold;
        warningDuration = warningTime;
        explosionRadius = radius;
        explosionDamage = damage;
        DamageMultiplier = dmgMultiplier;

        Debug.Log(monster.GetMonsterClass().ToString());
    }

    public void Initialize(IBossEssenceSystem essenceSystem)
    {
        this.essenceSystem = essenceSystem;
        Debug.Log($"광기의 균열 위험요소 초기화 완료 (임계값: {ActivationThreshold})");
    }

    public void ActivateHazard(Vector3 position, float intensity)
    {
        // 프리팹 사용 여부에 따른 균열 생성
        if (crackPrefab == null)
        {
            // 프리팹이 없을 경우에도 기존 DelayedExplosionImpact 사용
            DelayedExplosionImpact impactEffect = new DelayedExplosionImpact(
                explosionRadius * 0.3f,  // 안전 구역 반경
                intensity,                // 위험 구역 배수
                warningDuration,          // 폭발 지연 시간
                false,                    // 링 형태 여부
                null                      // 폭발 이펙트
            );

            impactEffect.OnImpact(position, explosionDamage * intensity * DamageMultiplier);
        }
        else
        {
            // 프리팹 기반 효과
            CreatePrefabCrackEffect(position, intensity);
        }

        Debug.Log($"광기의 균열 생성됨 - 위치: {position}, 강도: {intensity:F2}");
    }

    private void CreatePrefabCrackEffect(Vector3 position, float intensity)
    {
        // 임시 타겟 생성
        GameObject tempTarget = new GameObject("CrackTarget");
        tempTarget.transform.position = position;

        // DelayedExplosionImpact 설정
        DelayedExplosionImpact impactEffect = new DelayedExplosionImpact(
            explosionRadius * 0.3f,  // 안전 구역 반경
            intensity,               // 위험 구역 배수
            warningDuration,         // 폭발 지연 시간
            false,                   // 링 형태 여부
            explosionPrefab          // 폭발 이펙트
        );

        // GroundExplosionProjectile 생성 시도
        if (crackPrefab != null)
        {
            GameObject projectile = GameObject.Instantiate(crackPrefab, position + Vector3.up * 0.5f, Quaternion.identity);

            if (projectile.TryGetComponent<GroundExplosionProjectile>(out var explosionProj))
            {
                // 프로젝타일 초기화
                explosionProj.Initialize(
                    position + Vector3.up * 2f, // 시작 위치
                    tempTarget.transform,       // 목표 위치
                    5f,                         // 속도
                    explosionDamage * intensity * DamageMultiplier, // 데미지
                    new StraightMovement(),                       // 이동 전략
                    impactEffect,               // 임팩트 효과
                    explosionPrefab,                       // 히트 이펙트
                    0.5f                        // 높이 계수
                );

                // 추가 파라미터 설정
                explosionProj.SetExplosionParameters(
                    explosionRadius * 1f,  // 안전 구역
                    intensity,               // 위험 배수
                    warningDuration,         // 지연 시간
                    false,                   // 링 형태
                    indicatorPrefab,         // 인디케이터
                    monster,                    // 몬스터 상태
                    essenceAmount            // 에센스 증가량
                );

                // 발사
                explosionProj.Launch();
            }
            else
            {
                // 프로젝타일 컴포넌트가 없으면 직접 임팩트 효과 적용
                impactEffect.OnImpact(position, explosionDamage * intensity * DamageMultiplier);
                GameObject.Destroy(projectile);
            }
        }
        else
        {
            // 프리팹이 없으면 직접 임팩트 효과 적용
            impactEffect.OnImpact(position, explosionDamage * intensity * DamageMultiplier);
        }

        // 임시 타겟 정리
        GameObject.Destroy(tempTarget, 5f);
    }

    public void DeactivateHazard()
    {
        // 처리 중인 모든 임팩트 효과 찾기
        var explosionObjects = GameObject.FindGameObjectsWithTag("Explosion");
        foreach (var obj in explosionObjects)
        {
            GameObject.Destroy(obj);
        }

        Debug.Log("모든 광기의 균열 효과 제거됨");
    }

    public void UpdateHazardIntensity(float essenceValue)
    {
        // 에센스 수치에 따라 균열 특성 조정
        float intensityFactor = Mathf.Clamp01((essenceValue - ActivationThreshold) /
                               (100f - ActivationThreshold));

        // 데미지 및 반경 강화
        DamageMultiplier = Mathf.Lerp(1.0f, 1.5f, intensityFactor);
        explosionRadius = Mathf.Lerp(3f, 5f, intensityFactor);

        // 에센스 수치가 매우 높을 경우 경고 시간 감소
        if (essenceValue > 90f)
        {
            warningDuration = Mathf.Lerp(1.5f, 0.8f, (essenceValue - 90f) / 10f);
        }
    }
}