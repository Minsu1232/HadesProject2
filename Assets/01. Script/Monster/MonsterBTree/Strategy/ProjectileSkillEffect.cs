using System.Linq;
using UnityEngine;

public class ProjectileSkillEffect : ISkillEffect
{
    private MonsterStatus monsterStatus;
    private Transform target;
    private GameObject projectilePrefab;
    private float projectileSpeed;
    private float skillDamage;
    private IProjectileMovement moveStrategy;
    private Transform spawnPoint;  // 스킬 발사 위치

    public ProjectileSkillEffect(GameObject prefab, float speed, IProjectileMovement moveStrategy)
    {
        this.projectilePrefab = prefab;
        this.projectileSpeed = speed;
        this.moveStrategy = moveStrategy;
    }

    public void Initialize(MonsterStatus status, Transform target)
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

        if (projectile.TryGetComponent<SkillProjectile>(out var skillProjectile))
        {
            skillProjectile.Initialize(spawnPoint.position, target,
                projectileSpeed, skillDamage, moveStrategy);
            skillProjectile.Launch();
        }
    }
    public void OnComplete()
    {
       
    }
}