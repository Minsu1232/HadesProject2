using System.Collections;
using System.Collections.Generic;
using System.Text;
using DG.Tweening;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    /// <summary>
    /// ��������� �̿�(�ٸ� ��ü���� �ڿ������� �浹�� �̵��� ����)
    /// </summary>
    private PlayerClass playerClass;
    private Camera mainCamera;
    private Animator animator;
    private float smoothTime = 0.1f;
    private Vector2 currentAnimatorParameters;
    // isDashing �����ϰ� ��� ������Ʈ ���� �߰�
    private PlayerDashComponent dashComponent;
    public Rigidbody rb;

    // ��� ���� ���� ����
    [SerializeField] private float movementSmoothing = 0.1f; // �̵� ������ ��

    private Vector3 moveDirection;
    private Vector3 currentVelocity;
    private Vector3 smoothVelocity;

    void Awake()
    {
        // ��� ������Ʈ ���� ����
        dashComponent = GetComponent<PlayerDashComponent>();
        if (dashComponent == null)
        {
            dashComponent = gameObject.AddComponent<PlayerDashComponent>();
        }
    }

    private void OnEnable()
    {
        // �� ���� �� ī�޶� ���� ������Ʈ
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("ī�޶� ã�� �� �����ϴ�.");
        }
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("ī�޶� ã�� �� �����ϴ�.");
        }
        playerClass = GameInitializer.Instance.GetPlayerClass();
        animator = GetComponent<Animator>();

        // ������ٵ� ����
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    private void Update()
    {
        if (playerClass == null) return;

        // ī�޶� ���� Ȯ��
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null) return; // ī�޶� ������ �̵� ó�� �ǳʶٱ�
        }

        if (playerClass.IsStunned)
        {
            rb.velocity = Vector3.zero;
            return;
        }

        moveDirection = GetCameraRelativeMovement();
        SetAnimatorParameters(moveDirection);
        RotateTowardsMouse();

        // ��� ó���� PlayerDashComponent���� �����ϹǷ� ����
    }

    private void FixedUpdate()
    {
        if (playerClass == null) return;

        // ��� ���� �ƴ� ���� �̵� ó��
        if (dashComponent == null || !dashComponent.IsDashing())
        {
            SmoothMove(moveDirection);
        }
    }

    // PlayerMovement Ŭ������ �߰�
    public void TeleportTo(Vector3 position)
    {
        // ���� ��� �Ͻ� ����
        bool wasKinematic = rb.isKinematic;
        rb.isKinematic = true;

        // �ӵ� �� ���ӵ� �ʱ�ȭ
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // ��ġ ����
        transform.position = position;

        // PlayerClass�� ��ġ�� ����ȭ (�ʿ��� ���)
        if (playerClass != null && playerClass.playerTransform != null)
        {
            playerClass.playerTransform.position = position;
        }

        // ���� ���� ����
        rb.isKinematic = wasKinematic;

        // �̵� ���� ���� �ʱ�ȭ
        moveDirection = Vector3.zero;
        smoothVelocity = Vector3.zero;

        Debug.Log($"�÷��̾� �ڷ���Ʈ �Ϸ�: {position}");
    }

    // ī�޶� ���� �̵� ���� ��� - public���� �����Ͽ� DashComponent���� ������ �� �ְ� ��
    public Vector3 GetCameraRelativeMovement()
    {
        // ���� üũ
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

        // ��ǥ �ӵ� ���
        Vector3 targetVelocity = direction * playerClass.PlayerStats.Speed;
        targetVelocity.y = rb.velocity.y; // ���� �ӵ��� ����

        // ���� �ӵ����� ��ǥ �ӵ��� �ε巴�� ��ȯ
        Vector3 smoothedVelocity = Vector3.SmoothDamp(
            rb.velocity,
            targetVelocity,
            ref smoothVelocity,
            movementSmoothing
        );

        // �ӵ� ����
        rb.velocity = smoothedVelocity;

        // ���� �� ������ ���ߵ��� ó��
        if (direction.magnitude == 0)
        {
            // ���� �ӵ��� ����
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
            targetDirection.y = 0; // y�� ȸ���� ���

            // ���� ������ ũ�Ⱑ ����� Ŭ ���� ȸ�� ���� (����� �Ÿ��� ����)
            if (targetDirection.sqrMagnitude > 0.25f)
            {
                targetDirection.Normalize();

                // ���� ������ ��ǥ ���� ������ ���� ���
                float currentAngle = playerClass.playerTransform.eulerAngles.y;
                float targetAngle = Quaternion.LookRotation(targetDirection).eulerAngles.y;
                float angleDifference = Mathf.Abs(Mathf.DeltaAngle(currentAngle, targetAngle));

                // ���� ���̰� ����� Ŭ ���� ȸ�� (�̼��� ��ȭ ����)
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

    // DashCoroutine �޼��� ���� (PlayerDashComponent�� �̵�)
}