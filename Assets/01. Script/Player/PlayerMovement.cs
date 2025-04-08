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
    private PlayerDashComponent dashComponent;
    public Rigidbody rb;

    // ��� ���� ���� ����
    [SerializeField] private float movementSmoothing = 0.1f; // �̵� ������ ��

    private Vector3 moveDirection;
    private Vector3 currentVelocity;
    private Vector3 smoothVelocity;
  
    // 
    private bool canMove = true;
    private bool canRotate = true;
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


    // ���� �޼��� ���� - Update
    private void Update()
    {
        if (playerClass == null) return;

        // ī�޶� ���� Ȯ��
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

        // �̵� ���� Ȯ�� �߰�
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

        // ȸ�� ���� Ȯ�� �߰�
        if (canRotate)
        {
            RotateTowardsMouse();
        }
    }

    // ���� �޼��� ���� - FixedUpdate
    private void FixedUpdate()
    {
        if (playerClass == null) return;

        // �̵� ���� Ȯ�� �߰�
        if (canMove && (dashComponent == null || !dashComponent.IsDashing()))
        {
            SmoothMove(moveDirection);
        }
        else if (!canMove)
        {
            // �̵� �Ұ� �� ���� �ӵ� ����
            Vector3 velocity = rb.velocity;
            velocity.x = Mathf.Lerp(velocity.x, 0, 0.2f);
            velocity.z = Mathf.Lerp(velocity.z, 0, 0.2f);
            rb.velocity = velocity;
        }
    }
    // �̵� ���� (���̾�α� �ý��ۿ��� ȣ��)
    public void SetMovementEnabled(bool enabled)
    {
        canMove = enabled;
        dashComponent.enabled = enabled;
        // �̵� �Ұ� �� �ӵ� ��� ����
        if (!canMove && rb != null)
        {
            rb.velocity = new Vector3(0, rb.velocity.y, 0);
        }
    }
    // ȸ�� ���� (���̾�α� �ý��ۿ��� ȣ��)
    public void SetRotationEnabled(bool enabled)
    {
        canRotate = enabled;
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