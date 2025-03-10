using System.Collections.Generic;
using UnityEngine;

public enum SoulType
{
    Bright,
    Dark
}

public class SoulEntity : HazardObject
{
    [SerializeField] private SoulType soulType;    
    [SerializeField] private float rotationSpeed = 30f;

    private Transform bossTransform;
    private bool isDestroyed = false;
    public SoulType SoulType => soulType;

    [Header("Soul Components")]
    [SerializeField] private GameObject brightSoulObject; // ���� ��ȥ ��ü ������Ʈ
    [SerializeField] private GameObject darkSoulObject;   // ��ο� ��ȥ ��ü ������Ʈ

    [Header("Impact Effects")]
    [SerializeField] private GameObject brightImpactEffect; // ���� ��ȥ �浹 ����Ʈ
    [SerializeField] private GameObject darkImpactEffect;   // ��ο� ��ȥ �浹 ����Ʈ
    public override void Initialize(float radius, float dmg, float speed, HazardSpawnType type,
        TargetType targetType, float height, float areaRadius, IGimmickStrategy strategy)
    {
        base.Initialize(radius, dmg, speed, type, targetType, height, areaRadius, strategy);

        // ���� Ʈ������ ã��
        var boss = GameObject.FindObjectOfType<BossAI>();
        if (boss != null)
        {
            bossTransform = boss.transform;
        }

        // �����ϰ� ��ȥ Ÿ�� ���� �Ǵ� �ܺο��� ������ Ÿ�� ���
        if (Random.value < 0.5f)
        {
            soulType = SoulType.Bright;
        }
        else
        {
            soulType = SoulType.Dark;
        }

        // Ÿ�Կ� ���� ���־� ����
        UpdateVisuals();
    }
    // SoulEntity �ʱ�ȭ �� Ÿ�� ����
    public void SetSoulType(SoulType type)
    {
        soulType = type;
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        // Ÿ�Կ� �´� ������Ʈ�� Ȱ��ȭ
        if (brightSoulObject != null) brightSoulObject.SetActive(soulType == SoulType.Bright);
        if (darkSoulObject != null) darkSoulObject.SetActive(soulType == SoulType.Dark);
    }

    public override void StartMove()
    {
        isWarning = false;
        if (warningIndicator != null)
        {
            warningIndicator.SetActive(false);
        }
    }

    protected override void Update()
    {
        base.Update();

        if (!isWarning && !isDestroyed && bossTransform != null)
        {
            // ������ ���� �̵�
            Vector3 direction = (bossTransform.position - transform.position).normalized;
            transform.position += direction * moveSpeed * Time.deltaTime;

            // ȸ�� ȿ�� �߰�
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

            // �������� �Ÿ� Ȯ��
            float distanceToBoss = Vector3.Distance(transform.position, bossTransform.position);
            if (distanceToBoss < 1.0f)
            {
                HandleBossCollision();
            }
        }
    }

    private void HandleBossCollision()
    {
        if (soulType == SoulType.Bright)
        {
            // ���� ��ü�� ������ ������ ���� �̺�Ʈ �߻�
            gimmickStrategy?.SucessTrigget();
            OnImpact();
        }
        else
        {
            // ��ο� ��ü�� ������ ������ ���� �̺�Ʈ �߻�
            (gimmickStrategy as SoulGimmickStrategy)?.DarkSoulReachedBoss();
            OnImpact();
        }
    }

    // �÷��̾� ���ݰ��� �浹 ó��
    private void OnTriggerEnter(Collider other)
    {
        // �÷��̾� ���ݰ� �浹�߰�, ��ο� ��ü�� ��쿡�� ó��
        if (soulType == SoulType.Dark && other.CompareTag("Weapon"))
        {
            // ��ο� ��ü�� �÷��̾� ���ݿ� ������ ����
            OnImpact();
        }
    }

    protected override void CheckDamage()
    {
        // �� ����� �������� ���� �����Ƿ� ���� ���ʿ�
    }

    protected override void OnImpact()
    {
        isDestroyed = true;
        // ���� impactEffect ��� Ÿ�Ժ� ����Ʈ ���
        if (soulType == SoulType.Bright && brightImpactEffect != null)
        {
           
            Instantiate(brightImpactEffect, transform.position, Quaternion.identity);
        }
        else if (soulType == SoulType.Dark && darkImpactEffect != null)
        {
            CameraShakeManager.TriggerShake(0.2f, 0.5f);
            Instantiate(darkImpactEffect, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }

    protected override Vector3 GetRandomSpawnPosition()
    {
        // ���� ���� ������ �����ϰ� ��ġ ����
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float x = Mathf.Cos(angle) * spawnAreaRadius;
        float z = Mathf.Sin(angle) * spawnAreaRadius;

        return new Vector3(x, 0.5f, z) + bossTransform.position;
    }

    protected override Vector3 GetAboveTargetPosition(Transform target)
    {
        // �� ��Ϳ����� ������ ����
        return GetRandomSpawnPosition();
    }

    protected override Vector3 GetFixedSpawnPosition()
    {
        // �� ��Ϳ����� ������ ����
        return GetRandomSpawnPosition();
    }
}