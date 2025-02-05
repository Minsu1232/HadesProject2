using DG.Tweening.Core.Easing;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public enum HazardSpawnType
{
    Random,         // 랜덤 위치에서 생성
    AbovePlayer,   // 플레이어 위에서 생성
    AboveBoss,     // 보스 위에서 생성
    FixedPoints    // 정해진 위치에서 생성
}
public enum TargetType
{
    None,       // 타겟 없음
    Player,     // 플레이어 추적
    Boss,       // 보스 추적
    NearestEnemy,  // 가장 가까운 적
    CustomPosition  // 지정된 위치 (gimmickPosition 사용)
}
public abstract class HazardObject : MonoBehaviour
{
    [Header("시각 효과")]
    [SerializeField] protected GameObject warningIndicatorPrefab;  // Warning Quad 프리팹
    protected GameObject warningIndicator;  // 생성된 인스턴스
    [SerializeField] protected GameObject impactEffect;      // 충돌시 이펙트

    protected float warningDuration;  // 경고 지속 시간
    protected float damageRadius;    // 데미지 범위
    protected float damage;          // 데미지량
    protected float moveSpeed;       // 이동 속도
    protected bool isWarning = true; // 경고 상태 여부
    protected HazardSpawnType hazardSpawnType;   // 스폰 타입
    protected TargetType targetType;     // 타겟 타입 추가
    protected IGimmickStrategy gimmickStrategy;

      protected Transform currentTarget;  // 현재 타겟 캐싱

    protected float spawnHeight;
    // Fill 효과를 위한 변수 추가
    private Material warningMaterial;
    protected float warningTimer;
   

    /// <summary>
    /// 위험 오브젝트의 기본 정보를 초기화합니다.
    /// </summary>
    /// <param name="radius">데미지 범위</param>
    /// <param name="dmg">데미지량</param>
    /// <param name="speed">이동 속도</param>
    /// <param name="type">스폰 타입</param>
    /// <param name="target">타겟 트랜스폼 (선택사항)</param>
    public virtual void Initialize(float radius, float dmg, float speed, HazardSpawnType type, TargetType targetType)
    {
        this.damageRadius = radius;
        this.damage = dmg;
        this.moveSpeed = speed;
        this.hazardSpawnType = type;
        this.targetType = targetType;
        InitializeWarning();
    }

    //오버로딩된 Initialize 추가 (Stalactite 등 추가 정보가 필요한 경우 사용)
    public virtual void Initialize(float radius, float dmg, float speed, HazardSpawnType type,
   TargetType targetType, float height, float areaRadius, IGimmickStrategy strategy)
    {
        Initialize(radius, dmg, speed, type, targetType); // 기본 Initialize 호출        
        this.gimmickStrategy = strategy;        
        Debug.Log("등장");
    }

    /// <summary>
    /// 경고 표시를 초기화합니다.
    /// </summary>
    protected virtual void InitializeWarning()
    {
        if (warningIndicatorPrefab != null)
        {
            // Raycast로 표면 위치 찾기
            RaycastHit hit;
            Vector3 rayStart = transform.position + Vector3.up * 100f;

            if (Physics.Raycast(rayStart, Vector3.down, out hit, 1000f, LayerMask.GetMask("Ground")))
            {
                // 표면 위 약간 띄워서 위치 잡기
                Vector3 spawnPos = hit.point + (Vector3.up * 0.01f);

                // 표면 노말에 맞춰 회전 (경사진 지형 대응)
                Quaternion surfaceRotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
                warningIndicator = Instantiate(warningIndicatorPrefab, spawnPos, Quaternion.Euler(90, 0, 0));
            }
            else
            {
                // Raycast 실패시 기본 위치에 생성
                warningIndicator = Instantiate(warningIndicatorPrefab, transform.position, Quaternion.Euler(90, 0, 0));
            }

            // 크기 설정
            warningIndicator.transform.localScale = new Vector3(damageRadius * 2, damageRadius * 2, 1);

            // 렌더러 설정
            MeshRenderer renderer = warningIndicator.GetComponentInChildren<MeshRenderer>();
            warningMaterial = new Material(renderer.material);
            renderer.material = warningMaterial;

            // 레이어 설정
            warningIndicator.layer = LayerMask.NameToLayer("WarningEffect");

            warningTimer = warningDuration;

            // Fill Amount 초기화
            warningMaterial.SetFloat("_FillAmount", 0f);
        }
    }
    protected virtual float CalculateWarningDuration(float height)
    {
        // 자유 낙하 시간 = 거리 / 속도
        return height / moveSpeed;
    }


    protected virtual void Update()
    {
        if (isWarning)
        {
            UpdateWarningEffect();
        }
    }

    private void UpdateWarningEffect()
    {
        if (warningMaterial == null) return;

        warningTimer -= Time.deltaTime;
        float fillAmount = 1f - (warningTimer / warningDuration);
        fillAmount = Mathf.Clamp01(fillAmount);  // 0~1 사이값으로 제한
        warningMaterial.SetFloat("_FillAmount", fillAmount);

        OnWarningUpdate();  // 자식 클래스를 위한 가상 메서드
    }
    #region 생성위치매서드
    // Transform을 얻는 새로운 메서드 추가
    // GetTargetTransform 수정
    protected Transform GetTargetTransform()
    {
        switch (targetType)
        {
            case TargetType.Player:
                return GameInitializer.Instance.GetPlayerClass().playerTransform.transform;
            //case TargetType.Boss:
            //    return GameManager.Instance.CurrentBoss?.transform;
            //case TargetType.NearestEnemy:
            //    return FindNearestEnemy();
            default:
                return null;
        }
    }
    /// <summary>
    /// 스폰 타입에 따른 생성 위치를 반환합니다.
    /// </summary>
    /// <returns>생성될 위치</returns>
     // 스폰 포지션 수정
    public virtual Vector3 GetSpawnPosition()
    {
        currentTarget = GetTargetTransform();  // 타겟 갱신

        switch (hazardSpawnType)
        {
            case HazardSpawnType.Random:
                return GetRandomSpawnPosition();
            case HazardSpawnType.AbovePlayer:
            case HazardSpawnType.AboveBoss:
                if (currentTarget == null) return GetRandomSpawnPosition();
                return GetAboveTargetPosition(currentTarget);
            case HazardSpawnType.FixedPoints:
                return GetFixedSpawnPosition();
            default:
                return Vector3.zero;
        }
    }
    // 타겟 위치 업데이트 메서드 추가
    protected virtual void UpdateTargetPosition()
    {
        if (currentTarget == null)
            currentTarget = GetTargetTransform();
    }

    // 실시간 타겟 위치 가져오기
    protected Vector3 GetCurrentTargetPosition()
    {
        UpdateTargetPosition();
        return currentTarget != null ? currentTarget.position : transform.position;
    }
    #endregion

    /// <summary>
    /// 오브젝트의 이동을 시작합니다.
    /// </summary>
    public abstract void StartMove();

    /// <summary>
    /// 데미지 판정을 체크합니다.
    /// </summary>
    protected abstract void CheckDamage();

    /// <summary>
    /// 충돌시 발생하는 효과를 처리합니다.
    /// </summary>
    protected abstract void OnImpact();

    /// <summary>
    /// 랜덤한 생성 위치를 반환합니다.
    /// </summary>
    protected abstract Vector3 GetRandomSpawnPosition();

    /// <summary>
    /// 대상 위의 생성 위치를 반환합니다.
    /// </summary>
    /// <param name="target">대상의 Transform</param>
    protected abstract Vector3 GetAboveTargetPosition(Transform target);

    /// <summary>
    /// 미리 정의된 위치 중 하나를 반환합니다.
    /// </summary>
    protected abstract Vector3 GetFixedSpawnPosition();

    protected virtual void OnWarningUpdate() { }  // 자식 클래스에서 오버라이드
}