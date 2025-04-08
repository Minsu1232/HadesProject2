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
    private PlayerDashComponent dashComponent;
    public Rigidbody rb;

    // 대시 관련 변수 제거
    [SerializeField] private float movementSmoothing = 0.1f; // 이동 스무딩 값

    private Vector3 moveDirection;
    private Vector3 currentVelocity;
    private Vector3 smoothVelocity;
  
    // 
    private bool canMove = true;
    private bool canRotate = true;
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


    // 기존 메서드 수정 - Update
    private void Update()
    {
        if (playerClass == null) return;

        // 카메라 참조 확인
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null) return;
        }

        if (playerClass.IsStunned)
        {
            rb.velocity = Vector3.zero;
            return;
        }

        // 이동 제어 확인 추가
        if (canMove)
        {
            moveDirection = GetCameraRelativeMovement();
            SetAnimatorParameters(moveDirection);
        }
        else
        {
            moveDirection = Vector3.zero;
            SetAnimatorParameters(Vector3.zero);
        }

        // 회전 제어 확인 추가
        if (canRotate)
        {
            RotateTowardsMouse();
        }
    }

    // 기존 메서드 수정 - FixedUpdate
    private void FixedUpdate()
    {
        if (playerClass == null) return;

        // 이동 제어 확인 추가
        if (canMove && (dashComponent == null || !dashComponent.IsDashing()))
        {
            SmoothMove(moveDirection);
        }
        else if (!canMove)
        {
            // 이동 불가 시 수평 속도 감소
            Vector3 velocity = rb.velocity;
            velocity.x = Mathf.Lerp(velocity.x, 0, 0.2f);
            velocity.z = Mathf.Lerp(velocity.z, 0, 0.2f);
            rb.velocity = velocity;
        }
    }
    // 이동 제어 (다이얼로그 시스템에서 호출)
    public void SetMovementEnabled(bool enabled)
    {
        canMove = enabled;
        dashComponent.enabled = enabled;
        // 이동 불가 시 속도 즉시 멈춤
        if (!canMove && rb != null)
        {
            rb.velocity = new Vector3(0, rb.velocity.y, 0);
        }
    }
    // 회전 제어 (다이얼로그 시스템에서 호출)
    public void SetRotationEnabled(bool enabled)
    {
        canRotate = enabled;
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