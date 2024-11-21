using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private PlayerClass playerClass; // PlayerClass 인스턴스를 참조
    private Camera mainCamera;
    private Animator animator;
    private float smoothTime = 0.1f; // 애니메이션 파라미터 전환 속도
    private Vector2 currentAnimatorParameters; // 현재 Horizontal, Vertical 값을 저장

    void Awake()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("카메라를 찾을 수 없습니다.");
        }
    }

    private void Start()
    {
        // GameInitializer를 통해 PlayerClass를 가져와 초기화
        playerClass = GameInitializer.Instance.GetPlayerClass();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (playerClass == null) return;

        // 카메라 기준 이동 방향 설정 (고정된 동서남북 기준)
        Vector3 moveDirection = GetCameraRelativeMovement();

        // 이동 처리
        Move(moveDirection);

        // Animator 파라미터 설정
        SetAnimatorParameters(moveDirection);

        // 마우스 방향에 따라 캐릭터 회전
        RotateTowardsMouse();

        // 대시 기능
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Dash(moveDirection * 15f);
        }
    }

    // WSAD에 따라 카메라 기준 고정된 이동 방향 계산
    private Vector3 GetCameraRelativeMovement()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 cameraForward = mainCamera.transform.forward;
        cameraForward.y = 0;
        Vector3 cameraRight = mainCamera.transform.right;
        cameraRight.y = 0;

        Vector3 moveDirection = (cameraForward * vertical + cameraRight * horizontal).normalized;
        return moveDirection;
    }

    public void Move(Vector3 direction)
    {
        if (playerClass == null || direction.magnitude == 0) return;

        // 이동: WSAD로 입력된 고정된 방향으로만 이동
        playerClass.rb.MovePosition(playerClass.rb.position + direction * playerClass.CurrentSpeed * Time.fixedDeltaTime);
    }

    private void SetAnimatorParameters(Vector3 moveDirection)
    {
        Vector3 localMoveDirection = transform.InverseTransformDirection(moveDirection);

        // Lerp를 사용하여 값을 부드럽게 변경
        currentAnimatorParameters.x = Mathf.Lerp(currentAnimatorParameters.x, localMoveDirection.x, Time.deltaTime / smoothTime);
        currentAnimatorParameters.y = Mathf.Lerp(currentAnimatorParameters.y, localMoveDirection.z, Time.deltaTime / smoothTime);

        animator.SetFloat("Horizontal", currentAnimatorParameters.x);
        animator.SetFloat("Vertical", currentAnimatorParameters.y);
    }

    public void RotateTowardsMouse()
    {
        // 마우스 방향에 따라 캐릭터 회전
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hitInfo))
        {
            Vector3 targetDirection = (hitInfo.point - playerClass.playerTransform.position).normalized;
            targetDirection.y = 0;

            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            playerClass.playerTransform.rotation = Quaternion.Slerp(playerClass.playerTransform.rotation, targetRotation, 10 * Time.deltaTime);
        }
    }

    public void Dash(Vector3 direction)
    {
        playerClass?.Dash(direction); // Dash 호출을 PlayerClass에 위임
    }
}
