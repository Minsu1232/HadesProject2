using UnityEngine;
using System;

public class CircularProjectileSkillEffect : ProjectileSkillEffect
{
    // ���� ��ġ ���� �Ķ����
    private int projectileCount;  // ������ ������Ÿ�� ��
    private float radius;        // ���� ������
    private float safeZoneRadius;  // ���� ���� �ݰ�
    private float dangerRadiusMultiplier; // ���� ���� ���
    private float explosionDelay;  // ���� ���� �ð�
    private float randomVariation; // ���� ���� ��ȭ��(��)
    private float essenceAmount;
    // �ε������� ����
    private GameObject indicatorPrefab; // �ε������� ������

    // �����ڴ� �θ� Ŭ������ ������ ȣ��
    public CircularProjectileSkillEffect(GameObject prefab, GameObject indicatorPrefab, float speed,
        IProjectileMovement moveStrategy, IProjectileImpact impactEffect,
        GameObject hitEffect, float heightFactor)
        : base(prefab, speed, moveStrategy, impactEffect, hitEffect, heightFactor)
    {
        this.indicatorPrefab = indicatorPrefab;
    }

    // �߰� ���� �޼���
    public void SetCircleParameters(int count, float radius, float safeZoneRadius,
                                   float dangerRadiusMultiplier, float explosionDelay, float essenceAmount = 0)
    {
        this.projectileCount = count;
        this.radius = radius;
        this.safeZoneRadius = safeZoneRadius;
        this.dangerRadiusMultiplier = dangerRadiusMultiplier;
        this.explosionDelay = explosionDelay;
        this.essenceAmount = essenceAmount;
    }

    // Initialize �޼��� �������̵�
    public override void Initialize(ICreatureStatus status, Transform target)
    {
        base.Initialize(status, target);

        // Essence �ý��۰� ���� (Alexander ������ ���)
        if (status.GetMonsterClass() is AlexanderBoss alexBoss)
        {
            IBossEssenceSystem essenceSystem = alexBoss.GetEssenceSystem();
            if (essenceSystem != null)
            {
                float essenceRatio = essenceSystem.CurrentEssence / essenceSystem.MaxEssence;

                // ���� ��ġ�� ���� �߻�ü ���� ���� (�ּ� projectileCount��, �ִ� projectileCount + 5��)
                int additionalProjectiles = Mathf.RoundToInt(5 * essenceRatio);
                projectileCount += additionalProjectiles;

                Debug.Log($"[CircularProjectileSkillEffect] Essence ��ġ: {essenceRatio:P0}, ���� ��: {projectileCount}��");

                // ���Ⱑ �ִ�ġ(100%)�� ����� ��� ���� ��� �ð��� �� ª�� ����
                if (essenceRatio > 0.9f)
                {
                    explosionDelay *= 0.7f; // 30% �� ���� ����
                    Debug.Log($"[CircularProjectileSkillEffect] ���� �ִ�ġ ����! ���� ��� �ð�: {explosionDelay}��");
                }
            }
        }
    }

    // Execute �޼��� �������̵�
    public override void Execute()
    {
        try
        {
            if (projectilePrefab == null || monsterStatus == null) return;

            Transform source = monsterStatus.GetMonsterTransform();
            Vector3 centerPos = source.position;

            // �������� ������Ÿ�� ����
            for (int i = 0; i < projectileCount; i++)
            {
                // �յ��ϰ� ������ ���� + ���� ��ȭ
                float angle = (360f / projectileCount) * i + UnityEngine.Random.Range(-randomVariation, randomVariation);
                // �ణ�� ���� �ݰ�
                float rad = UnityEngine.Random.Range(radius * 0.8f, radius * 1.2f);

                // ��ǥ ��ġ ��� (�ٴ�)
                Vector3 targetPos = centerPos + new Vector3(
                    Mathf.Cos(angle * Mathf.Deg2Rad) * rad,
                    0.1f,
                    Mathf.Sin(angle * Mathf.Deg2Rad) * rad
                );

                // �߻� ���� ��ġ (���� ��ġ���� �ణ ��)
                Vector3 spawnPos = centerPos + Vector3.up * 2f;

                // Alexander ������ ������ �ý��� ����
                bool isRingShaped = true; // �⺻��: ����(������)
                if (monsterStatus.GetMonsterClass() is AlexanderBoss alexBoss)
                {
                    IBossEssenceSystem essenceSystem = alexBoss.GetEssenceSystem();
                    if (essenceSystem != null)
                    {
                        float essenceRatio = essenceSystem.CurrentEssence / essenceSystem.MaxEssence;
                        // ���� ��ġ�� ���� �Ķ��� ���� Ȯ�� ���� (10% ~ 30%)
                        float blueClawChance = 0.1f + (0.2f * essenceRatio);
                        isRingShaped = (UnityEngine.Random.value > blueClawChance);
                    }
                    else
                    {
                        isRingShaped = (UnityEngine.Random.value <= 0.1f); // �⺻: 90% ������
                    }
                }
                else
                {
                    isRingShaped = (UnityEngine.Random.value <= 0.1f); // �⺻: 90% ������
                }

       

                // �ӽ� Ÿ�� ���� (������Ÿ���� ���� ����)
                GameObject tempTarget = new GameObject($"GroundTarget_{i}");
                tempTarget.transform.position = targetPos;
                Debug.Log(isRingShaped + "���� ���� ����");
                // �� �߻�ü���� ���ο� DelayedExplosionImpact �ν��Ͻ� ����
                DelayedExplosionImpact customImpact = new DelayedExplosionImpact(
                    safeZoneRadius,
                    dangerRadiusMultiplier,
                    explosionDelay,
                    isRingShaped,
                    hitEffect
                );

                // �߻�ü ����
                GameObject projectile = GameObject.Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
                InvokeEffectCompleted();
                if (projectile.TryGetComponent<GroundExplosionProjectile>(out var explosionProj))
                {
                    // �⺻ �ʱ�ȭ
                    explosionProj.Initialize(spawnPos, tempTarget.transform, projectileSpeed, skillDamage,
                                            moveStrategy, customImpact, hitEffect, heightFactor);

                    // �߰� �Ķ���� ���� (�ε������� ������ ����)
                    explosionProj.SetExplosionParameters(safeZoneRadius, dangerRadiusMultiplier,
                                                       explosionDelay, isRingShaped, indicatorPrefab, monsterStatus, essenceAmount);

                    // ���� ���� �������� ����
                    explosionProj.targetPosition = new Vector3(spawnPos.x, 0f, spawnPos.z);

                    // �߻�
                    explosionProj.Launch();
                }

                // �ӽ� Ÿ�� ���� (���� �ð� ��)
                GameObject.Destroy(tempTarget, explosionDelay + 2f);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"CircularProjectileSkillEffect.Execute ����: {e.Message}\n{e.StackTrace}");
        }
    }

    // ���� ������ �̸� �����ִ� �ε������� ����
   
}