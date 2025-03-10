using System.Collections.Generic;
using UnityEngine;

public enum SoulType
{
    Bright,
    Dark
}

public class SoulEntity : HazardObject
{
    [SerializeField] private SoulType soulType;    
    [SerializeField] private float rotationSpeed = 30f;

    private Transform bossTransform;
    private bool isDestroyed = false;
    public SoulType SoulType => soulType;

    [Header("Soul Components")]
    [SerializeField] private GameObject brightSoulObject; // 밝은 영혼 전체 오브젝트
    [SerializeField] private GameObject darkSoulObject;   // 어두운 영혼 전체 오브젝트

    [Header("Impact Effects")]
    [SerializeField] private GameObject brightImpactEffect; // 밝은 영혼 충돌 이펙트
    [SerializeField] private GameObject darkImpactEffect;   // 어두운 영혼 충돌 이펙트
    public override void Initialize(float radius, float dmg, float speed, HazardSpawnType type,
        TargetType targetType, float height, float areaRadius, IGimmickStrategy strategy)
    {
        base.Initialize(radius, dmg, speed, type, targetType, height, areaRadius, strategy);

        // 보스 트랜스폼 찾기
        var boss = GameObject.FindObjectOfType<BossAI>();
        if (boss != null)
        {
            bossTransform = boss.transform;
        }

        // 랜덤하게 영혼 타입 지정 또는 외부에서 지정된 타입 사용
        if (Random.value < 0.5f)
        {
            soulType = SoulType.Bright;
        }
        else
        {
            soulType = SoulType.Dark;
        }

        // 타입에 따른 비주얼 설정
        UpdateVisuals();
    }
    // SoulEntity 초기화 시 타입 설정
    public void SetSoulType(SoulType type)
    {
        soulType = type;
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        // 타입에 맞는 오브젝트만 활성화
        if (brightSoulObject != null) brightSoulObject.SetActive(soulType == SoulType.Bright);
        if (darkSoulObject != null) darkSoulObject.SetActive(soulType == SoulType.Dark);
    }

    public override void StartMove()
    {
        isWarning = false;
        if (warningIndicator != null)
        {
            warningIndicator.SetActive(false);
        }
    }

    protected override void Update()
    {
        base.Update();

        if (!isWarning && !isDestroyed && bossTransform != null)
        {
            // 보스를 향해 이동
            Vector3 direction = (bossTransform.position - transform.position).normalized;
            transform.position += direction * moveSpeed * Time.deltaTime;

            // 회전 효과 추가
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

            // 보스와의 거리 확인
            float distanceToBoss = Vector3.Distance(transform.position, bossTransform.position);
            if (distanceToBoss < 1.0f)
            {
                HandleBossCollision();
            }
        }
    }

    private void HandleBossCollision()
    {
        if (soulType == SoulType.Bright)
        {
            // 밝은 형체가 보스에 닿으면 성공 이벤트 발생
            gimmickStrategy?.SucessTrigget();
            OnImpact();
        }
        else
        {
            // 어두운 형체가 보스에 닿으면 실패 이벤트 발생
            (gimmickStrategy as SoulGimmickStrategy)?.DarkSoulReachedBoss();
            OnImpact();
        }
    }

    // 플레이어 공격과의 충돌 처리
    private void OnTriggerEnter(Collider other)
    {
        // 플레이어 공격과 충돌했고, 어두운 형체인 경우에만 처리
        if (soulType == SoulType.Dark && other.CompareTag("Weapon"))
        {
            // 어두운 형체가 플레이어 공격에 맞으면 제거
            OnImpact();
        }
    }

    protected override void CheckDamage()
    {
        // 이 기믹은 데미지를 주지 않으므로 구현 불필요
    }

    protected override void OnImpact()
    {
        isDestroyed = true;
        // 기존 impactEffect 대신 타입별 이펙트 사용
        if (soulType == SoulType.Bright && brightImpactEffect != null)
        {
           
            Instantiate(brightImpactEffect, transform.position, Quaternion.identity);
        }
        else if (soulType == SoulType.Dark && darkImpactEffect != null)
        {
            CameraShakeManager.TriggerShake(0.2f, 0.5f);
            Instantiate(darkImpactEffect, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }

    protected override Vector3 GetRandomSpawnPosition()
    {
        // 원형 범위 끝에서 랜덤하게 위치 생성
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float x = Mathf.Cos(angle) * spawnAreaRadius;
        float z = Mathf.Sin(angle) * spawnAreaRadius;

        return new Vector3(x, 0.5f, z) + bossTransform.position;
    }

    protected override Vector3 GetAboveTargetPosition(Transform target)
    {
        // 이 기믹에서는 사용되지 않음
        return GetRandomSpawnPosition();
    }

    protected override Vector3 GetFixedSpawnPosition()
    {
        // 이 기믹에서는 사용되지 않음
        return GetRandomSpawnPosition();
    }
}