using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private PlayerClass playerClass; // PlayerClass �ν��Ͻ��� ����
    private Camera mainCamera;
    private Animator animator;
    private float smoothTime = 0.1f; // �ִϸ��̼� �Ķ���� ��ȯ �ӵ�
    private Vector2 currentAnimatorParameters; // ���� Horizontal, Vertical ���� ����

    void Awake()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("ī�޶� ã�� �� �����ϴ�.");
        }
    }

    private void Start()
    {
        // GameInitializer�� ���� PlayerClass�� ������ �ʱ�ȭ
        playerClass = GameInitializer.Instance.GetPlayerClass();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (playerClass == null) return;

        // ī�޶� ���� �̵� ���� ���� (������ �������� ����)
        Vector3 moveDirection = GetCameraRelativeMovement();

        // �̵� ó��
        Move(moveDirection);

        // Animator �Ķ���� ����
        SetAnimatorParameters(moveDirection);

        // ���콺 ���⿡ ���� ĳ���� ȸ��
        RotateTowardsMouse();

        // ��� ���
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Dash(moveDirection * 15f);
        }
    }

    // WSAD�� ���� ī�޶� ���� ������ �̵� ���� ���
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

        // �̵�: WSAD�� �Էµ� ������ �������θ� �̵�
        playerClass.rb.MovePosition(playerClass.rb.position + direction * playerClass.CurrentSpeed * Time.fixedDeltaTime);
    }

    private void SetAnimatorParameters(Vector3 moveDirection)
    {
        Vector3 localMoveDirection = transform.InverseTransformDirection(moveDirection);

        // Lerp�� ����Ͽ� ���� �ε巴�� ����
        currentAnimatorParameters.x = Mathf.Lerp(currentAnimatorParameters.x, localMoveDirection.x, Time.deltaTime / smoothTime);
        currentAnimatorParameters.y = Mathf.Lerp(currentAnimatorParameters.y, localMoveDirection.z, Time.deltaTime / smoothTime);

        animator.SetFloat("Horizontal", currentAnimatorParameters.x);
        animator.SetFloat("Vertical", currentAnimatorParameters.y);
    }

    public void RotateTowardsMouse()
    {
        // ���콺 ���⿡ ���� ĳ���� ȸ��
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
        playerClass?.Dash(direction); // Dash ȣ���� PlayerClass�� ����
    }
}
