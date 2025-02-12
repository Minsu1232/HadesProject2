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
    private Transform spawnPoint;  // 스킬 발사 위치

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
        this.monsterStatus = status;
        this.target = target;
        this.skillDamage = status.GetMonsterClass().CurrentSkillDamage;
        this.spawnPoint = status.GetSkillSpawnPoint();

        
    }

    public void Execute()
    {
        GameObject projectile = GameObject.Instantiate(projectilePrefab,
            spawnPoint.position,
            spawnPoint.rotation);

        if (projectile.TryGetComponent<BaseProjectile>(out var skillProjectile))
        {
            skillProjectile.Initialize(spawnPoint.position, target,
                projectileSpeed, skillDamage, moveStrategy, impactEffect,hitEffect);
            skillProjectile.Launch();
        }
    }
    public void OnComplete()
    {
       
    }
}