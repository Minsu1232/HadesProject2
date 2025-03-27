// PlayerDashComponent.cs - ���� ��ƼŬ �ý��� Ȱ�� ����
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class PlayerDashComponent : MonoBehaviour
{
    // ��� ����
    private float dashForce = StatConstants.BASE_DASH_FORCE;         // ��� ��
    private float dashDuration = StatConstants.BASE_DASH_DURATION;   // ��� ���� �ð�
    private float dashCooldown = StatConstants.BASE_DASH_COOLDOWN;   // ��� ��Ÿ��

    // ��� ����
    private bool canDash = true;           // ��� ���� ����
    private bool isDashing = false;        // ��� ������ ����
    private float cooldownTimer = 0f;      // ��ٿ� Ÿ�̸�

    // ���� ������Ʈ
    private PlayerClass playerClass;
    private Rigidbody rb;
    private PlayerMovement playerMovement;

    // ��� ����Ʈ ����
    [SerializeField] private ParticleSystem dashParticleSystem;

    // ��� ���� �̺�Ʈ �߰�
    public UnityEvent OnDashStart = new UnityEvent();
    public UnityEvent OnDashEnd = new UnityEvent();
    // UI ǥ�ÿ� �Ӽ�
    public float CooldownProgress => cooldownTimer / dashCooldown; // 0~1 ���� ��

    private void Awake()
    {
        // �ʿ��� ������Ʈ ã��
        playerClass = GameInitializer.Instance.GetPlayerClass();
        rb = GetComponent<Rigidbody>();
        playerMovement = GetComponent<PlayerMovement>();

        if (playerClass == null || rb == null || playerMovement == null)
        {
            Debug.LogError("PlayerDashComponent: �ʿ��� ������Ʈ�� �����ϴ�.");
            enabled = false;
            return;
        }

        // ��ƼŬ �ý����� ������ ã��
        if (dashParticleSystem == null)
        {
            dashParticleSystem = GetComponentInChildren<ParticleSystem>();
            if (dashParticleSystem == null)
            {
                Debug.LogWarning("PlayerDashComponent: ��ÿ� ��ƼŬ �ý����� ã�� �� �����ϴ�.");
            }
        }
    }

    private void Update()
    {
        // ��ٿ� Ÿ�̸� ������Ʈ
        if (!canDash && !isDashing)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0f)
            {
                cooldownTimer = 0f;
                canDash = true;
            }
        }

        // ��� �Է� Ȯ��
        if (Input.GetKeyDown(KeyCode.Space) && canDash && !isDashing && !playerClass.IsStunned)
        {
            StartCoroutine(DashCoroutine());
        }
    }

    // ��� �ڷ�ƾ
    private IEnumerator DashCoroutine()
    {
        // ��� ����
        isDashing = true;
        canDash = false;
        cooldownTimer = dashCooldown;

        // ��� ���� �̺�Ʈ �߻�
        OnDashStart.Invoke();

        // ���� �巡�� �� ����
        float originalDrag = rb.drag;
        rb.drag = 0; // ��� �߿��� �巡�׸� 0���� ����

        // ��� ���� (���� �̵� ���� �Ǵ� ���� ����)
        Vector3 dashDirection = playerMovement.GetCameraRelativeMovement().normalized;
        if (dashDirection.magnitude < 0.1f) // �������� ���� ���� �ٶ󺸴� �������� ���
        {
            dashDirection = transform.forward;
        }

        // ��� ����Ʈ ����
        PlayDashEffect(dashDirection);

        // ��� �� ����
        rb.AddForce(dashDirection * dashForce, ForceMode.Impulse);

        // ��� ����
        yield return new WaitForSeconds(dashDuration);

        // ��� ����
        rb.drag = originalDrag;
        rb.velocity = rb.velocity * 0.3f; // ��� �� �ӵ� ����
        isDashing = false;

        // ��� ���� �̺�Ʈ �߻�
        OnDashEnd.Invoke();
    }

    // ��� ����Ʈ ����
    private void PlayDashEffect(Vector3 direction)
    {
        if (dashParticleSystem != null)
        {
            // ��ƼŬ �ý��� �������� ���� ��ġ�� ����
            ParticleSystem newDashEffect = Instantiate(
                dashParticleSystem,
                transform.position,
                Quaternion.identity
            );

            // ��ƼŬ ���� ����
            newDashEffect.transform.forward = direction;

            // ��ƼŬ ���
            newDashEffect.Play();

            // ��ƼŬ �ý��� ���� �ð� �� �ڵ� ����
            float particleDuration = newDashEffect.main.duration + newDashEffect.main.startLifetime.constant;
            Destroy(newDashEffect.gameObject, particleDuration);
        }
    }

    // ��� �� ����
    public void IncreaseDashForce(float amount)
    {
        dashForce += amount;
        dashForce = Mathf.Min(dashForce, StatConstants.MAX_DASH_FORCE);
        Debug.Log($"��� �� ����: +{amount}, ����: {dashForce}");
    }

    // ��� ��Ÿ�� ����
    public void ReduceDashCooldown(float amount)
    {
        dashCooldown -= amount;
        dashCooldown = Mathf.Max(StatConstants.MIN_DASH_COOLDOWN, dashCooldown); // �ּ� ��Ÿ�� ����
        Debug.Log($"��� ��Ÿ�� ����: -{amount}��, ����: {dashCooldown}��");
    }

    // ��� ���ӽð� ����
    public void IncreaseDashDuration(float amount)
    {
        dashDuration += amount;
        dashDuration = Mathf.Min(dashDuration, StatConstants.MAX_DASH_DURATION);
        Debug.Log($"��� ���ӽð� ����: +{amount}��, ����: {dashDuration}��");
    }

    // ���� ��� �� ��ȯ
    public float GetDashForce()
    {
        return dashForce;
    }

    // ���� ��� ��Ÿ�� ��ȯ
    public float GetDashCooldown()
    {
        return dashCooldown;
    }

    // ���� ��� ���ӽð� ��ȯ
    public float GetDashDuration()
    {
        return dashDuration;
    }

    // ��� ���� ���� ��ȯ
    public bool CanDash()
    {
        return canDash && !isDashing;
    }

    // ��� ������ ���� ��ȯ
    public bool IsDashing()
    {
        return isDashing;
    }

    // ��� ���� ����
    public void ResetDashSettings()
    {
        dashForce = StatConstants.BASE_DASH_FORCE;
        dashDuration = StatConstants.BASE_DASH_DURATION;
        dashCooldown = StatConstants.BASE_DASH_COOLDOWN;

        canDash = true;
        cooldownTimer = 0f;

        Debug.Log("��� ���� �ʱ�ȭ��");
    }
}