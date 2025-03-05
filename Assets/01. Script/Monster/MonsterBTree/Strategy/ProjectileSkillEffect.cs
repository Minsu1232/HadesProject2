using System;
using System.Linq;
using UnityEngine;

public class ProjectileSkillEffect : ISkillEffect
{// �̺�Ʈ ����
    public event Action OnEffectCompleted;
    // ��� Ŭ�������� ������ �� �ֵ��� protected�� ����
    protected ICreatureStatus monsterStatus;
    protected Transform target;
    protected GameObject projectilePrefab;
    protected GameObject hitEffect;
    protected float projectileSpeed; // ����� ���� �ӵ���
    protected float defaultProjectileSpeed; // �����Ϳ��� ���� ����Ʈ��
    protected float skillDamage;
    protected IProjectileMovement moveStrategy;
    protected IProjectileImpact impactEffect;
    protected Transform spawnPoint;  // ��ų �߻� ��ġ
    protected float damageMultiplier = 1.0f; // ������ ��� �⺻��
    protected float speedMultiplier = 1.0f; // ���ǵ� ��� �⺻��
    protected float heightFactor; // ������ �̵� ����

    public ProjectileSkillEffect(GameObject prefab, float speed,
        IProjectileMovement moveStrategy, IProjectileImpact impactEffect, GameObject hitEffect, float heightFactor)
    {
        this.projectilePrefab = prefab;
        this.defaultProjectileSpeed = speed;
        this.moveStrategy = moveStrategy;
        this.impactEffect = impactEffect;
        this.hitEffect = hitEffect;
        this.heightFactor = heightFactor;
    }

    // ���� Initialize �޼��忡 virtual �߰�
    public virtual void Initialize(ICreatureStatus status, Transform target)
    {
        // �⺻ ������ ��� 1.0���� ���� �޼��� ȣ��
        InitializeInternal(status, target, this.damageMultiplier, this.speedMultiplier);
    }

    // ���ο� Initialize �޼��忡 virtual �߰� (������ ��� �߰�)
    public virtual void Initialize(ICreatureStatus status, Transform target, float damageMultiplier, float speedMultiplier)
    {
        // ������ ����� �����ϰ� ���� �޼��� ȣ��
        this.damageMultiplier = damageMultiplier;
        this.speedMultiplier = speedMultiplier;
        InitializeInternal(status, target, damageMultiplier, speedMultiplier);
    }

    // ���� �ʱ�ȭ �޼��带 protected�� �����Ͽ� ��� Ŭ�������� ���� �����ϰ� ��
    protected virtual void InitializeInternal(ICreatureStatus status, Transform target, float damageMultiplier, float speedMultiplier)
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
            this.skillDamage = Mathf.RoundToInt(baseSkillDamage * damageMultiplier); // ���� ������
            this.projectileSpeed = Mathf.RoundToInt(defaultProjectileSpeed * speedMultiplier); // ���� �ӵ�
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

    // Execute �޼��忡�� virtual �߰�
    public virtual void Execute()
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
                    projectileSpeed, skillDamage, actualMoveStrategy, impactEffect, hitEffect, heightFactor);
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
    // �߻�ü ���� �Ϸ� �� �̺�Ʈ ȣ��
    protected void InvokeEffectCompleted()
    {
        OnEffectCompleted?.Invoke();
    }
    // OnComplete �޼��忡�� virtual �߰�
    public virtual void OnComplete()
    {
        // ������ �ʿ��� ��� ���⿡ �߰�
    }
}