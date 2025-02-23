using static IMonsterState;
using UnityEngine;

public class ChargeAttackStrategy : BasePhysicalAttackStrategy
{
    public override PhysicalAttackType AttackType => PhysicalAttackType.Charge;

    private enum ChargeState
    {
        None,
        Preparing,    // �� ������ + �ε�������
        Charging,     // ���� ����
        End
    }

    private ChargeState currentChargeState = ChargeState.None;
    private Vector3 chargeDirection;
    private float chargeSpeed;
    private float chargeDuration;  // �ִ� ���ӽð�
    private float currentChargeTime;  // ���� ���ӵ� �ð�
    private float prepareTime = 1.5f;  // �غ� �ð�
    private float currentPrepareTime = 0f;
    private GameObject chargeIndicator;  // �ε������� ������
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

        // �غ� ���� ����
        currentChargeState = ChargeState.Preparing;
        currentPrepareTime = 0f;

        // �ε������� ����
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

        // �ε������� ������Ʈ
        if (chargeIndicator != null)
        {
            // �÷��̾� �������� �ε������� ������Ʈ
            Transform playerTransform = GameInitializer.Instance.GetPlayerClass().playerTransform;
            chargeDirection = (playerTransform.position - transform.position).normalized;
            chargeIndicator.transform.position = transform.position;

            // Fill �� ������Ʈ (0 -> 1)
            float fillAmount = currentPrepareTime / prepareTime;

            //chargeIndicator.GetComponent<IndicatorController>().UpdateFill(fillAmount);
        }

        // �غ� �ð� �Ϸ�
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

        // �ð� �ʰ� üũ
        if (currentChargeTime >= chargeDuration)
        {
            Debug.Log("Charge Time Over");
            StopAttack();
            return;
        }

        // �� üũ
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

        // �÷��̾� �浹 üũ
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

        // ���� �̵�
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