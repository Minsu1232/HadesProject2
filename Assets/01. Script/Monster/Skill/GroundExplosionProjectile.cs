using System.Collections;
using UnityEngine;

public class GroundExplosionProjectile : BaseProjectile
{
    public float safeZoneRadius; // ���� ���� �ݰ�
    public float dangerRadius; // ���� ���� �ݰ�(safe�� 1.5~2��)
    public float explosionDelay; // ���� ���� �ð�
    public bool isRingShaped = true; // true: �������� �ٱ��� ������, false: ��ü ���� ������

    private float timer = 0f;
    private bool hasExploded = false;
    private float essenceAmount;
    private bool isGrounded = false;
    private bool damageApplied = false; // �� �� �������� ������ �� ������������
    private GameObject indicatorInstance; // ������ �ε������� ����
    public GameObject indicatorPrefab; // �ε������� ������
    private ParticleSystem projectileParticle; // �߻�ü ��ƼŬ �ý���

    private ICreatureStatus monsterStatus;
    // BaseProjectile�� Initialize �޼��带 ������
    public override void Initialize(Vector3 startPos, Transform target, float speed, float damage,
        IProjectileMovement moveStrategy, IProjectileImpact impactEffect, GameObject hitEffect, float heightFactor)
    {
        base.Initialize(startPos, target, speed, damage, moveStrategy, impactEffect, hitEffect, heightFactor);

        // ��ƼŬ �ý��� ��������
        projectileParticle = GetComponentInChildren<ParticleSystem>();

        // �߻�ü ������ ���� ���� ����
        UpdateProjectileColor();
    }

    // �߰� �ʱ�ȭ �޼���
    public void SetExplosionParameters(float safeZoneRadius, float dangerRadiusMultiplier, float explosionDelay, bool isRingShaped, GameObject indicatorPrefab, ICreatureStatus status, float essenceAmount = 0)
    {
        this.safeZoneRadius = safeZoneRadius;
        this.dangerRadius = safeZoneRadius * dangerRadiusMultiplier;
        this.explosionDelay = explosionDelay;
        this.isRingShaped = isRingShaped;
        this.indicatorPrefab = indicatorPrefab;
        this.monsterStatus = status;
        this.essenceAmount = essenceAmount;
        // �߻�ü ������ ���� ���� ����
        UpdateProjectileColor();
    }

    // �߻�ü ���� ������Ʈ
    private void UpdateProjectileColor()
    {
        if (projectileParticle != null)
        {
            var main = projectileParticle.main;

            // isRingShaped�� false�� �� �Ķ���, true�� �� ������
            if (!isRingShaped)
            {
                main.startColor = Color.blue; // �Ķ� ����
                Debug.Log("�Ķ� ���� ���� ����");
            }
            else
            {
                main.startColor = Color.red; // ���� ����
                Debug.Log("���� ���� ���� ����");
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ground"))
        {
            OnImpact(other);
        }
    }

    // Impact �̺�Ʈ ó��
    protected override void OnImpact(Collider other)
    {
        // �̹� ���� ������� ����
        if (isGrounded) return;

        // ���鿡 ������ ó��
        if (other.CompareTag("Ground") || other.CompareTag("Terrain"))
        {
            Debug.Log($"���� ���� - ����: {(isRingShaped ? "����(����)" : "��ü(�Ķ�)")}");
            isGrounded = true;
            moveStrategy = null; // ������ ����

            // ���� �ణ �������� ��ġ ����
            transform.position = new Vector3(transform.position.x, 0.1f, transform.position.z);

            // �ε������� ���� (���� ���� ǥ��)
            CreateExplosionIndicator();
        }
    }

    protected override void Update()
    {
        // ���� ������ ������Ʈ
        base.Update();

        // ���� ����� �� ���� Ÿ�̸� ����
        if (isGrounded && !hasExploded)
        {
            timer += Time.deltaTime;

            // Ÿ�̸Ӱ� ���� �ð��� �����ϸ� ����
            if (timer >= explosionDelay)
            {
                Explode();
            }
        }
    }

    // �ε������� ���� (������ ���)
    private void CreateExplosionIndicator()
    {
        if (indicatorPrefab == null)
        {
            Debug.LogError("�ε������� �������� �������� �ʾҽ��ϴ�!");
            return;
        }

        // �ε������� ������ �ν��Ͻ�ȭ
        indicatorInstance = Instantiate(
            indicatorPrefab,
            new Vector3(transform.position.x, 0.9f, transform.position.z),
            Quaternion.Euler(90f, 0f, 0f)
        );

        // �ε������� ũ�� ����
        if (isRingShaped)
        {
            // ���� ���� - ���� ���� ũ��� ����
            indicatorInstance.transform.localScale = new Vector3(dangerRadius * 2f, dangerRadius * 2f, 1f);

            // ���� ��� �ε������� �Ӽ� ���� (�����տ� �°� ���� �ʿ�)
            Renderer renderer = indicatorInstance.GetComponentInChildren<Renderer>();
            if (renderer != null && renderer.material != null)
            {
                // ���� ��� ���� (���� �ݰ� ����)
                float innerRadiusRatio = safeZoneRadius / dangerRadius;

                // _InnerRadius �Ӽ��� �ִٸ� ����
                if (renderer.material.HasProperty("_InnerRadius"))
                {
                    renderer.material.SetFloat("_InnerRadius", innerRadiusRatio);
                }

                // _FillAmount �Ӽ��� �ִٸ� ����
                if (renderer.material.HasProperty("_FillAmount"))
                {
                    renderer.material.SetFloat("_FillAmount", 0f); // ������ 0
                    StartCoroutine(AnimateFill(renderer.material, explosionDelay));
                }
            }
        }
        else
        {
            // ��ü ���� ���� - ���� ���� ũ��� ����
            indicatorInstance.transform.localScale = new Vector3(safeZoneRadius * 2f, safeZoneRadius * 2f, 1f);

            // ���� �ε������� �Ӽ� ����
            Renderer renderer = indicatorInstance.GetComponentInChildren<Renderer>();
            if (renderer != null && renderer.material != null)
            {
                // _InnerRadius �Ӽ��� �ִٸ� 0���� ���� (���� �� ����)
                if (renderer.material.HasProperty("_InnerRadius"))
                {
                    renderer.material.SetFloat("_InnerRadius", 0f);
                }

                // _FillAmount �Ӽ��� �ִٸ� ����
                if (renderer.material.HasProperty("_FillAmount"))
                {
                    renderer.material.SetFloat("_FillAmount", 0f); // ������ 0
                    StartCoroutine(AnimateFill(renderer.material, explosionDelay));
                }
            }
        }
    }

    // Fill �ִϸ��̼�
    private IEnumerator AnimateFill(Material material, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float fillAmount = elapsed / duration;
            material.SetFloat("_FillAmount", fillAmount);

            elapsed += Time.deltaTime;
            yield return null;
        }

        material.SetFloat("_FillAmount", 1f); // ������ ä��
    }

    private void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;

        Debug.Log($"���� ���� - ����: {(isRingShaped ? "�� ����(������)" : "��ü ����(�Ķ���)")}");
        Debug.Log($"���� ���� �ݰ�: {safeZoneRadius}, ���� ���� �ݰ�: {dangerRadius}");

        // ���� ������ ���� �ٸ� ���� ����
        if (isRingShaped)
        {
            // ������ ����: ������ ����, �ٱ��� ����
            ApplyRingExplosionDamage();
        }
        else
        {
            // �Ķ��� ����: ���� �� ������
            ApplyFullExplosionDamage();
        }

        // ���� ����Ʈ ����
        if (hitEffect != null)
        {
            Instantiate(hitEffect, transform.position, Quaternion.identity);
        }

        // �ε������� ����
        if (indicatorInstance != null)
        {
            Destroy(indicatorInstance);
        }

        // ���� ������Ʈ ����
        Destroy(gameObject, 0.2f);
    }

    // �� ��� ���� ������ (���� ������ �ִ�)
    private void ApplyRingExplosionDamage()
    {
        Debug.Log($"�� ���� ������ ��� - ���� ����: {safeZoneRadius}, ���� ����: {dangerRadius}");

        // ���� ������ �ִ� ��� �ݶ��̴� �˻�
        Collider[] colliders = Physics.OverlapSphere(transform.position, dangerRadius);

        foreach (var collider in colliders)
        {
            if (collider.CompareTag("Player") && !damageApplied)
            {
                // �÷��̾� ��ġ �������� (�߽���)
                PlayerClass player = GameInitializer.Instance.GetPlayerClass();
                if (player != null)
                {
                    Vector3 playerPosition = player.playerTransform.position;
                    // �Ÿ� ���
                    float distance = Vector3.Distance(transform.position, playerPosition);

                    Debug.Log($"�÷��̾� �Ÿ�: {distance}, ���� ����: {safeZoneRadius}, ���� ����: {dangerRadius}");

                    // ���� �������� ������ ���� (���� ���� �� && ���� ���� ��)
                    if (distance > safeZoneRadius && distance <= dangerRadius)
                    {
                        if (monsterStatus.GetMonsterClass() is AlexanderBoss alexBoss)
                        {
                            
                            IBossEssenceSystem essenceSystem = alexBoss.GetEssenceSystem();
                            if (essenceSystem != null)
                            {
                                
                                essenceSystem.IncreaseEssence(essenceAmount);
                            }
                            player.TakeDamage((int)damage);
                            damageApplied = true; // ������ ���� ǥ��
                            Debug.Log($"���� ������ ����: {damage}");
                        }
                        else
                        {
                            Debug.Log("�÷��̾ ���� ���� �ۿ� �־� ������ ����");
                        }
                    }
                }
            }
        } 
    }

    // ��ü ���� ���� ������ (���� ���� ����)
    private void ApplyFullExplosionDamage()
    {
        Debug.Log($"��ü ���� ���� ������ - ����: {safeZoneRadius}");

        // ��ü ������ ������
        Collider[] colliders = Physics.OverlapSphere(transform.position, safeZoneRadius);

        foreach (var collider in colliders)
        {
            if (collider.CompareTag("Player") && !damageApplied)
            {
                PlayerClass player = GameInitializer.Instance.GetPlayerClass();
                if (player != null)
                {
                    if (monsterStatus.GetMonsterClass() is AlexanderBoss alexBoss)
                    {

                        IBossEssenceSystem essenceSystem = alexBoss.GetEssenceSystem();
                        if (essenceSystem != null)
                        {

                            essenceSystem.IncreaseEssence(essenceAmount);
                        }
                    }
                        player.TakeDamage((int)damage);
                    damageApplied = true;
                    Debug.Log($"��ü ���� ������ ����: {damage}");
                }
            }
        }
    }
 
    // ����׿� ����� �׸���
    private void OnDrawGizmos()
    {
        if (!isGrounded) return;

        if (isRingShaped)
        {
            // ���� ���� - ���
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, safeZoneRadius);

            // ���� ���� - ������
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, dangerRadius);
        }
        else
        {
            // �Ķ��� ���� ���� - �Ķ���
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, safeZoneRadius);
        }
    }
}