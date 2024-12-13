using System.Collections;
using System.Collections.Generic;
using System.Text;
using DG.Tweening;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{/// <summary>
/// ��������� �̿�(�ٸ� ��ü���� �ڿ������� �浹�� �̵��� ����)
/// </summary>
    private PlayerClass playerClass;
    private Camera mainCamera;
    private Animator animator;
    private float smoothTime = 0.1f;
    private Vector2 currentAnimatorParameters;
    protected bool isDashing = false;
    public Rigidbody rb;

    [SerializeField] private float dashForce;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float movementSmoothing = 0.1f; // �̵� ������ ��

    private Vector3 moveDirection;
    private Vector3 currentVelocity;
    private Vector3 smoothVelocity;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("ī�޶� ã�� �� �����ϴ�.");
        }
    }

    private void Start()
    {
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

        moveDirection = GetCameraRelativeMovement();
        SetAnimatorParameters(moveDirection);
        RotateTowardsMouse();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(DashCoroutine());
        }
    }

    private void FixedUpdate()
    {
        if (playerClass == null) return;

        if (!isDashing)
        {
            SmoothMove(moveDirection);
        }
    }

    private Vector3 GetCameraRelativeMovement()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 cameraForward = mainCamera.transform.forward;
        cameraForward.y = 0;
        Vector3 cameraRight = mainCamera.transform.right;
        cameraRight.y = 0;

        return (cameraForward * vertical + cameraRight * horizontal).normalized;
    }

    public void SmoothMove(Vector3 direction)
    {
        if (playerClass == null) return;

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
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hitInfo))
        {
            Vector3 targetDirection = (hitInfo.point - playerClass.playerTransform.position).normalized;
            targetDirection.y = 0;

            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            playerClass.playerTransform.rotation = Quaternion.Slerp(
                playerClass.playerTransform.rotation,
                targetRotation,
                15f * Time.deltaTime
            );
        }
    }

    private IEnumerator DashCoroutine()
    {
        if (isDashing) yield break;
        
        isDashing = true;
        float originalDrag = rb.drag;
        rb.drag = 0; // ��� �߿��� �巡�׸� 0���� ����

        Vector3 dashDirection = moveDirection.normalized;
        rb.AddForce(dashDirection * dashForce, ForceMode.Impulse);
        
        yield return new WaitForSeconds(dashDuration);
        
        rb.drag = originalDrag; // ���� �巡�� ������ ����
        rb.velocity = rb.velocity * 0.3f; // ��� �� �ӵ� ����
        isDashing = false;
    }
}
