using System;
using System.Linq;
using UnityEngine;

public class ProjectileSkillEffect : ISkillEffect
{// 이벤트 구현
    public event Action OnEffectCompleted;
    // 상속 클래스에서 접근할 수 있도록 protected로 변경
    protected ICreatureStatus monsterStatus;
    protected Transform target;
    protected GameObject projectilePrefab;
    protected GameObject hitEffect;
    protected float projectileSpeed; // 계수가 계산된 속도값
    protected float defaultProjectileSpeed; // 데이터에서 받은 디폴트값
    protected float skillDamage;
    protected IProjectileMovement moveStrategy;
    protected IProjectileImpact impactEffect;
    protected Transform spawnPoint;  // 스킬 발사 위치
    protected float damageMultiplier = 1.0f; // 데미지 계수 기본값
    protected float speedMultiplier = 1.0f; // 스피드 계수 기본값
    protected float heightFactor; // 포물선 이동 높이

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

    // 기존 Initialize 메서드에 virtual 추가
    public virtual void Initialize(ICreatureStatus status, Transform target)
    {
        // 기본 데미지 계수 1.0으로 내부 메서드 호출
        InitializeInternal(status, target, this.damageMultiplier, this.speedMultiplier);
    }

    // 새로운 Initialize 메서드에 virtual 추가 (데미지 계수 추가)
    public virtual void Initialize(ICreatureStatus status, Transform target, float damageMultiplier, float speedMultiplier)
    {
        // 데미지 계수를 저장하고 내부 메서드 호출
        this.damageMultiplier = damageMultiplier;
        this.speedMultiplier = speedMultiplier;
        InitializeInternal(status, target, damageMultiplier, speedMultiplier);
    }

    // 내부 초기화 메서드를 protected로 변경하여 상속 클래스에서 접근 가능하게 함
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

            // 기본 스킬 데미지에 계수 적용
            float baseSkillDamage = status.GetMonsterClass().CurrentSkillDamage;
            this.skillDamage = Mathf.RoundToInt(baseSkillDamage * damageMultiplier); // 계산된 데미지
            this.projectileSpeed = Mathf.RoundToInt(defaultProjectileSpeed * speedMultiplier); // 계산된 속도
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

    // Execute 메서드에도 virtual 추가
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
                    projectileSpeed, skillDamage, actualMoveStrategy, impactEffect, hitEffect, heightFactor);
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
    // 발사체 생성 완료 후 이벤트 호출
    protected void InvokeEffectCompleted()
    {
        OnEffectCompleted?.Invoke();
    }
    // OnComplete 메서드에도 virtual 추가
    public virtual void OnComplete()
    {
        // 구현이 필요한 경우 여기에 추가
    }
}