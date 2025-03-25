using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using System.Collections;
using DG.Tweening;

public class DungeonManager : Singleton<DungeonManager>
{
    [Header("�⺻ ����")]
    [SerializeField] private bool usePortalSystem = true;

    [Header("���� ���� ����")]
    [SerializeField] private float spawnDelay = 0.3f;
    [SerializeField] private GameObject spawnEffectPrefab;
    [SerializeField] private float bossIntroDelay = 2f;

    [Header("��Ż ����")]
    [SerializeField] private GameObject portalPrefab;
    [SerializeField] private float portalSpawnDelay = 2f;
    [SerializeField] private Vector3 portalSpawnOffset = new Vector3(0, 0.1f, 0);
    [SerializeField] private GameObject portalAppearEffectPrefab;

    [Header("��ȯ ȿ��")]
    [SerializeField] private float transitionDelay = 0.5f;
    [SerializeField] private Color bossRoomColor = new Color(0.5f, 0, 0, 0.3f);
    public event System.Action<StageData> OnStageLoaded; // é�� ������ ���� �̺�Ʈ
    [SerializeField] private GameObject bossEssenceUI; // é�� ������ EssenceUI

    [Header("�нú� �����Ƽ ����")]
    [SerializeField] private AbilitySelectionPanel abilitySelectionPanel;

    private string currentStageID;
    private StageData currentStage;
    private List<ICreatureStatus> activeMonsters = new List<ICreatureStatus>();
    private bool isStageClear = false;
    private Vector3 stageCenter = Vector3.zero;
    private GameObject currentPortal;


    [SerializeField]
    private List<string> dungeonSceneNames = new List<string>
    {
        "Chapter1Dungeon", "Chapter2Dungeon", "Chapter3Dungeon", "Chapter4Dungeon"
    };

    protected override void Awake()
    {
        base.Awake();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        // �̺�Ʈ ����
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // �� �ε� �� ȣ��Ǵ� �̺�Ʈ �ڵ鷯
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // ��ϵ� ���� �� Ȯ��
        if (dungeonSceneNames.Contains(scene.name))
        {
            // �������� �ε� ����
            if (DungeonDataManager.Instance.IsInitialized())
            {
                string stageID = PlayerPrefs.GetString("CurrentStageID", "1_1");

                Debug.Log($"�� ��ȯ �� ��ų ���� ����: {SkillConfigManager.Instance.GetAllSkillConfigs().Count}");

                // Ư�� ID Ȯ��
                var config = SkillConfigManager.Instance.GetSkillConfig(1);
                Debug.Log($"��ų ���� ID 1: {(config != null ? "������" : "����")}");

                // ���� ���� �� �нú� �����Ƽ �Ŵ��� �ʱ�ȭ
                if (DungeonAbilityManager.Instance != null)
                {
                    DungeonAbilityManager.Instance.InitializeDungeon();
                    Debug.Log("���� ����: �нú� �����Ƽ �Ŵ��� �ʱ�ȭ �Ϸ�");
                }
                else
                {
                    Debug.LogWarning("DungeonAbilityManager�� ã�� �� �����ϴ�.");
                }

                LoadStage(stageID, false);
            }
            else
            {
                Debug.LogError("DungeonDataManager�� �ʱ�ȭ���� �ʾҽ��ϴ�.");
            }
        }
    }

    // �������� �ε� (���̵� ȿ�� �ɼ�)
    public void LoadStage(string stageID, bool withFadeEffect = true)
    {
        if (withFadeEffect && SceneTransitionManager.Instance != null)
        {
            // ���� ������������ Ȯ��
            StageData stageData = DungeonDataManager.Instance.GetStageData(stageID);
            bool isBossStage = stageData != null && stageData.isBossStage;

            // ���̵� ȿ�� ���� ����
            Color fadeColor = isBossStage ? bossRoomColor : Color.black;

            // ���̵� �� ȿ��
            SceneTransitionManager.Instance.FadeIn(() => {
                InternalLoadStage(stageID);

                // �������� �ε� �� ���̵� �ƿ�
                DOVirtual.DelayedCall(transitionDelay, () => {
                    SceneTransitionManager.Instance.FadeOut();
                });
            }, fadeColor);
        }
        else
        {
            // ���̵� ���� �ٷ� �ε�
            InternalLoadStage(stageID);
        }
    }

    // ���� �������� �ε� ����
    private void InternalLoadStage(string stageID)
    {
        // ���� ���� ����
        ClearActiveMonsters();

        // ��Ż ����
        if (currentPortal != null)
        {
            Destroy(currentPortal);
            currentPortal = null;
        }

        isStageClear = false;

        // �������� ������ ��������
        currentStageID = stageID;
        currentStage = DungeonDataManager.Instance.GetStageData(stageID);

        if (currentStage == null)
        {
            Debug.LogError($"�������� �����͸� ã�� �� ����: {stageID}");
            return;
        }
        // ���� ������������ Ȯ�� �� UI ó��
        HandleBossStageUI(currentStage);
        // �÷��̾� ��ġ ����
        GameObject player = GameInitializer.Instance.gameObject;
        if (player != null && currentStage.playerSpawnPosition != Vector3.zero)
        {

            // ��ü �̵� ���
            Rigidbody rb = player.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
                player.transform.position = currentStage.playerSpawnPosition;
                DOVirtual.DelayedCall(0.2f, () => {
                    rb.isKinematic = false;
                });
            }
            else
            {
                player.transform.position = currentStage.playerSpawnPosition;
            }

        }

        // ���� ����
        StartCoroutine(SpawnMonstersWithDelay());

        Debug.Log($"�������� �ε� �Ϸ�: {currentStage.stageName} (ID: {stageID})");

        // �������� �ε� �Ϸ� �̺�Ʈ �߻�
        OnStageLoaded?.Invoke(currentStage);
    }

    private void HandleBossStageUI(StageData stageData)
    {
        if (stageData.isBossStage)
        {
            if (bossEssenceUI != null)
            {
                // UI Ȱ��ȭ
                bossEssenceUI.SetActive(true);
                Debug.Log("���� ������ UI Ȱ��ȭ");
            }
            else
            {
                Debug.LogWarning("���� ������ UI�� ã�� �� �����ϴ�.");
            }
        }
        else
        {
            // �Ϲ� ���������� ��� UI ��Ȱ��ȭ

            if (bossEssenceUI != null)
            {
                bossEssenceUI.SetActive(false);
            }
        }
    }

    // ���� ���� (�����̷� ������ ����)
    private IEnumerator SpawnMonstersWithDelay()
    {
        // �������� �߽��� ��� (��Ż ���� ��ġ��)
        CalculateStageCenter();

        // ���� ���������� ��� ���� �߰�
        if (currentStage.isBossStage)
        {
            // ���� �ð� ��� (ī�޶� �Ҵ� ���� ���� ����)
            yield return new WaitForSeconds(bossIntroDelay);

            // ���� ���� ȿ�� (SceneTransitionManager�� FlashEffect Ȱ��)
            if (SceneTransitionManager.Instance != null)
            {
                SceneTransitionManager.Instance.FlashEffect(Color.red, 0.5f);
            }
        }

        // ���� ��ġ ���� ����
        foreach (var spawn in currentStage.fixedSpawns)
        {
            // ����/�̵庸���� �ణ�� ��� �� ����
            if (spawn.monsterID >= 1000 || (currentStage.isMidBossStage && spawn.isFixedPosition))
            {
                yield return new WaitForSeconds(0.5f);
            }

            MonsterFactoryBase factory = GetFactoryForMonsterId(spawn.monsterID);

            // ���� ����Ʈ
            if (spawnEffectPrefab != null)
            {
                Instantiate(spawnEffectPrefab, spawn.position, Quaternion.identity);
            }

            // ��� ������ �� ���� ����
            yield return new WaitForSeconds(spawnDelay);
            factory?.CreateMonster(spawn.position, OnMonsterCreated);
        }

        // ���� ��ġ ���� ����
        if (currentStage.randomMonsters.Count > 0 && currentStage.spawnPoints.Count > 0)
        {
            int count = Mathf.Min(currentStage.totalRandomSpawns, currentStage.spawnPoints.Count);
            List<Vector3> availablePoints = new List<Vector3>(currentStage.spawnPoints);

            for (int i = 0; i < count; i++)
            {
                if (availablePoints.Count == 0) break;

                // ���� ��ġ ����
                int pointIndex = Random.Range(0, availablePoints.Count);
                Vector3 position = availablePoints[pointIndex];
                availablePoints.RemoveAt(pointIndex);

                // ����ġ ��� ���� ����
                MonsterSpawnInfo monster = SelectRandomMonster();
                if (monster == null) continue;

                // ���� ����Ʈ
                if (spawnEffectPrefab != null)
                {
                    Instantiate(spawnEffectPrefab, position, Quaternion.identity);
                }

                // ������ �� ���� ����
                yield return new WaitForSeconds(spawnDelay);
                MonsterFactoryBase factory = GetFactoryForMonsterId(monster.monsterID);
                factory?.CreateMonster(position, OnMonsterCreated);
            }
        }
    }

    // �������� �߽��� ��� (��Ż ��)
    private void CalculateStageCenter()
    {
        //��Ż ��ġ�� �����Ǿ� ������ �� ��ġ ���
        if (currentStage.portalSpawnPosition != Vector3.zero)
        {
            stageCenter = currentStage.portalSpawnPosition;
            return;
        }

        // �ƴϸ� �ڵ� ���
        if (currentStage.fixedSpawns.Count > 0)
        {
            Vector3 sum = Vector3.zero;
            foreach (var spawn in currentStage.fixedSpawns)
            {
                sum += spawn.position;
            }
            stageCenter = sum / currentStage.fixedSpawns.Count;
        }
        else if (currentStage.spawnPoints.Count > 0)
        {
            Vector3 sum = Vector3.zero;
            foreach (var point in currentStage.spawnPoints)
            {
                sum += point;
            }
            stageCenter = sum / currentStage.spawnPoints.Count;
        }
        else
        {
            GameObject player = GameObject.FindGameObjectWithTag("PlayerObj");
            if (player != null)
            {
                stageCenter = player.transform.position;
            }
        }
    }

    // ����ġ ��� ���� ���� ����
    private MonsterSpawnInfo SelectRandomMonster()
    {
        if (currentStage.randomMonsters.Count == 0) return null;

        float totalWeight = 0f;
        foreach (var monster in currentStage.randomMonsters)
        {
            totalWeight += monster.spawnWeight;
        }

        float random = Random.Range(0f, totalWeight);
        float current = 0f;

        foreach (var monster in currentStage.randomMonsters)
        {
            current += monster.spawnWeight;
            if (random <= current)
            {
                return monster;
            }
        }

        return currentStage.randomMonsters[0];
    }

    // ���� ���� �ݹ�
    private void OnMonsterCreated(ICreatureStatus monster)
    {
        if (monster != null)
        {
            activeMonsters.Add(monster);
            Debug.Log($"���� ������: {monster.GetType().Name}");
        }
    }

    // ���� ID�� ���� ���丮 ��ȯ
    private MonsterFactoryBase GetFactoryForMonsterId(int monsterId)
    {
        // ���� ���� (ID�� 1000 �̻�)
        if (monsterId >= 1000)
        {
            switch (monsterId)
            {
                case 1001:
                    return new CrabBossFactory();
                case 1002:
                    return new AlexanderBossFactory();
                default:
                    Debug.LogError($"�� �� ���� ���� ID: {monsterId}");
                    return new DummyMonsterFactory();
            }
        }

        // �Ϲ� ����
        switch (monsterId)
        {
            case 2:
                return new DummyMonsterFactory();
            case 3:
                return new SPiderMonsterFactory();
            case 4:
                return new SlimeMonsterFactory();
            case 5:
                return new TurtleMonsterFactory();
            case 6:
                return new RayMonsterFactory();
            case 7:
                return new WarmMonsterFactory();
            case 8:
                return new RatMonsterFactory();
            default:
                Debug.LogWarning($"�� �� ���� ���� ID: {monsterId}, �⺻ ���ͷ� ��ü�մϴ�.");
                return new DummyMonsterFactory();
        }
    }

    // �������� Ŭ���� Ȯ��
    public void CheckStageClear()
    {
        // �̹� Ŭ������ ��� ��ŵ
        if (isStageClear) return;

        // ��ü ���� üũ
        bool allDefeated = true;
        foreach (var monster in activeMonsters)
        {
            if (monster != null && monster.GetMonsterClass().IsAlive)
            {
                allDefeated = false;
                break;
            }
        }

        // ��� ���Ͱ� óġ�ƴٸ� Ŭ���� ó��
        if (allDefeated && activeMonsters.Count > 0)
        {
            OnStageClear();
        }
    }

    // �������� Ŭ���� ó��
    private void OnStageClear()
    {
        Debug.Log($"�������� Ŭ����: {currentStage.stageName}");
        isStageClear = true;

        if (currentStage.isBossStage)
        {
            Debug.Log("é�� Ŭ����! �������� ��ȯ�մϴ�.");

            // ���� óġ ���� ȿ��
            if (SceneTransitionManager.Instance != null)
            {
                SceneTransitionManager.Instance.ScreenColorEffect(Color.white, 0.3f, 1.5f);
            }

            // ������ �� �������� �̵�
            DOVirtual.DelayedCall(5f, async () => {
                await ReturnToVillage();
            });
        }
        else
        {
            // �нú� �����Ƽ ���� UI ǥ��
            ShowAbilitySelection();

            // ���� �������� ID
            string nextStageID = currentStage.nextStageID;

            if (nextStageID == "Village")
            {
                // �������� ��ȯ
                DOVirtual.DelayedCall(3f, async () => {
                    await ReturnToVillage();
                });
            }
        }
    }

    // �нú� �����Ƽ ���� UI ǥ�� �޼��� �߰�
    private void ShowAbilitySelection()
    {
        // AbilitySelectionPanel ������Ʈ�� ã�ų� ����
        if (abilitySelectionPanel == null)
        {
            abilitySelectionPanel = FindObjectOfType<AbilitySelectionPanel>();
        }

        if (abilitySelectionPanel == null)
        {
            Debug.LogError("AbilitySelectionPanel�� ã�� �� �����ϴ�.");

            // ��Ż �ٷ� ���� (���� ��Ŀ����)
            DOVirtual.DelayedCall(portalSpawnDelay, () => {
                CheckItemsAndSpawnPortal(currentStage.nextStageID);
            });
            return;
        }

        // ���� �����Ƽ �Ŵ������� ���� ������ �ɷ� ��� ��������
        List<DungeonAbility> abilitySelection = DungeonAbilityManager.Instance.GetSmartAbilitySelection();

        if (abilitySelection.Count == 0)
        {
            Debug.LogWarning("���� ������ �нú� �ɷ��� �����ϴ�. �г��� �ǳʶٰ� ��Ż�� �����մϴ�.");

            // ��Ż �ٷ� ����
            DOVirtual.DelayedCall(portalSpawnDelay, () => {
                CheckItemsAndSpawnPortal(currentStage.nextStageID);
            });
            return;
        }

        // ���� �Ϸ� �ݹ� ���� (��Ż ����)
        System.Action onSelectionCompleted = () => {
            DOVirtual.DelayedCall(1f, () => {
                CheckItemsAndSpawnPortal(currentStage.nextStageID);
            });
        };

        // �г� ���� ���� �� ǥ��
        abilitySelectionPanel.SetTitle("�нú� �ɷ� ����");
        abilitySelectionPanel.ShowSelectionPanel(abilitySelection, onSelectionCompleted);
    }

    // ������ Ȯ�� �� ��Ż ����
    private void CheckItemsAndSpawnPortal(string nextStageID)
    {
        // ���� ������ ���̾ ���� ������Ʈ�� ã��
        Collider[] itemColliders = Physics.OverlapSphere(stageCenter, 20f, LayerMask.GetMask("Item"));

        if (itemColliders.Length == 0)
        {
            // �������� ������ �ٷ� ��Ż ����
            SpawnPortal(nextStageID);
        }
        else
        {
            Debug.Log($"���� ȹ������ ���� �������� {itemColliders.Length}�� �ֽ��ϴ�. ��Ż ������ ����մϴ�.");

            // 3�� �� �ٽ� Ȯ��
            DOVirtual.DelayedCall(3f, () => {
                CheckItemsAndSpawnPortal(nextStageID);
            });
        }
    }

    // ��Ż ����
    private void SpawnPortal(string nextStageID)
    {
        if (currentPortal != null)
        {
            Destroy(currentPortal);
        }

        // ��Ż ��ġ ���
        Vector3 portalPosition = stageCenter + portalSpawnOffset;

        // ��Ż ���� ����Ʈ
        if (portalAppearEffectPrefab != null)
        {
            Instantiate(portalAppearEffectPrefab, portalPosition, Quaternion.identity);
        }

        // ������ �� ��Ż ����
        DOVirtual.DelayedCall(0.5f, () => {
            // ��Ż ������ ����
            currentPortal = Instantiate(portalPrefab, portalPosition, Quaternion.identity);

            // ũ�� �ִϸ��̼�
            currentPortal.transform.localScale = Vector3.zero;
            currentPortal.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);

            // StagePortal ������Ʈ ���� (DungeonPortal -> StagePortal�� ����)
            StagePortal portalComponent = currentPortal.GetComponent<StagePortal>();
            if (portalComponent != null)
            {
                portalComponent.targetStageID = nextStageID;
            }

            // ȸ�� �ִϸ��̼�
            StartCoroutine(RotatePortal(currentPortal.transform));
        });
    }

    // ��Ż ȸ�� �ִϸ��̼�
    private IEnumerator RotatePortal(Transform portalTransform)
    {
        float rotationSpeed = 30f;

        while (portalTransform != null)
        {
            portalTransform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
            yield return null;
        }
    }

    // �������� ��ȯ
    private async Task ReturnToVillage()
    {
        Debug.Log("�������� ��ȯ�մϴ�.");

        // �ε� ȭ�� ǥ�� (���� ���) - ���̵� ȿ�� ����
        if (LoadingScreen.Instance != null)
        {
            LoadingScreen.Instance.ShowLoading("Village");
        }
        else
        {
            // �ε� ȭ���� ���� ��츸 SceneTransitionManager ���
            if (SceneTransitionManager.Instance != null)
            {
                SceneTransitionManager.Instance.FadeIn();
                await Task.Delay(500);
            }

            var operation = SceneManager.LoadSceneAsync("Village");
            while (!operation.isDone)
            {
                await Task.Yield();
            }

            if (SceneTransitionManager.Instance != null)
            {
                SceneTransitionManager.Instance.FadeOut();
            }
        }
    }

    // Ȱ��ȭ�� ���� ����
    private void ClearActiveMonsters()
    {
        // ���Ϳ� ����� ���� ������Ʈ ���
        List<GameObject> monsterObjects = new List<GameObject>();

        // ��� MonsterStatus ã��
        MonsterStatus[] monsterStatuses = FindObjectsOfType<MonsterStatus>();
        foreach (var status in monsterStatuses)
        {
            monsterObjects.Add(status.gameObject);
        }

        // ��� BossStatus ã��
        BossStatus[] bossStatuses = FindObjectsOfType<BossStatus>();
        foreach (var status in bossStatuses)
        {
            monsterObjects.Add(status.gameObject);
        }

        // ��� ���� ���� ������Ʈ ����
        foreach (var obj in monsterObjects)
        {
            Destroy(obj);
        }

        activeMonsters.Clear();
    }
    // DungeonManager Ŭ������ �߰�
    public List<ICreatureStatus> GetActiveMonsters()
    {
        // ����ִ� ���͸� ��ȯ�ϰų�, ��ü ����Ʈ ��ȯ
        return new List<ICreatureStatus>(activeMonsters);
    }
    // ���� óġ �̺�Ʈ ó��
    public void OnMonsterDefeated(ICreatureStatus monster)
    {
        if (monster != null && activeMonsters.Contains(monster))
        {
            Debug.Log($"���� óġ��: {monster.GetType().Name}");

            // ���� óġ �� Ư�� ó��
            if (monster is BossMonster || monster is AlexanderBoss)
            {
                Debug.Log("���� óġ! �������� Ŭ���� ���� ����");
                // ���� óġ ���� ����
            }

            // �������� Ŭ���� üũ
            CheckStageClear();
        }
    }
    public float GetDungeonProgress()
    {
        // ���� �������� ID���� ��ȣ ���� (��: "1-5"���� 5 ����)
        if (currentStage != null)
        {
            int stageNumber = ExtractStageNumber(currentStage.stageID);
            int maxStageNumber = 10; // �� é�ʹ� �ִ� �������� ��
            float progress = Mathf.Clamp01((float)stageNumber / maxStageNumber);
            Debug.Log($"���� ���൵ ���: �������� ID={currentStage.stageID}, �������� ��ȣ={stageNumber}, ���൵={progress:F2}");
            return progress;
        }
        Debug.LogWarning("���� ���൵ ��� ����: currentStage�� null");
        return 0f;
    }

    public int GetCurrentStageNumber()
    {
        // ���� �������� ID���� ��ȣ�� ���� (��: "1-5"���� 5 ����)
        if (currentStage != null)
        {
            return ExtractStageNumber(currentStage.stageID);
        }
        return 1; // �⺻��
    }

    private int ExtractStageNumber(string stageID)
    {
        // �������� ID ����: "1-5" (é��-��������)
        if (!string.IsNullOrEmpty(stageID) && stageID.Contains("_"))
        {
            string[] parts = stageID.Split('_');
            if (parts.Length >= 2 && int.TryParse(parts[1], out int stageNumber))
            {
                Debug.Log($"�������� ��ȣ ����: {stageID} -> {stageNumber}");
                return stageNumber;
            }
        }
        Debug.LogWarning($"�������� ��ȣ ���� ����: {stageID}, �⺻�� 1 ���");
        return 1; // �Ľ� ���� �� �⺻��
    }
}