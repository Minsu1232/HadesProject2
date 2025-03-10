using UnityEngine;

public class MadnessCrackHazard : IBossEssenceHazard
{
    // �Ӽ�
    public string HazardName => "������ �տ�";
    public float ActivationThreshold { get; private set; }
    public float DamageMultiplier { get; private set; }

    // ������ �� ����
    private GameObject crackPrefab;
    private GameObject indicatorPrefab;
    private GameObject explosionPrefab;
    private float warningDuration;
    private float explosionRadius;
    private float explosionDamage;
    private IBossEssenceSystem essenceSystem;
    private ICreatureStatus monster;
    private float essenceAmount = 10f;  // ���� �� ������ ��������

    // ����Ʈ �� ����
    private Color crackColor = Color.red;

    public MadnessCrackHazard(
        GameObject prefab,
        GameObject indicatorPrefab,
        GameObject explosionPrefab,
        ICreatureStatus monster,
        float threshold = 70f,
        float warningTime = 1.5f,
        float radius = 3f,
        float damage = 20f,
        float dmgMultiplier = 1.2f)
    {
        crackPrefab = prefab;
        this.indicatorPrefab = indicatorPrefab;
        this.explosionPrefab = explosionPrefab;
        this.monster = monster;
        ActivationThreshold = threshold;
        warningDuration = warningTime;
        explosionRadius = radius;
        explosionDamage = damage;
        DamageMultiplier = dmgMultiplier;

        Debug.Log(monster.GetMonsterClass().ToString());
    }

    public void Initialize(IBossEssenceSystem essenceSystem)
    {
        this.essenceSystem = essenceSystem;
        Debug.Log($"������ �տ� ������ �ʱ�ȭ �Ϸ� (�Ӱ谪: {ActivationThreshold})");
    }

    public void ActivateHazard(Vector3 position, float intensity)
    {
        // ������ ��� ���ο� ���� �տ� ����
        if (crackPrefab == null)
        {
            // �������� ���� ��쿡�� ���� DelayedExplosionImpact ���
            DelayedExplosionImpact impactEffect = new DelayedExplosionImpact(
                explosionRadius * 0.3f,  // ���� ���� �ݰ�
                intensity,                // ���� ���� ���
                warningDuration,          // ���� ���� �ð�
                false,                    // �� ���� ����
                null                      // ���� ����Ʈ
            );

            impactEffect.OnImpact(position, explosionDamage * intensity * DamageMultiplier);
        }
        else
        {
            // ������ ��� ȿ��
            CreatePrefabCrackEffect(position, intensity);
        }

        Debug.Log($"������ �տ� ������ - ��ġ: {position}, ����: {intensity:F2}");
    }

    private void CreatePrefabCrackEffect(Vector3 position, float intensity)
    {
        // �ӽ� Ÿ�� ����
        GameObject tempTarget = new GameObject("CrackTarget");
        tempTarget.transform.position = position;

        // DelayedExplosionImpact ����
        DelayedExplosionImpact impactEffect = new DelayedExplosionImpact(
            explosionRadius * 0.3f,  // ���� ���� �ݰ�
            intensity,               // ���� ���� ���
            warningDuration,         // ���� ���� �ð�
            false,                   // �� ���� ����
            explosionPrefab          // ���� ����Ʈ
        );

        // GroundExplosionProjectile ���� �õ�
        if (crackPrefab != null)
        {
            GameObject projectile = GameObject.Instantiate(crackPrefab, position + Vector3.up * 0.5f, Quaternion.identity);

            if (projectile.TryGetComponent<GroundExplosionProjectile>(out var explosionProj))
            {
                // ������Ÿ�� �ʱ�ȭ
                explosionProj.Initialize(
                    position + Vector3.up * 2f, // ���� ��ġ
                    tempTarget.transform,       // ��ǥ ��ġ
                    5f,                         // �ӵ�
                    explosionDamage * intensity * DamageMultiplier, // ������
                    new StraightMovement(),                       // �̵� ����
                    impactEffect,               // ����Ʈ ȿ��
                    explosionPrefab,                       // ��Ʈ ����Ʈ
                    0.5f                        // ���� ���
                );

                // �߰� �Ķ���� ����
                explosionProj.SetExplosionParameters(
                    explosionRadius * 1f,  // ���� ����
                    intensity,               // ���� ���
                    warningDuration,         // ���� �ð�
                    false,                   // �� ����
                    indicatorPrefab,         // �ε�������
                    monster,                    // ���� ����
                    essenceAmount            // ������ ������
                );

                // �߻�
                explosionProj.Launch();
            }
            else
            {
                // ������Ÿ�� ������Ʈ�� ������ ���� ����Ʈ ȿ�� ����
                impactEffect.OnImpact(position, explosionDamage * intensity * DamageMultiplier);
                GameObject.Destroy(projectile);
            }
        }
        else
        {
            // �������� ������ ���� ����Ʈ ȿ�� ����
            impactEffect.OnImpact(position, explosionDamage * intensity * DamageMultiplier);
        }

        // �ӽ� Ÿ�� ����
        GameObject.Destroy(tempTarget, 5f);
    }

    public void DeactivateHazard()
    {
        // ó�� ���� ��� ����Ʈ ȿ�� ã��
        var explosionObjects = GameObject.FindGameObjectsWithTag("Explosion");
        foreach (var obj in explosionObjects)
        {
            GameObject.Destroy(obj);
        }

        Debug.Log("��� ������ �տ� ȿ�� ���ŵ�");
    }

    public void UpdateHazardIntensity(float essenceValue)
    {
        // ������ ��ġ�� ���� �տ� Ư�� ����
        float intensityFactor = Mathf.Clamp01((essenceValue - ActivationThreshold) /
                               (100f - ActivationThreshold));

        // ������ �� �ݰ� ��ȭ
        DamageMultiplier = Mathf.Lerp(1.0f, 1.5f, intensityFactor);
        explosionRadius = Mathf.Lerp(3f, 5f, intensityFactor);

        // ������ ��ġ�� �ſ� ���� ��� ��� �ð� ����
        if (essenceValue > 90f)
        {
            warningDuration = Mathf.Lerp(1.5f, 0.8f, (essenceValue - 90f) / 10f);
        }
    }
}