using GSpawn_Pro;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

/// <summary>
/// ���� ���� ���� ���� ��ũ��Ʈ
/// </summary>
public class DungeonManager : Singleton<DungeonManager>
{
    private MonsterFactoryBase monsterFactory;
    private IMonsterClass currentMonster;
    [SerializeField] private Transform player;

    // ���� Ÿ���� enum���� ����
    public enum DungeonType
    {
        Test,
        SpiderTest,
        SlimeTest,
        JumpTest,
        CrabBoss, // ���� ���� �߰�
        AlexanderBoss,
        RayTest,
        warmTest,
        RatTest
    }

    [Title("Dungeon Settings")]
    [EnumToggleButtons] // ��� ��ư �������� ǥ��
    [SerializeField, OnValueChanged("OnDungeonTypeChanged")]
    private DungeonType selectedDungeonType;

    [Title("Spawn Settings")]
    [SerializeField] private Vector3 defaultSpawnPosition = new Vector3(7.5999999f, 0f, 57.0354156f);
    [SerializeField] private bool useMousePosition = false;

    [PropertySpace(10)]
    [HorizontalGroup("Spawn Buttons")]
    [Button(ButtonSizes.Large), GUIColor(0.4f, 0.8f, 1f)]
    public void SpawnMonsterAtDefault()
    {
        SpawnSelectedMonster(defaultSpawnPosition);
    }

    [HorizontalGroup("Spawn Buttons")]
    [Button(ButtonSizes.Large), GUIColor(0.4f, 1f, 0.6f)]
    public void SpawnMonsterAtMouse()
    {
        if (useMousePosition && Application.isPlaying)
        {
            Vector3 spawnPos = GetMouseWorldPosition();
            if (spawnPos != Vector3.zero)
            {
                SpawnSelectedMonster(spawnPos);
            }
            else
            {
                Debug.LogWarning("��ȿ�� ���콺 ��ġ�� ã�� �� �����ϴ�. �⺻ ��ġ�� ��ȯ�մϴ�.");
                SpawnSelectedMonster(defaultSpawnPosition);
            }
        }
        else
        {
            SpawnSelectedMonster(defaultSpawnPosition);
        }
    }

    // ���� ���� ��ȯ (�׽�Ʈ��)
    [PropertySpace(5)]
    [Button(ButtonSizes.Large), GUIColor(1f, 0.8f, 0.2f)]
    public void SpawnMultipleMonsters(int count = 3, float radius = 5f)
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("���� ���� �߿��� ���͸� ��ȯ�� �� �ֽ��ϴ�.");
            return;
        }

        for (int i = 0; i < count; i++)
        {
            // �������� ��ġ
            float angle = i * (360f / count) * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius;
            Vector3 spawnPos = defaultSpawnPosition + offset;

            // �ణ�� ������ �ΰ� ��ȯ (���� ��ȯ ����)
            StartCoroutine(SpawnWithDelay(spawnPos, i * 0.2f));
        }
    }

    private IEnumerator SpawnWithDelay(Vector3 position, float delay)
    {
        yield return new WaitForSeconds(delay);
        SpawnSelectedMonster(position);
    }

    private bool isInitialized = false;

    private void Awake()
    {
    }

    private async void Start()
    {
        await InitializeManagers();

        // �÷��̾� ���� ����
        if (GameInitializer.Instance != null && GameInitializer.Instance.GetPlayerClass() != null)
        {
            player = GameInitializer.Instance.GetPlayerClass().playerTransform.transform;
        }
        else
        {
            Debug.LogWarning("�÷��̾� ������ ã�� �� �����ϴ�. ������ Ÿ�� ������ ����� �۵����� ���� �� �ֽ��ϴ�.");

            // ���� �ִ� Player �±׸� ���� ������Ʈ ã�� �õ�
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
                Debug.Log("������ Player �±׸� ���� ������Ʈ�� ã�� �����߽��ϴ�.");
            }
        }

        // �ʱ� ���� ��ȯ
        SpawnMonsterAtDefault();
    }

    private async Task InitializeManagers()
    {
        if (!isInitialized)
        {
            await SkillConfigManager.Instance.Initialize();
            await MonsterDataManager.Instance.InitializeMonsters();
            await BossDataManager.Instance.InitializeBosses();
            isInitialized = true;
        }
    }

    // ���� Ÿ���� ����� �� ȣ��Ǵ� �޼���
    private void OnDungeonTypeChanged()
    {
        UpdateMonsterFactory();
    }

    // ���� ���丮 ������Ʈ - �Ź� ���ο� �ν��Ͻ� ����
    private void UpdateMonsterFactory()
    {
        monsterFactory = GetMonsterFactoryForDungeon(selectedDungeonType.ToString());
    }

    private MonsterFactoryBase GetMonsterFactoryForDungeon(string dungeonType)
    {
        switch (dungeonType.ToLower()) // ��ҹ��� ���� ���� ó��
        {
            case "test":
                return new DummyMonsterFactory();
            case "spidertest":
                return new SPiderMonsterFactory();
            case "slimetest":
                // ������, ��Ʋ ���͸� �� �� ������ �� �ִ� CompositeMonsterFactory ��ȯ
                return new CompositeMonsterFactory(
                    new SlimeMonsterFactory(),
                    new TurtleMonsterFactory()
                );
            case "jumptest":
                return new SlimeMonsterFactory();
            case "crabboss":
                return new CrabBossFactory();  // ���� ID ���� 
            case "raytest":
                return new RayMonsterFactory();
            case "warmtest":
                return new WarmMonsterFactory();
            case "rattest":
                return new RatMonsterFactory();
            case "alexanderboss":
                return new AlexanderBossFactory();
            default:
                Debug.LogError($"�� �� ���� ���� Ÿ���Դϴ�: {dungeonType}");
                return null;
        }
    }

    // ���õ� ���� Ÿ�Կ� ���� ��ȯ
    private async void SpawnSelectedMonster(Vector3 spawnPosition)
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("���� ���� �߿��� ���͸� ��ȯ�� �� �ֽ��ϴ�.");
            return;
        }

        // �Ŵ������� �ʱ�ȭ�Ǿ����� Ȯ��
        if (!isInitialized)
        {
            await InitializeManagers();
        }

        // �� ���͸� ��ȯ�� ������ ���丮�� ���� ���� (�߿�!)
        monsterFactory = GetMonsterFactoryForDungeon(selectedDungeonType.ToString());

        if (monsterFactory != null)
        {
            SpawnMonster(spawnPosition);
        }
        else
        {
            Debug.LogError("���� ���丮�� �ʱ�ȭ���� �ʾҽ��ϴ�.");
        }
    }

    private void SpawnMonster(Vector3 spawnPosition)
    {
        // CreateMonster ȣ�� ���� monsterFactory�� null�� �ƴ��� Ȯ��
        if (monsterFactory == null)
        {
            Debug.LogError("���� ���丮�� null�Դϴ�. ��ȯ�� �� �����ϴ�.");
            return;
        }

        // ���⼭ ���丮�� ���� ���� ����
        monsterFactory.CreateMonster(spawnPosition, monster =>
        {
            if (monster != null)
            {
                // ���� ������ ������ ���� (�ʿ��� ���)
                if (currentMonster != null)
                {
                   
                }

                currentMonster = monster; // ���� ������ ���͸� �ʵ忡 ����
                Debug.Log($"{selectedDungeonType} Ÿ�� ���Ͱ� ��ġ {spawnPosition}�� ��ȯ�Ǿ����ϴ�.");
            }
            else
            {
                Debug.LogError("���� ������ �����߽��ϴ�.");
            }
        });
    }

    public IMonsterClass GetMonsterClass()
    {
        return currentMonster;
    }

    // Player Transform�� ��ȯ�ϴ� �޼���
    public Transform GetPlayerTransform()
    {
        return player;
    }

    // ���콺 ��ġ�� ���� ��ǥ�� ��ȯ
    private Vector3 GetMouseWorldPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            return hit.point;
        }

        return Vector3.zero;
    }

#if UNITY_EDITOR
    // ������ ���� ��ư (��Ÿ�� �ܿ��� ����)
    [PropertySpace(10)]
    [Button(ButtonSizes.Large), GUIColor(1f, 0.6f, 0.4f)]
    private void UpdateSelectedMonsterType()
    {
        UpdateMonsterFactory();
        Debug.Log($"���� Ÿ���� {selectedDungeonType}(��)�� ������Ʈ�Ǿ����ϴ�.");
    }
#endif
}