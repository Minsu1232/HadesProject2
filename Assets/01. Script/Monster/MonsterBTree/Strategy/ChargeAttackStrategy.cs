using System;
using UnityEngine;
using static IMonsterState;

public class ChargeAttackStrategy : BasePhysicalAttackStrategy
{
    public override PhysicalAttackType AttackType => PhysicalAttackType.Charge;
    public event System.Action OnChargeStateChanged;

    private GameObject prepareDustEffectPrefab;
    private GameObject startEffectPrefab;
    private GameObject trailEffectPrefab;
    private GameObject wallImpactEffectPrefab;
    private GameObject playerImpactEffectPrefab;

    private GameObject prepareDustEffect;
    private GameObject trailEffect;

    private float targetChargeDistance; // 목표 이동 거리
    private float currentChargeDistance; // 현재까지 이동한 거리
    private enum ChargeState
    {
        None,
        Preparing,
        Charging,
        End
    }

    private ChargeState currentChargeState = ChargeState.None;
    private Vector3 chargeDirection;
    private float chargeSpeed;
    private float chargeDuration;
    private float currentChargeTime;
    private float prepareTime;
    private float currentPrepareTime = 0f;
    private GameObject chargeIndicator;
    private GameObject indicatorPrefab;
    private CreatureAI owner;
    private Animator animator;

    public ChargeAttackStrategy(CreatureAI owner, ICreatureData data, Animator animator)
    {
        this.owner = owner;
        chargeSpeed = data.chargeSpeed;
        chargeDuration = data.chargeDuration;
        prepareTime = data.prepareTime;
        indicatorPrefab = data.chargeIndicatorPrefab;
        this.animator = animator;

        // 이펙트 프리팹 참조
        prepareDustEffectPrefab = data.ChargePrepareDustEffect;
        startEffectPrefab = data.ChargeStartEffect;
        trailEffectPrefab = data.ChargeTrailEffect;
        wallImpactEffectPrefab = data.WallImpactEffect;
        playerImpactEffectPrefab = data.PlayerImpactEffect;
    }

    public override void Attack(Transform transform, Transform target, IMonsterClass monsterData)
    {
        if (!CanAttack(Vector3.Distance(transform.position, target.position), monsterData))
            return;

        StartAttack();
        FaceTarget(transform, target);

        Vector3 directionXZ = target.position - transform.position;
        directionXZ.y = 0;
        chargeDirection = directionXZ.normalized;

        currentChargeState = ChargeState.Preparing;
        currentPrepareTime = 0f;

        // 인디케이터 생성
        if (chargeIndicator == null && indicatorPrefab != null)
        {
            chargeIndicator = GameObject.Instantiate(indicatorPrefab, transform.position, Quaternion.identity);
            var renderer = chargeIndicator.GetComponentInChildren<Renderer>();
            if (renderer != null)
            {
                renderer.material.SetFloat("_FillAmount", 0f);
            }
        }

        // 준비 단계 먼지 이펙트 생성
        if (prepareDustEffectPrefab != null)
        {
            prepareDustEffect = GameObject.Instantiate(
                prepareDustEffectPrefab,
                transform.position,
                Quaternion.identity
            );
            prepareDustEffect.transform.parent = transform;

         
        }
    }

    public void UpdateCharge(Transform transform)
    {
        switch (currentChargeState)
        {
            case ChargeState.Preparing:
                HandlePreparingState(transform);
                break;
            case ChargeState.Charging:
                HandleChargingState(transform);
                break;
            case ChargeState.End:
                HandleChargingStateEnd();
                break;
        }
    }

    private void HandleChargingStateEnd()
    {
        // 필요한 경우 종료 로직 추가
    }

    private void HandlePreparingState(Transform transform)
    {
        currentPrepareTime += Time.deltaTime;

        if (chargeIndicator != null)
        {
            Transform playerTransform = GameInitializer.Instance.GetPlayerClass().playerTransform;

            Vector3 directionXZ = playerTransform.position - transform.position;
            directionXZ.y = 0;
            chargeDirection = directionXZ.normalized;

            // 보스가 플레이어를 바라보도록 회전
            transform.rotation = Quaternion.Lerp(transform.rotation,
                                               Quaternion.LookRotation(chargeDirection),
                                               Time.deltaTime * 5f);

            float distanceToPlayer = directionXZ.magnitude;

            Vector3 indicatorPosition = transform.position;
            indicatorPosition.y += 0.5f;

            // 피벗을 시작점으로 이동
            indicatorPosition += chargeDirection * (distanceToPlayer * 0.5f);

            chargeIndicator.transform.position = indicatorPosition;

            float angle = Mathf.Atan2(chargeDirection.x, chargeDirection.z) * Mathf.Rad2Deg;
            chargeIndicator.transform.rotation = Quaternion.Euler(90f, angle, 0f);

            // 더 넓은 스케일로 조정 (너비를 2배로)
            chargeIndicator.transform.localScale = new Vector3(2f, distanceToPlayer, 1f);

            var renderer = chargeIndicator.GetComponentInChildren<Renderer>();
            if (renderer != null)
            {
                // 기본 설정
                renderer.material.SetFloat("_FillAmount", currentPrepareTime / prepareTime);
            }
        }

        if (currentPrepareTime >= prepareTime)
        {
            // 차징 시작 이펙트
            if (startEffectPrefab != null)
            {
                GameObject startEffect = GameObject.Instantiate(
                    startEffectPrefab,
                    transform.position,
                    Quaternion.LookRotation(chargeDirection)
                );

                // 파티클 시스템 수명 관리
                ParticleSystem ps = startEffect.GetComponent<ParticleSystem>();
                if (ps != null)
                {
                    float duration = ps.main.duration;
                    if (ps.main.loop)
                    {
                        duration = 2f; // 루프 모드일 경우 기본 2초
                    }
                    else if (duration == 0)
                    {
                        duration = ps.main.startLifetimeMultiplier;
                    }

                    GameObject.Destroy(startEffect, duration + 0.5f); // 여유 시간 추가
                }
                else
                {
                    GameObject.Destroy(startEffect, 2f);
                }
            }

            // 트레일 이펙트 생성
            if (trailEffectPrefab != null && trailEffect == null)
            {
                trailEffect = GameObject.Instantiate(
                    trailEffectPrefab,
                    transform.position,
                    Quaternion.identity
                );
                trailEffect.transform.parent = transform;
            }

            // 준비 이펙트 정리
            if (prepareDustEffect != null)
            {
                GameObject.Destroy(prepareDustEffect);
                prepareDustEffect = null;
            }

            // 플레이어 방향을 마지막으로 한 번 더 업데이트
            Transform playerTransform = GameInitializer.Instance.GetPlayerClass().playerTransform;
            Vector3 directionXZ = playerTransform.position - transform.position;
            directionXZ.y = 0;
            chargeDirection = directionXZ.normalized;

            // 보스가 플레이어를 정확히 바라보도록 회전
            transform.rotation = Quaternion.LookRotation(chargeDirection);
            currentChargeState = ChargeState.Charging;
            Debug.Log("똘징");
            currentChargeTime = 0f;
            float distanceToPlayer = directionXZ.magnitude;
            targetChargeDistance = distanceToPlayer + 2f; // 예: 플레이어 거리 + 2미터
            currentChargeDistance = 0f; // 이동 거리 초기화
            OnChargeStateChanged?.Invoke();  // 차지 상태로 변경 시 이벤트 발생

            if (chargeIndicator != null)
            {
                GameObject.Destroy(chargeIndicator);
                chargeIndicator = null;
            }
        }
    }

    private void HandleChargingState(Transform transform)
    {
        currentChargeTime += Time.deltaTime;

        // 목표 거리 도달 체크 추가
        if (currentChargeDistance >= targetChargeDistance)
        {
            StopAttack();
            return;
        }

        // 벽 체크
        if (Physics.Raycast(transform.position, chargeDirection, out RaycastHit hit, 1f))
        {
            if (hit.collider.CompareTag("CrashWall"))
            {
                Debug.Log("벽");
                // 벽 충돌 이펙트 생성
                if (wallImpactEffectPrefab != null)
                {
                    GameObject wallEffect = GameObject.Instantiate(
                        wallImpactEffectPrefab,
                        hit.point,
                        Quaternion.LookRotation(hit.normal)
                    );

                    // 파티클 시스템 수명 관리
                    ParticleSystem ps = wallEffect.GetComponent<ParticleSystem>();
                    if (ps != null)
                    {
                        float duration = ps.main.duration;
                        if (ps.main.loop)
                        {
                            duration = 2f; // 루프 모드일 경우 기본 2초
                        }
                        else if (duration == 0)
                        {
                            duration = ps.main.startLifetimeMultiplier;
                        }

                        GameObject.Destroy(wallEffect, duration + 0.5f);
                    }
                    else
                    {
                        GameObject.Destroy(wallEffect, 2f);
                    }
                }

                // 보스를 약간 뒤로 밀기
                transform.position -= chargeDirection * 3f;
                CameraShakeManager.TriggerShake(1f, 0.2f);
                StopAttack();
                owner.ChangeState(MonsterStateType.Groggy);
                
                return;
            }
        }

        // 플레이어 충돌 체크
        Collider[] hits = Physics.OverlapSphere(transform.position, 1f);
        foreach (var hitCollider in hits)
        {
            if (hitCollider.CompareTag("Player"))
            {
                // 플레이어 충돌 이펙트 생성
                if (playerImpactEffectPrefab != null)
                {
                    GameObject playerEffect = GameObject.Instantiate(
                        playerImpactEffectPrefab,
                        hitCollider.transform.position,
                        Quaternion.LookRotation(-chargeDirection)
                    );

                    // 파티클 시스템 수명 관리
                    ParticleSystem ps = playerEffect.GetComponent<ParticleSystem>();
                    if (ps != null)
                    {
                        float duration = ps.main.duration;
                        if (ps.main.loop)
                        {
                            duration = 2f; // 루프 모드일 경우 기본 2초
                        }
                        else if (duration == 0)
                        {
                            duration = ps.main.startLifetimeMultiplier;
                        }

                        GameObject.Destroy(playerEffect, duration + 0.5f);
                    }
                    else
                    {
                        GameObject.Destroy(playerEffect, 2f);
                    }
                }

                IDamageable player = GameInitializer.Instance.GetPlayerClass();
                ApplyDamage(player, owner.GetStatus().GetMonsterClass());
                StopAttack();
                return;
            }
        }
    
        // 돌진 이동
        float moveDistance = chargeSpeed * Time.deltaTime;
        transform.position += chargeDirection * moveDistance;
        currentChargeDistance += moveDistance; // 이동 거리 누적
    }

    public override string GetAnimationTriggerName()
    {
        return currentChargeState == ChargeState.Preparing ? "ChargePrepare" : "Attack_Charge";
    }

    public override void StopAttack()
    {
        base.StopAttack();
        animator.SetTrigger("ChargingComplete");
        currentChargeTime = 0f;
        currentPrepareTime = 0f;
        currentChargeState = ChargeState.None;

        // 모든 이펙트 정리
        if (chargeIndicator != null)
        {
            GameObject.Destroy(chargeIndicator);
            chargeIndicator = null;
        }

        if (prepareDustEffect != null)
        {
            GameObject.Destroy(prepareDustEffect);
            prepareDustEffect = null;
        }

        if (trailEffect != null)
        {
            GameObject.Destroy(trailEffect);
            trailEffect = null;
        }
    }
}