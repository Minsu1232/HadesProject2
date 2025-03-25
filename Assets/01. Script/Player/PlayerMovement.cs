using System.Collections;
using System.Collections.Generic;
using System.Text;
using DG.Tweening;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    /// <summary>
    /// 물리기반을 이용(다른 물체와의 자연스러운 충돌과 이동을 위해)
    /// </summary>
    private PlayerClass playerClass;
    private Camera mainCamera;
    private Animator animator;
    private float smoothTime = 0.1f;
    private Vector2 currentAnimatorParameters;
    // isDashing 제거하고 대시 컴포넌트 참조 추가
    private PlayerDashComponent dashComponent;
    public Rigidbody rb;

    // 대시 관련 변수 제거
    [SerializeField] private float movementSmoothing = 0.1f; // 이동 스무딩 값

    private Vector3 moveDirection;
    private Vector3 currentVelocity;
    private Vector3 smoothVelocity;

    void Awake()
    {
        // 대시 컴포넌트 참조 설정
        dashComponent = GetComponent<PlayerDashComponent>();
        if (dashComponent == null)
        {
            dashComponent = gameObject.AddComponent<PlayerDashComponent>();
        }
    }

    private void OnEnable()
    {
        // 씬 변경 시 카메라 참조 업데이트
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("카메라를 찾을 수 없습니다.");
        }
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("카메라를 찾을 수 없습니다.");
        }
        playerClass = GameInitializer.Instance.GetPlayerClass();
        animator = GetComponent<Animator>();

        // 리지드바디 설정
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    private void Update()
    {
        if (playerClass == null) return;

        // 카메라 참조 확인
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null) return; // 카메라가 없으면 이동 처리 건너뛰기
        }

        if (playerClass.IsStunned)
        {
            rb.velocity = Vector3.zero;
            return;
        }

        moveDirection = GetCameraRelativeMovement();
        SetAnimatorParameters(moveDirection);
        RotateTowardsMouse();

        // 대시 처리는 PlayerDashComponent에서 관리하므로 제거
    }

    private void FixedUpdate()
    {
        if (playerClass == null) return;

        // 대시 중이 아닐 때만 이동 처리
        if (dashComponent == null || !dashComponent.IsDashing())
        {
            SmoothMove(moveDirection);
        }
    }

    // PlayerMovement 클래스에 추가
    public void TeleportTo(Vector3 position)
    {
        // 물리 계산 일시 중지
        bool wasKinematic = rb.isKinematic;
        rb.isKinematic = true;

        // 속도 및 가속도 초기화
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // 위치 설정
        transform.position = position;

        // PlayerClass의 위치도 동기화 (필요한 경우)
        if (playerClass != null && playerClass.playerTransform != null)
        {
            playerClass.playerTransform.position = position;
        }

        // 물리 상태 복원
        rb.isKinematic = wasKinematic;

        // 이동 관련 변수 초기화
        moveDirection = Vector3.zero;
        smoothVelocity = Vector3.zero;

        Debug.Log($"플레이어 텔레포트 완료: {position}");
    }

    // 카메라 기준 이동 방향 얻기 - public으로 변경하여 DashComponent에서 참조할 수 있게 함
    public Vector3 GetCameraRelativeMovement()
    {
        // 안전 체크
        if (mainCamera == null)
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            return new Vector3(horizontal, 0, vertical).normalized;
        }

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 cameraForward = mainCamera.transform.forward;
        cameraForward.y = 0;
        Vector3 cameraRight = mainCamera.transform.right;
        cameraRight.y = 0;

        return (cameraForward * v + cameraRight * h).normalized;
    }

    public void SmoothMove(Vector3 direction)
    {
        if (playerClass == null || rb.isKinematic) return;

        // 목표 속도 계산
        Vector3 targetVelocity = direction * playerClass.PlayerStats.Speed;
        targetVelocity.y = rb.velocity.y; // 수직 속도는 유지

        // 현재 속도에서 목표 속도로 부드럽게 전환
        Vector3 smoothedVelocity = Vector3.SmoothDamp(
            rb.velocity,
            targetVelocity,
            ref smoothVelocity,
            movementSmoothing
        );

        // 속도 적용
        rb.velocity = smoothedVelocity;

        // 정지 시 빠르게 멈추도록 처리
        if (direction.magnitude == 0)
        {
            // 수평 속도만 감소
            Vector3 horizontalVelocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
            if (horizontalVelocity.magnitude < 0.1f)
            {
                rb.velocity = new Vector3(0, rb.velocity.y, 0);
            }
        }
    }

    private void SetAnimatorParameters(Vector3 moveDirection)
    {
        Vector3 localMoveDirection = transform.InverseTransformDirection(moveDirection);

        currentAnimatorParameters.x = Mathf.Lerp(currentAnimatorParameters.x, localMoveDirection.x, Time.deltaTime / smoothTime);
        currentAnimatorParameters.y = Mathf.Lerp(currentAnimatorParameters.y, localMoveDirection.z, Time.deltaTime / smoothTime);

        animator.SetFloat("Horizontal", currentAnimatorParameters.x);
        animator.SetFloat("Vertical", currentAnimatorParameters.y);
    }

    public void RotateTowardsMouse()
    {
        if (mainCamera == null) return;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hitInfo))
        {
            Vector3 targetDirection = (hitInfo.point - playerClass.playerTransform.position);
            targetDirection.y = 0; // y축 회전만 고려

            // 방향 벡터의 크기가 충분히 클 때만 회전 적용 (가까운 거리는 무시)
            if (targetDirection.sqrMagnitude > 0.25f)
            {
                targetDirection.Normalize();

                // 현재 각도와 목표 각도 사이의 차이 계산
                float currentAngle = playerClass.playerTransform.eulerAngles.y;
                float targetAngle = Quaternion.LookRotation(targetDirection).eulerAngles.y;
                float angleDifference = Mathf.Abs(Mathf.DeltaAngle(currentAngle, targetAngle));

                // 각도 차이가 충분히 클 때만 회전 (미세한 변화 무시)
                if (angleDifference > 2.0f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
                    playerClass.playerTransform.rotation = Quaternion.Slerp(
                        playerClass.playerTransform.rotation,
                        targetRotation,
                        15f * Time.deltaTime
                    );
                }
            }
        }
    }

    // DashCoroutine 메서드 제거 (PlayerDashComponent로 이동)
}