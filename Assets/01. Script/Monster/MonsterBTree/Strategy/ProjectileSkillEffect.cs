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
    private Transform spawnPoint;  // 스킬 발사 위치
    private float damageMultiplier = 1.0f; // 데미지 계수 기본값

    public ProjectileSkillEffect(GameObject prefab, float speed,
        IProjectileMovement moveStrategy, IProjectileImpact impactEffect, GameObject hitEffect)
    {
        this.projectilePrefab = prefab;
        this.projectileSpeed = speed;
        this.moveStrategy = moveStrategy;
        this.impactEffect = impactEffect;
        this.hitEffect = hitEffect;
       
    }

    // 기존 Initialize 메서드 유지 (호환성)
    public void Initialize(ICreatureStatus status, Transform target)
    {
        // 기본 데미지 계수 1.0으로 내부 메서드 호출
        InitializeInternal(status, target, this.damageMultiplier);
    }

    // 새로운 Initialize 메서드 (데미지 계수 추가)
    public void Initialize(ICreatureStatus status, Transform target, float damageMultiplier)
    {
        // 데미지 계수를 저장하고 내부 메서드 호출
        this.damageMultiplier = damageMultiplier;
        InitializeInternal(status, target, damageMultiplier);
    }

    // 실제 초기화 로직을 담당하는 내부 메서드
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

            // 기본 스킬 데미지에 계수 적용
            float baseSkillDamage = status.GetMonsterClass().CurrentSkillDamage;
            this.skillDamage = Mathf.RoundToInt(baseSkillDamage * damageMultiplier);

            this.spawnPoint = status.GetSkillSpawnPoint();

            // moveStrategy가 null이면 로그만 남기고 계속 진행
            if (moveStrategy == null)
            {
                Debug.LogWarning("ProjectileSkillEffect.Initialize: moveStrategy is null, 기본 직선 이동으로 설정됩니다.");
                // 필요시 여기서 기본 이동 전략으로 설정할 수 있음
                moveStrategy = new StraightMovement();
            }

            Debug.Log($"ProjectileSkillEffect 초기화 완료: 데미지={skillDamage} (기본값 x{damageMultiplier})");
        }
        catch (Exception e)
        {
            Debug.LogError($"ProjectileSkillEffect.Initialize 오류: {e.Message}\n{e.StackTrace}");
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
                // 폴백으로 생물체의 위치 사용
                if (monsterStatus != null)
                {
                    spawnPoint = monsterStatus.GetSkillSpawnPoint();
                    if (spawnPoint == null)
                    {
                        Debug.LogError("ProjectileSkillEffect.Execute: 폴백 spawnPoint도 null");
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
                // moveStrategy가 null이면 기본 직선 이동으로 설정
                IProjectileMovement actualMoveStrategy = moveStrategy ?? new StraightMovement();

                skillProjectile.Initialize(spawnPoint.position, target,
                    projectileSpeed, skillDamage, actualMoveStrategy, impactEffect, hitEffect);
                skillProjectile.Launch();
            }
            else
            {
                Debug.LogError($"ProjectileSkillEffect.Execute: 프리팹 {projectilePrefab.name}에 BaseProjectile 컴포넌트가 없습니다.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"ProjectileSkillEffect.Execute 오류: {e.Message}\n{e.StackTrace}");
        }
    }
    public void OnComplete()
    {
       
    }
}