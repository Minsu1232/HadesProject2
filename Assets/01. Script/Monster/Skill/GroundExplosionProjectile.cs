using System.Collections;
using UnityEngine;

public class GroundExplosionProjectile : BaseProjectile
{
    public float safeZoneRadius; // 안전 구역 반경
    public float dangerRadius; // 위험 구역 반경(safe의 1.5~2배)
    public float explosionDelay; // 폭발 지연 시간
    public bool isRingShaped = true; // true: 안전구역 바깥만 데미지, false: 전체 영역 데미지

    private float timer = 0f;
    private bool hasExploded = false;
    private float essenceAmount;
    private bool isGrounded = false;
    private bool damageApplied = false; // 한 번 데미지를 적용한 후 빠져나가도록
    private GameObject indicatorInstance; // 생성된 인디케이터 참조
    public GameObject indicatorPrefab; // 인디케이터 프리팹
    private ParticleSystem projectileParticle; // 발사체 파티클 시스템

    private ICreatureStatus monsterStatus;
    // BaseProjectile의 Initialize 메서드를 재정의
    public override void Initialize(Vector3 startPos, Transform target, float speed, float damage,
        IProjectileMovement moveStrategy, IProjectileImpact impactEffect, GameObject hitEffect, float heightFactor)
    {
        base.Initialize(startPos, target, speed, damage, moveStrategy, impactEffect, hitEffect, heightFactor);

        // 파티클 시스템 가져오기
        projectileParticle = GetComponentInChildren<ParticleSystem>();

        // 발사체 유형에 따라 색상 설정
        UpdateProjectileColor();
    }

    // 추가 초기화 메서드
    public void SetExplosionParameters(float safeZoneRadius, float dangerRadiusMultiplier, float explosionDelay, bool isRingShaped, GameObject indicatorPrefab, ICreatureStatus status, float essenceAmount = 0)
    {
        this.safeZoneRadius = safeZoneRadius;
        this.dangerRadius = safeZoneRadius * dangerRadiusMultiplier;
        this.explosionDelay = explosionDelay;
        this.isRingShaped = isRingShaped;
        this.indicatorPrefab = indicatorPrefab;
        this.monsterStatus = status;
        this.essenceAmount = essenceAmount;
        // 발사체 유형에 따라 색상 설정
        UpdateProjectileColor();
    }

    // 발사체 색상 업데이트
    private void UpdateProjectileColor()
    {
        if (projectileParticle != null)
        {
            var main = projectileParticle.main;

            // isRingShaped가 false일 때 파란색, true일 때 빨간색
            if (!isRingShaped)
            {
                main.startColor = Color.blue; // 파란 발톱
                Debug.Log("파란 발톱 색상 설정");
            }
            else
            {
                main.startColor = Color.red; // 빨간 발톱
                Debug.Log("빨간 발톱 색상 설정");
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ground"))
        {
            OnImpact(other);
        }
    }

    // Impact 이벤트 처리
    protected override void OnImpact(Collider other)
    {
        // 이미 땅에 닿았으면 무시
        if (isGrounded) return;

        // 지면에 닿으면 처리
        if (other.CompareTag("Ground") || other.CompareTag("Terrain"))
        {
            Debug.Log($"발톱 착지 - 유형: {(isRingShaped ? "링형(빨강)" : "전체(파랑)")}");
            isGrounded = true;
            moveStrategy = null; // 움직임 중지

            // 땅에 약간 박히도록 위치 조정
            transform.position = new Vector3(transform.position.x, 0.1f, transform.position.z);

            // 인디케이터 생성 (폭발 영역 표시)
            CreateExplosionIndicator();
        }
    }

    protected override void Update()
    {
        // 기존 움직임 업데이트
        base.Update();

        // 땅에 닿았을 때 폭발 타이머 시작
        if (isGrounded && !hasExploded)
        {
            timer += Time.deltaTime;

            // 타이머가 폭발 시간에 도달하면 폭발
            if (timer >= explosionDelay)
            {
                Explode();
            }
        }
    }

    // 인디케이터 생성 (프리팹 사용)
    private void CreateExplosionIndicator()
    {
        if (indicatorPrefab == null)
        {
            Debug.LogError("인디케이터 프리팹이 설정되지 않았습니다!");
            return;
        }

        // 인디케이터 프리팹 인스턴스화
        indicatorInstance = Instantiate(
            indicatorPrefab,
            new Vector3(transform.position.x, 0.9f, transform.position.z),
            Quaternion.Euler(90f, 0f, 0f)
        );

        // 인디케이터 크기 설정
        if (isRingShaped)
        {
            // 링형 발톱 - 위험 영역 크기로 설정
            indicatorInstance.transform.localScale = new Vector3(dangerRadius * 2f, dangerRadius * 2f, 1f);

            // 도넛 모양 인디케이터 속성 설정 (프리팹에 맞게 조정 필요)
            Renderer renderer = indicatorInstance.GetComponentInChildren<Renderer>();
            if (renderer != null && renderer.material != null)
            {
                // 도넛 모양 설정 (내부 반경 비율)
                float innerRadiusRatio = safeZoneRadius / dangerRadius;

                // _InnerRadius 속성이 있다면 설정
                if (renderer.material.HasProperty("_InnerRadius"))
                {
                    renderer.material.SetFloat("_InnerRadius", innerRadiusRatio);
                }

                // _FillAmount 속성이 있다면 설정
                if (renderer.material.HasProperty("_FillAmount"))
                {
                    renderer.material.SetFloat("_FillAmount", 0f); // 시작은 0
                    StartCoroutine(AnimateFill(renderer.material, explosionDelay));
                }
            }
        }
        else
        {
            // 전체 영역 발톱 - 안전 영역 크기로 설정
            indicatorInstance.transform.localScale = new Vector3(safeZoneRadius * 2f, safeZoneRadius * 2f, 1f);

            // 원형 인디케이터 속성 설정
            Renderer renderer = indicatorInstance.GetComponentInChildren<Renderer>();
            if (renderer != null && renderer.material != null)
            {
                // _InnerRadius 속성이 있다면 0으로 설정 (내부 원 없음)
                if (renderer.material.HasProperty("_InnerRadius"))
                {
                    renderer.material.SetFloat("_InnerRadius", 0f);
                }

                // _FillAmount 속성이 있다면 설정
                if (renderer.material.HasProperty("_FillAmount"))
                {
                    renderer.material.SetFloat("_FillAmount", 0f); // 시작은 0
                    StartCoroutine(AnimateFill(renderer.material, explosionDelay));
                }
            }
        }
    }

    // Fill 애니메이션
    private IEnumerator AnimateFill(Material material, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float fillAmount = elapsed / duration;
            material.SetFloat("_FillAmount", fillAmount);

            elapsed += Time.deltaTime;
            yield return null;
        }

        material.SetFloat("_FillAmount", 1f); // 완전히 채움
    }

    private void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;

        Debug.Log($"폭발 시작 - 유형: {(isRingShaped ? "링 형태(빨간색)" : "전체 영역(파란색)")}");
        Debug.Log($"안전 구역 반경: {safeZoneRadius}, 위험 구역 반경: {dangerRadius}");

        // 발톱 유형에 따라 다른 폭발 패턴
        if (isRingShaped)
        {
            // 빨간색 발톱: 안쪽은 안전, 바깥은 위험
            ApplyRingExplosionDamage();
        }
        else
        {
            // 파란색 발톱: 범위 내 데미지
            ApplyFullExplosionDamage();
        }

        // 폭발 이펙트 생성
        if (hitEffect != null)
        {
            Instantiate(hitEffect, transform.position, Quaternion.identity);
        }

        // 인디케이터 제거
        if (indicatorInstance != null)
        {
            Destroy(indicatorInstance);
        }

        // 발톱 오브젝트 제거
        Destroy(gameObject, 0.2f);
    }

    // 링 모양 폭발 데미지 (안전 구역이 있는)
    private void ApplyRingExplosionDamage()
    {
        Debug.Log($"링 폭발 데미지 계산 - 안전 범위: {safeZoneRadius}, 위험 범위: {dangerRadius}");

        // 위험 구역에 있는 모든 콜라이더 검색
        Collider[] colliders = Physics.OverlapSphere(transform.position, dangerRadius);

        foreach (var collider in colliders)
        {
            if (collider.CompareTag("Player") && !damageApplied)
            {
                // 플레이어 위치 가져오기 (중심점)
                PlayerClass player = GameInitializer.Instance.GetPlayerClass();
                if (player != null)
                {
                    Vector3 playerPosition = player.playerTransform.position;
                    // 거리 계산
                    float distance = Vector3.Distance(transform.position, playerPosition);

                    Debug.Log($"플레이어 거리: {distance}, 안전 구역: {safeZoneRadius}, 위험 구역: {dangerRadius}");

                    // 도넛 영역에만 데미지 적용 (안전 구역 밖 && 위험 구역 안)
                    if (distance > safeZoneRadius && distance <= dangerRadius)
                    {
                        if (monsterStatus.GetMonsterClass() is AlexanderBoss alexBoss)
                        {
                            
                            IBossEssenceSystem essenceSystem = alexBoss.GetEssenceSystem();
                            if (essenceSystem != null)
                            {
                                
                                essenceSystem.IncreaseEssence(essenceAmount);
                            }
                            player.TakeDamage((int)damage);
                            damageApplied = true; // 데미지 적용 표시
                            Debug.Log($"링형 데미지 적용: {damage}");
                        }
                        else
                        {
                            Debug.Log("플레이어가 도넛 영역 밖에 있어 데미지 없음");
                        }
                    }
                }
            }
        } 
    }

    // 전체 영역 폭발 데미지 (안전 구역 없음)
    private void ApplyFullExplosionDamage()
    {
        Debug.Log($"전체 영역 폭발 데미지 - 범위: {safeZoneRadius}");

        // 전체 범위에 데미지
        Collider[] colliders = Physics.OverlapSphere(transform.position, safeZoneRadius);

        foreach (var collider in colliders)
        {
            if (collider.CompareTag("Player") && !damageApplied)
            {
                PlayerClass player = GameInitializer.Instance.GetPlayerClass();
                if (player != null)
                {
                    if (monsterStatus.GetMonsterClass() is AlexanderBoss alexBoss)
                    {

                        IBossEssenceSystem essenceSystem = alexBoss.GetEssenceSystem();
                        if (essenceSystem != null)
                        {

                            essenceSystem.IncreaseEssence(essenceAmount);
                        }
                    }
                        player.TakeDamage((int)damage);
                    damageApplied = true;
                    Debug.Log($"전체 영역 데미지 적용: {damage}");
                }
            }
        }
    }
 
    // 디버그용 기즈모 그리기
    private void OnDrawGizmos()
    {
        if (!isGrounded) return;

        if (isRingShaped)
        {
            // 안전 영역 - 녹색
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, safeZoneRadius);

            // 위험 영역 - 빨간색
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, dangerRadius);
        }
        else
        {
            // 파란색 발톱 영역 - 파란색
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, safeZoneRadius);
        }
    }
}