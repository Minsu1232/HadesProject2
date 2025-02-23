using static IMonsterState;
using UnityEngine;

public class ChargeAttackStrategy : BasePhysicalAttackStrategy
{
    public override PhysicalAttackType AttackType => PhysicalAttackType.Charge;

    private enum ChargeState
    {
        None,
        Preparing,    // 발 구르기 + 인디케이터
        Charging,     // 실제 돌진
        End
    }

    private ChargeState currentChargeState = ChargeState.None;
    private Vector3 chargeDirection;
    private float chargeSpeed;
    private float chargeDuration;  // 최대 지속시간
    private float currentChargeTime;  // 현재 지속된 시간
    private float prepareTime = 1.5f;  // 준비 시간
    private float currentPrepareTime = 0f;
    private GameObject chargeIndicator;  // 인디케이터 프리팹
    private CreatureAI owner;

    public ChargeAttackStrategy(CreatureAI owner, ICreatureData data)
    {
        this.owner = owner;
        chargeSpeed = data.chargeSpeed;
        chargeDuration = data.chargeDuration;
    }

    public override void Attack(Transform transform, Transform target, IMonsterClass monsterData)
    {
        if (!CanAttack(Vector3.Distance(transform.position, target.position), monsterData))
            return;

        StartAttack();
        FaceTarget(transform, target);

        // 준비 상태 시작
        currentChargeState = ChargeState.Preparing;
        currentPrepareTime = 0f;

        // 인디케이터 생성
        if (chargeIndicator == null)
        {
            chargeIndicator = GameObject.Instantiate(chargeIndicator, transform.position, Quaternion.identity);
        }
    }

    public void UpdateCharge(Transform transform)
    {
        switch (currentChargeState)
        {
            case ChargeState.Preparing:
                UpdatePrepare(transform);
                break;
            case ChargeState.Charging:
                UpdateCharging(transform);
                break;
        }
    }

    private void UpdatePrepare(Transform transform)
    {
        currentPrepareTime += Time.deltaTime;

        // 인디케이터 업데이트
        if (chargeIndicator != null)
        {
            // 플레이어 방향으로 인디케이터 업데이트
            Transform playerTransform = GameInitializer.Instance.GetPlayerClass().playerTransform;
            chargeDirection = (playerTransform.position - transform.position).normalized;
            chargeIndicator.transform.position = transform.position;

            // Fill 값 업데이트 (0 -> 1)
            float fillAmount = currentPrepareTime / prepareTime;

            //chargeIndicator.GetComponent<IndicatorController>().UpdateFill(fillAmount);
        }

        // 준비 시간 완료
        if (currentPrepareTime >= prepareTime)
        {
            currentChargeState = ChargeState.Charging;
            if (chargeIndicator != null)
            {
                GameObject.Destroy(chargeIndicator);
            }
        }
    }

    private void UpdateCharging(Transform transform)
    {
        currentChargeTime += Time.deltaTime;

        // 시간 초과 체크
        if (currentChargeTime >= chargeDuration)
        {
            Debug.Log("Charge Time Over");
            StopAttack();
            return;
        }

        // 벽 체크
        if (Physics.Raycast(transform.position, chargeDirection, out RaycastHit hit, 1f))
        {
            if (hit.collider.CompareTag("CrashWall"))
            {
                owner.ChangeState(MonsterStateType.Groggy);
                StopAttack();
                Debug.Log("wall");
                return;
            }
        }

        // 플레이어 충돌 체크
        Collider[] hits = Physics.OverlapSphere(transform.position, 1f);
        foreach (var hitCollider in hits)
        {
            if (hitCollider.CompareTag("Player"))
            {
                Debug.Log("Player");
                IDamageable player = GameInitializer.Instance.GetPlayerClass();
                ApplyDamage(player, owner.GetStatus().GetMonsterClass());
                StopAttack();
                return;
            }
        }

        // 돌진 이동
        transform.position += chargeDirection * chargeSpeed * Time.deltaTime;
    }

    public override void StopAttack()
    {
        base.StopAttack();
        currentChargeTime = 0f;
        currentPrepareTime = 0f;
        currentChargeState = ChargeState.None;

        if (chargeIndicator != null)
        {
            GameObject.Destroy(chargeIndicator);
        }
    }
}