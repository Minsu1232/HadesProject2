using System;
using System.Linq;
using UnityEngine;

public class ProjectileSkillEffect : ISkillEffect
{
    private ICreatureStatus monsterStatus;
    private Transform target;
    private GameObject projectilePrefab;
    private GameObject hitEffect;
    private float projectileSpeed;
    private float skillDamage;
    private IProjectileMovement moveStrategy;
    private IProjectileImpact impactEffect;
    private Transform spawnPoint;  // ��ų �߻� ��ġ
    private float damageMultiplier = 1.0f; // ������ ��� �⺻��

    public ProjectileSkillEffect(GameObject prefab, float speed,
        IProjectileMovement moveStrategy, IProjectileImpact impactEffect, GameObject hitEffect)
    {
        this.projectilePrefab = prefab;
        this.projectileSpeed = speed;
        this.moveStrategy = moveStrategy;
        this.impactEffect = impactEffect;
        this.hitEffect = hitEffect;
       
    }

    // ���� Initialize �޼��� ���� (ȣȯ��)
    public void Initialize(ICreatureStatus status, Transform target)
    {
        // �⺻ ������ ��� 1.0���� ���� �޼��� ȣ��
        InitializeInternal(status, target, this.damageMultiplier);
    }

    // ���ο� Initialize �޼��� (������ ��� �߰�)
    public void Initialize(ICreatureStatus status, Transform target, float damageMultiplier)
    {
        // ������ ����� �����ϰ� ���� �޼��� ȣ��
        this.damageMultiplier = damageMultiplier;
        InitializeInternal(status, target, damageMultiplier);
    }

    // ���� �ʱ�ȭ ������ ����ϴ� ���� �޼���
    private void InitializeInternal(ICreatureStatus status, Transform target, float damageMultiplier)
    {
        try
        {
            if (status == null)
            {
                Debug.LogError("ProjectileSkillEffect.Initialize: status is null");
                return;
            }

            this.monsterStatus = status;
            this.target = target;

            // �⺻ ��ų �������� ��� ����
            float baseSkillDamage = status.GetMonsterClass().CurrentSkillDamage;
            this.skillDamage = Mathf.RoundToInt(baseSkillDamage * damageMultiplier);

            this.spawnPoint = status.GetSkillSpawnPoint();

            // moveStrategy�� null�̸� �α׸� ����� ��� ����
            if (moveStrategy == null)
            {
                Debug.LogWarning("ProjectileSkillEffect.Initialize: moveStrategy is null, �⺻ ���� �̵����� �����˴ϴ�.");
                // �ʿ�� ���⼭ �⺻ �̵� �������� ������ �� ����
                moveStrategy = new StraightMovement();
            }

            Debug.Log($"ProjectileSkillEffect �ʱ�ȭ �Ϸ�: ������={skillDamage} (�⺻�� x{damageMultiplier})");
        }
        catch (Exception e)
        {
            Debug.LogError($"ProjectileSkillEffect.Initialize ����: {e.Message}\n{e.StackTrace}");
        }
    }






public void Execute()
    {
        try
        {
            if (projectilePrefab == null)
            {
                Debug.LogError("ProjectileSkillEffect.Execute: projectilePrefab is null");
                return;
            }

            if (spawnPoint == null)
            {
                Debug.LogError("ProjectileSkillEffect.Execute: spawnPoint is null");
                // �������� ����ü�� ��ġ ���
                if (monsterStatus != null)
                {
                    spawnPoint = monsterStatus.GetSkillSpawnPoint();
                    if (spawnPoint == null)
                    {
                        Debug.LogError("ProjectileSkillEffect.Execute: ���� spawnPoint�� null");
                        return;
                    }
                }
                else
                {
                    return;
                }
            }

            GameObject projectile = GameObject.Instantiate(projectilePrefab,
                spawnPoint.position,
                spawnPoint.rotation);

            if (projectile.TryGetComponent<BaseProjectile>(out var skillProjectile))
            {
                // moveStrategy�� null�̸� �⺻ ���� �̵����� ����
                IProjectileMovement actualMoveStrategy = moveStrategy ?? new StraightMovement();

                skillProjectile.Initialize(spawnPoint.position, target,
                    projectileSpeed, skillDamage, actualMoveStrategy, impactEffect, hitEffect);
                skillProjectile.Launch();
            }
            else
            {
                Debug.LogError($"ProjectileSkillEffect.Execute: ������ {projectilePrefab.name}�� BaseProjectile ������Ʈ�� �����ϴ�.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"ProjectileSkillEffect.Execute ����: {e.Message}\n{e.StackTrace}");
        }
    }
    public void OnComplete()
    {
       
    }
}