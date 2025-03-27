// PlayerDashComponent.cs - 기존 파티클 시스템 활용 버전
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class PlayerDashComponent : MonoBehaviour
{
    // 대시 스탯
    private float dashForce = StatConstants.BASE_DASH_FORCE;         // 대시 힘
    private float dashDuration = StatConstants.BASE_DASH_DURATION;   // 대시 지속 시간
    private float dashCooldown = StatConstants.BASE_DASH_COOLDOWN;   // 대시 쿨타임

    // 대시 상태
    private bool canDash = true;           // 대시 가능 여부
    private bool isDashing = false;        // 대시 중인지 여부
    private float cooldownTimer = 0f;      // 쿨다운 타이머

    // 참조 컴포넌트
    private PlayerClass playerClass;
    private Rigidbody rb;
    private PlayerMovement playerMovement;

    // 대시 이펙트 관련
    [SerializeField] private ParticleSystem dashParticleSystem;

    // 대시 관련 이벤트 추가
    public UnityEvent OnDashStart = new UnityEvent();
    public UnityEvent OnDashEnd = new UnityEvent();
    // UI 표시용 속성
    public float CooldownProgress => cooldownTimer / dashCooldown; // 0~1 사이 값

    private void Awake()
    {
        // 필요한 컴포넌트 찾기
        playerClass = GameInitializer.Instance.GetPlayerClass();
        rb = GetComponent<Rigidbody>();
        playerMovement = GetComponent<PlayerMovement>();

        if (playerClass == null || rb == null || playerMovement == null)
        {
            Debug.LogError("PlayerDashComponent: 필요한 컴포넌트가 없습니다.");
            enabled = false;
            return;
        }

        // 파티클 시스템이 없으면 찾기
        if (dashParticleSystem == null)
        {
            dashParticleSystem = GetComponentInChildren<ParticleSystem>();
            if (dashParticleSystem == null)
            {
                Debug.LogWarning("PlayerDashComponent: 대시용 파티클 시스템을 찾을 수 없습니다.");
            }
        }
    }

    private void Update()
    {
        // 쿨다운 타이머 업데이트
        if (!canDash && !isDashing)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0f)
            {
                cooldownTimer = 0f;
                canDash = true;
            }
        }

        // 대시 입력 확인
        if (Input.GetKeyDown(KeyCode.Space) && canDash && !isDashing && !playerClass.IsStunned)
        {
            StartCoroutine(DashCoroutine());
        }
    }

    // 대시 코루틴
    private IEnumerator DashCoroutine()
    {
        // 대시 시작
        isDashing = true;
        canDash = false;
        cooldownTimer = dashCooldown;

        // 대시 시작 이벤트 발생
        OnDashStart.Invoke();

        // 원래 드래그 값 저장
        float originalDrag = rb.drag;
        rb.drag = 0; // 대시 중에는 드래그를 0으로 설정

        // 대시 방향 (현재 이동 방향 또는 보는 방향)
        Vector3 dashDirection = playerMovement.GetCameraRelativeMovement().normalized;
        if (dashDirection.magnitude < 0.1f) // 움직이지 않을 때는 바라보는 방향으로 대시
        {
            dashDirection = transform.forward;
        }

        // 대시 이펙트 실행
        PlayDashEffect(dashDirection);

        // 대시 힘 적용
        rb.AddForce(dashDirection * dashForce, ForceMode.Impulse);

        // 대시 지속
        yield return new WaitForSeconds(dashDuration);

        // 대시 종료
        rb.drag = originalDrag;
        rb.velocity = rb.velocity * 0.3f; // 대시 후 속도 감소
        isDashing = false;

        // 대시 종료 이벤트 발생
        OnDashEnd.Invoke();
    }

    // 대시 이펙트 실행
    private void PlayDashEffect(Vector3 direction)
    {
        if (dashParticleSystem != null)
        {
            // 파티클 시스템 복제본을 현재 위치에 생성
            ParticleSystem newDashEffect = Instantiate(
                dashParticleSystem,
                transform.position,
                Quaternion.identity
            );

            // 파티클 방향 설정
            newDashEffect.transform.forward = direction;

            // 파티클 재생
            newDashEffect.Play();

            // 파티클 시스템 지속 시간 후 자동 삭제
            float particleDuration = newDashEffect.main.duration + newDashEffect.main.startLifetime.constant;
            Destroy(newDashEffect.gameObject, particleDuration);
        }
    }

    // 대시 힘 증가
    public void IncreaseDashForce(float amount)
    {
        dashForce += amount;
        dashForce = Mathf.Min(dashForce, StatConstants.MAX_DASH_FORCE);
        Debug.Log($"대시 힘 증가: +{amount}, 현재: {dashForce}");
    }

    // 대시 쿨타임 감소
    public void ReduceDashCooldown(float amount)
    {
        dashCooldown -= amount;
        dashCooldown = Mathf.Max(StatConstants.MIN_DASH_COOLDOWN, dashCooldown); // 최소 쿨타임 적용
        Debug.Log($"대시 쿨타임 감소: -{amount}초, 현재: {dashCooldown}초");
    }

    // 대시 지속시간 증가
    public void IncreaseDashDuration(float amount)
    {
        dashDuration += amount;
        dashDuration = Mathf.Min(dashDuration, StatConstants.MAX_DASH_DURATION);
        Debug.Log($"대시 지속시간 증가: +{amount}초, 현재: {dashDuration}초");
    }

    // 현재 대시 힘 반환
    public float GetDashForce()
    {
        return dashForce;
    }

    // 현재 대시 쿨타임 반환
    public float GetDashCooldown()
    {
        return dashCooldown;
    }

    // 현재 대시 지속시간 반환
    public float GetDashDuration()
    {
        return dashDuration;
    }

    // 대시 가능 여부 반환
    public bool CanDash()
    {
        return canDash && !isDashing;
    }

    // 대시 중인지 여부 반환
    public bool IsDashing()
    {
        return isDashing;
    }

    // 대시 설정 리셋
    public void ResetDashSettings()
    {
        dashForce = StatConstants.BASE_DASH_FORCE;
        dashDuration = StatConstants.BASE_DASH_DURATION;
        dashCooldown = StatConstants.BASE_DASH_COOLDOWN;

        canDash = true;
        cooldownTimer = 0f;

        Debug.Log("대시 설정 초기화됨");
    }
}