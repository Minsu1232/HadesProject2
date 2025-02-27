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

    public ProjectileSkillEffect(GameObject prefab, float speed,
        IProjectileMovement moveStrategy, IProjectileImpact impactEffect, GameObject hitEffect)
    {
        this.projectilePrefab = prefab;
        this.projectileSpeed = speed;
        this.moveStrategy = moveStrategy;
        this.impactEffect = impactEffect;
        this.hitEffect = hitEffect;
       
    }

    public void Initialize(ICreatureStatus status, Transform target)
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
            this.skillDamage = status.GetMonsterClass().CurrentSkillDamage;
            this.spawnPoint = status.GetSkillSpawnPoint();

            // moveStrategy�� null�̸� �α׸� ����� ��� ����
            if (moveStrategy == null)
            {
                Debug.LogWarning("ProjectileSkillEffect.Initialize: moveStrategy is null, �⺻ ���� �̵����� �����˴ϴ�.");
                // �ʿ�� ���⼭ �⺻ �̵� �������� ������ �� ����
                moveStrategy = new StraightMovement();
            }
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