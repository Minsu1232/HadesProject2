using DG.Tweening.Core.Easing;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public enum HazardSpawnType
{
    Random,         // ���� ��ġ���� ����
    AbovePlayer,   // �÷��̾� ������ ����
    AboveBoss,     // ���� ������ ����
    FixedPoints    // ������ ��ġ���� ����
}
public enum TargetType
{
    None,       // Ÿ�� ����
    Player,     // �÷��̾� ����
    Boss,       // ���� ����
    NearestEnemy,  // ���� ����� ��
    CustomPosition  // ������ ��ġ (gimmickPosition ���)
}
public abstract class HazardObject : MonoBehaviour
{
    [Header("�ð� ȿ��")]
    [SerializeField] protected GameObject warningIndicatorPrefab;  // Warning Quad ������
    protected GameObject warningIndicator;  // ������ �ν��Ͻ�
    [SerializeField] protected GameObject impactEffect;      // �浹�� ����Ʈ

    protected float warningDuration;  // ��� ���� �ð�
    protected float damageRadius;    // ������ ����
    protected float damage;          // ��������
    protected float moveSpeed;       // �̵� �ӵ�
    protected bool isWarning = true; // ��� ���� ����
    protected HazardSpawnType hazardSpawnType;   // ���� Ÿ��
    protected TargetType targetType;     // Ÿ�� Ÿ�� �߰�
    protected IGimmickStrategy gimmickStrategy;

      protected Transform currentTarget;  // ���� Ÿ�� ĳ��

    protected float spawnHeight;
    // Fill ȿ���� ���� ���� �߰�
    private Material warningMaterial;
    protected float warningTimer;
   

    /// <summary>
    /// ���� ������Ʈ�� �⺻ ������ �ʱ�ȭ�մϴ�.
    /// </summary>
    /// <param name="radius">������ ����</param>
    /// <param name="dmg">��������</param>
    /// <param name="speed">�̵� �ӵ�</param>
    /// <param name="type">���� Ÿ��</param>
    /// <param name="target">Ÿ�� Ʈ������ (���û���)</param>
    public virtual void Initialize(float radius, float dmg, float speed, HazardSpawnType type, TargetType targetType)
    {
        this.damageRadius = radius;
        this.damage = dmg;
        this.moveSpeed = speed;
        this.hazardSpawnType = type;
        this.targetType = targetType;
        InitializeWarning();
    }

    //�����ε��� Initialize �߰� (Stalactite �� �߰� ������ �ʿ��� ��� ���)
    public virtual void Initialize(float radius, float dmg, float speed, HazardSpawnType type,
   TargetType targetType, float height, float areaRadius, IGimmickStrategy strategy)
    {
        Initialize(radius, dmg, speed, type, targetType); // �⺻ Initialize ȣ��        
        this.gimmickStrategy = strategy;        
        Debug.Log("����");
    }

    /// <summary>
    /// ��� ǥ�ø� �ʱ�ȭ�մϴ�.
    /// </summary>
    protected virtual void InitializeWarning()
    {
        if (warningIndicatorPrefab != null)
        {
            // Raycast�� ǥ�� ��ġ ã��
            RaycastHit hit;
            Vector3 rayStart = transform.position + Vector3.up * 100f;

            if (Physics.Raycast(rayStart, Vector3.down, out hit, 1000f, LayerMask.GetMask("Ground")))
            {
                // ǥ�� �� �ణ ����� ��ġ ���
                Vector3 spawnPos = hit.point + (Vector3.up * 0.01f);

                // ǥ�� �븻�� ���� ȸ�� (����� ���� ����)
                Quaternion surfaceRotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
                warningIndicator = Instantiate(warningIndicatorPrefab, spawnPos, Quaternion.Euler(90, 0, 0));
            }
            else
            {
                // Raycast ���н� �⺻ ��ġ�� ����
                warningIndicator = Instantiate(warningIndicatorPrefab, transform.position, Quaternion.Euler(90, 0, 0));
            }

            // ũ�� ����
            warningIndicator.transform.localScale = new Vector3(damageRadius * 2, damageRadius * 2, 1);

            // ������ ����
            MeshRenderer renderer = warningIndicator.GetComponentInChildren<MeshRenderer>();
            warningMaterial = new Material(renderer.material);
            renderer.material = warningMaterial;

            // ���̾� ����
            warningIndicator.layer = LayerMask.NameToLayer("WarningEffect");

            warningTimer = warningDuration;

            // Fill Amount �ʱ�ȭ
            warningMaterial.SetFloat("_FillAmount", 0f);
        }
    }
    protected virtual float CalculateWarningDuration(float height)
    {
        // ���� ���� �ð� = �Ÿ� / �ӵ�
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
        fillAmount = Mathf.Clamp01(fillAmount);  // 0~1 ���̰����� ����
        warningMaterial.SetFloat("_FillAmount", fillAmount);

        OnWarningUpdate();  // �ڽ� Ŭ������ ���� ���� �޼���
    }
    #region ������ġ�ż���
    // Transform�� ��� ���ο� �޼��� �߰�
    // GetTargetTransform ����
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
    /// ���� Ÿ�Կ� ���� ���� ��ġ�� ��ȯ�մϴ�.
    /// </summary>
    /// <returns>������ ��ġ</returns>
     // ���� ������ ����
    public virtual Vector3 GetSpawnPosition()
    {
        currentTarget = GetTargetTransform();  // Ÿ�� ����

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
    // Ÿ�� ��ġ ������Ʈ �޼��� �߰�
    protected virtual void UpdateTargetPosition()
    {
        if (currentTarget == null)
            currentTarget = GetTargetTransform();
    }

    // �ǽð� Ÿ�� ��ġ ��������
    protected Vector3 GetCurrentTargetPosition()
    {
        UpdateTargetPosition();
        return currentTarget != null ? currentTarget.position : transform.position;
    }
    #endregion

    /// <summary>
    /// ������Ʈ�� �̵��� �����մϴ�.
    /// </summary>
    public abstract void StartMove();

    /// <summary>
    /// ������ ������ üũ�մϴ�.
    /// </summary>
    protected abstract void CheckDamage();

    /// <summary>
    /// �浹�� �߻��ϴ� ȿ���� ó���մϴ�.
    /// </summary>
    protected abstract void OnImpact();

    /// <summary>
    /// ������ ���� ��ġ�� ��ȯ�մϴ�.
    /// </summary>
    protected abstract Vector3 GetRandomSpawnPosition();

    /// <summary>
    /// ��� ���� ���� ��ġ�� ��ȯ�մϴ�.
    /// </summary>
    /// <param name="target">����� Transform</param>
    protected abstract Vector3 GetAboveTargetPosition(Transform target);

    /// <summary>
    /// �̸� ���ǵ� ��ġ �� �ϳ��� ��ȯ�մϴ�.
    /// </summary>
    protected abstract Vector3 GetFixedSpawnPosition();

    protected virtual void OnWarningUpdate() { }  // �ڽ� Ŭ�������� �������̵�
}