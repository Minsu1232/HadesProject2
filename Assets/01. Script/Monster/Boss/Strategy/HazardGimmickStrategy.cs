using UnityEngine;
using DG.Tweening; // DOTween ���ӽ����̽� �߰�

public class HazardGimmickStrategy : IGimmickStrategy
{
    private readonly BossAI boss;             // ���� AI
    private readonly GimmickData data;          // ��� ������
    private readonly GameObject hazardPrefab;   // ������ ���� ������Ʈ ������
    private readonly IGimmickReward gimmickReward; // ���� �ý���
    private AudioClip roarSound;                // ��� �������� ����� ���� (������)
    private readonly ISuccessUI successUI;      // ���� UI ��� (ISuccessUI �������̽�)

    private float elapsedTime;  // ��� ���� �ð�
    private int hitCount;       // ���� Ƚ��
    private bool isComplete;    // ��� �Ϸ� ����
    private float nextSpawnTime;// ���� ������Ʈ ���� �ð�
    private bool isInProgress = false; // ��� ���� �� ����

    public bool IsGimmickComplete => isComplete;

    // ������: SuccessUI ����� �ܺο��� �����Ͽ� ���
    public HazardGimmickStrategy(BossAI boss, GimmickData data, GameObject prefab, ISuccessUI successUI, AudioClip roarSound = null)
    {
        this.boss = boss;
        this.data = data;
        this.hazardPrefab = prefab;
        this.gimmickReward = new HazardGimmickReward(boss.GetBossMonster());
        this.roarSound = roarSound;
        this.successUI = successUI;
    }

    public HazardGimmickStrategy() { }

    public AudioClip GetOptionalRoarSound()
    {
        return roarSound;
    }

    public void StartGimmick()
    {
        //// ī�޶� ����ũ ȿ�� ���� (��: 1�� ���� ��鸲)
        //HitStopManager.TriggerHitStop(1f, 1.5f);
        isInProgress = false;

        // HitStop ���� �� 1�� ������ �� ��� �ʱ�ȭ
        DOVirtual.DelayedCall(1f, () =>
        {
            elapsedTime = 0f;
            hitCount = 0;
            isComplete = false;
            nextSpawnTime = Time.time;
            isInProgress = true;

            // ��� ���� �� ���� ���� (�ɼ�)
            if (data.makeInvulnerable)
            {
                boss.GetBossMonster().SetInvulnerable(true);
            }

            // ��� ���� �� SuccessUI �ʱ�ȭ:
            // �ִ� ���� Ƚ��(data.successCount)�� �������� �������� ������ ����
            if (successUI != null)
            {
                successUI.InitializeSuccessUI(data.successCount);
                successUI.UpdateSuccessCount(0);
            }
        });
    }

    public void UpdateGimmick()
    {
        if (!isInProgress || isComplete)
            return;

        elapsedTime += Time.deltaTime;

        // ���� �ð��� ���� ��� (1.0 -> 0.0)
        float remainingTimeRatio = 1f - (elapsedTime / data.duration);

        // UI ������Ʈ
        if (successUI != null)
        {
            successUI.UpdateTimeBar(remainingTimeRatio);
        }

        // ��� ���� �ð� �ʰ� �� ���� ó��
        if (elapsedTime >= data.duration)
        {
            if (isInProgress)
            {
                FailGimmick();
            }
            return;
        }

        // ���� ���� üũ
        if (hitCount >= data.successCount)
        {
            SucceedGimmick();
            return;
        }

        // ���� �������� ���� ������Ʈ ����
        if (Time.time >= nextSpawnTime)
        {
            SpawnHazardObject();
            nextSpawnTime = Time.time + data.preparationTime;
        }
    }

    private void SpawnHazardObject()
    {
        Vector3 spawnPosition = GetHazardSpawnPosition();
        GameObject hazardGO = Object.Instantiate(hazardPrefab, spawnPosition, Quaternion.identity);
        HazardObject hazard = hazardGO.GetComponent<HazardObject>();

        hazard.Initialize(
            data.areaRadius,
            data.damage,
            data.moveSpeed,
            data.hazardSpawnType,
            data.targetType,
            GetSpawnHeight(),
            data.areaRadius,
            this
        );
        hazard.StartMove();
    }

    private float GetSpawnHeight()
    {
        switch (data.hazardSpawnType)
        {
            case HazardSpawnType.AbovePlayer:
                return GameInitializer.Instance.GetPlayerClass().playerTransform.transform.position.y + 20f;
            case HazardSpawnType.AboveBoss:
                return boss.transform.position.y + 5f;
            case HazardSpawnType.Random:
                return 5f;
            case HazardSpawnType.FixedPoints:
                return 4f;
            default:
                return 3f;
        }
    }

    private Vector3 GetHazardSpawnPosition()
    {
        Vector3 position;
        switch (data.hazardSpawnType)
        {
            case HazardSpawnType.AbovePlayer:
                var playerPos = GameInitializer.Instance.GetPlayerClass().playerTransform.transform.position;
                position = new Vector3(playerPos.x, GetSpawnHeight(), playerPos.z);
                break;
            case HazardSpawnType.Random:
                float randomX = Random.Range(-data.areaRadius, data.areaRadius);
                float randomZ = Random.Range(-data.areaRadius, data.areaRadius);
                position = new Vector3(randomX, GetSpawnHeight(), randomZ);
                break;
            case HazardSpawnType.FixedPoints:
                position = new Vector3(0, GetSpawnHeight(), 0);
                break;
            default:
                position = Vector3.zero;
                position.y = GetSpawnHeight();
                break;
        }
        return position;
    }

    private void SucceedGimmick()
    {
        PlayerClass player = GameInitializer.Instance.GetPlayerClass();
        isComplete = true;
        isInProgress = false;
        data.isEnabled = false;
        Debug.Log("��� ����");

        // ���� ���� ����
        gimmickReward.ApplySuccess(boss, player);

        // ��� ���� �� ���� ������ ����
        if (data.makeInvulnerable)
        {
            boss.GetBossMonster().SetInvulnerable(false);
            Debug.Log("���� ����");
        }
        successUI.UIOff();
    }

    private void FailGimmick()
    {
        isComplete = true;
        isInProgress = false;
        data.isEnabled = false;
        Debug.Log("��� ����");

        // ���� �г�Ƽ ����
        gimmickReward.ApplyFailure(boss, GameInitializer.Instance.GetPlayerClass());

        if (data.makeInvulnerable)
        {
            boss.GetBossMonster().SetInvulnerable(false);
            Debug.Log("���� ����");
        }
        successUI.UIOff();
    }

    // ���� �̺�Ʈ �߻� �� ȣ��: ��� �� ���� Ƚ���� ������Ű�� UI ������Ʈ
    public void SucessTrigget()
    {
        hitCount++;
        Debug.Log("���� Ƚ��: " + hitCount);
        if (successUI != null)
        {
            successUI.UpdateSuccessCount(hitCount);
        }
        if (hitCount >= data.successCount)
        {
            SucceedGimmick();
        }
    }

    public string GetGimmickAnimationTrigger()
    {
        return "Roar";
    }

    public IGimmickStrategy GetGimmickStrategy()
    {
        throw new System.NotImplementedException();
    }
}
