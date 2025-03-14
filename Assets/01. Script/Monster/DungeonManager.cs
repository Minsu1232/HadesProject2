using GSpawn_Pro;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

/// <summary>
/// 던전 몬스터 스폰 관리 스크립트
/// </summary>
public class DungeonManager : Singleton<DungeonManager>
{
    private MonsterFactoryBase monsterFactory;
    private IMonsterClass currentMonster;
    [SerializeField] private Transform player;

    // 던전 타입을 enum으로 정의
    public enum DungeonType
    {
        Test,
        SpiderTest,
        SlimeTest,
        JumpTest,
        CrabBoss, // 보스 던전 추가
        AlexanderBoss,
        RayTest,
        warmTest,
        RatTest
    }

    [Title("Dungeon Settings")]
    [EnumToggleButtons] // 토글 버튼 형식으로 표시
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
                Debug.LogWarning("유효한 마우스 위치를 찾을 수 없습니다. 기본 위치에 소환합니다.");
                SpawnSelectedMonster(defaultSpawnPosition);
            }
        }
        else
        {
            SpawnSelectedMonster(defaultSpawnPosition);
        }
    }

    // 여러 몬스터 소환 (테스트용)
    [PropertySpace(5)]
    [Button(ButtonSizes.Large), GUIColor(1f, 0.8f, 0.2f)]
    public void SpawnMultipleMonsters(int count = 3, float radius = 5f)
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("게임 실행 중에만 몬스터를 소환할 수 있습니다.");
            return;
        }

        for (int i = 0; i < count; i++)
        {
            // 원형으로 배치
            float angle = i * (360f / count) * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius;
            Vector3 spawnPos = defaultSpawnPosition + offset;

            // 약간의 지연을 두고 소환 (동시 소환 방지)
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

        // 플레이어 참조 설정
        if (GameInitializer.Instance != null && GameInitializer.Instance.GetPlayerClass() != null)
        {
            player = GameInitializer.Instance.GetPlayerClass().playerTransform.transform;
        }
        else
        {
            Debug.LogWarning("플레이어 참조를 찾을 수 없습니다. 몬스터의 타겟 설정이 제대로 작동하지 않을 수 있습니다.");

            // 씬에 있는 Player 태그를 가진 오브젝트 찾기 시도
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
                Debug.Log("씬에서 Player 태그를 가진 오브젝트를 찾아 설정했습니다.");
            }
        }

        // 초기 몬스터 소환
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

    // 던전 타입이 변경될 때 호출되는 메서드
    private void OnDungeonTypeChanged()
    {
        UpdateMonsterFactory();
    }

    // 몬스터 팩토리 업데이트 - 매번 새로운 인스턴스 생성
    private void UpdateMonsterFactory()
    {
        monsterFactory = GetMonsterFactoryForDungeon(selectedDungeonType.ToString());
    }

    private MonsterFactoryBase GetMonsterFactoryForDungeon(string dungeonType)
    {
        switch (dungeonType.ToLower()) // 대소문자 구분 없이 처리
        {
            case "test":
                return new DummyMonsterFactory();
            case "spidertest":
                return new SPiderMonsterFactory();
            case "slimetest":
                // 슬라임, 터틀 몬스터를 둘 다 생성할 수 있는 CompositeMonsterFactory 반환
                return new CompositeMonsterFactory(
                    new SlimeMonsterFactory(),
                    new TurtleMonsterFactory()
                );
            case "jumptest":
                return new SlimeMonsterFactory();
            case "crabboss":
                return new CrabBossFactory();  // 보스 ID 전달 
            case "raytest":
                return new RayMonsterFactory();
            case "warmtest":
                return new WarmMonsterFactory();
            case "rattest":
                return new RatMonsterFactory();
            case "alexanderboss":
                return new AlexanderBossFactory();
            default:
                Debug.LogError($"알 수 없는 던전 타입입니다: {dungeonType}");
                return null;
        }
    }

    // 선택된 몬스터 타입에 따라 소환
    private async void SpawnSelectedMonster(Vector3 spawnPosition)
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("게임 실행 중에만 몬스터를 소환할 수 있습니다.");
            return;
        }

        // 매니저들이 초기화되었는지 확인
        if (!isInitialized)
        {
            await InitializeManagers();
        }

        // 새 몬스터를 소환할 때마다 팩토리를 새로 생성 (중요!)
        monsterFactory = GetMonsterFactoryForDungeon(selectedDungeonType.ToString());

        if (monsterFactory != null)
        {
            SpawnMonster(spawnPosition);
        }
        else
        {
            Debug.LogError("몬스터 팩토리가 초기화되지 않았습니다.");
        }
    }

    private void SpawnMonster(Vector3 spawnPosition)
    {
        // CreateMonster 호출 전에 monsterFactory가 null이 아닌지 확인
        if (monsterFactory == null)
        {
            Debug.LogError("몬스터 팩토리가 null입니다. 소환할 수 없습니다.");
            return;
        }

        // 여기서 팩토리를 통해 몬스터 생성
        monsterFactory.CreateMonster(spawnPosition, monster =>
        {
            if (monster != null)
            {
                // 이전 몬스터의 참조를 정리 (필요한 경우)
                if (currentMonster != null)
                {
                   
                }

                currentMonster = monster; // 현재 생성된 몬스터를 필드에 저장
                Debug.Log($"{selectedDungeonType} 타입 몬스터가 위치 {spawnPosition}에 소환되었습니다.");
            }
            else
            {
                Debug.LogError("몬스터 생성에 실패했습니다.");
            }
        });
    }

    public IMonsterClass GetMonsterClass()
    {
        return currentMonster;
    }

    // Player Transform을 반환하는 메서드
    public Transform GetPlayerTransform()
    {
        return player;
    }

    // 마우스 위치를 월드 좌표로 변환
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
    // 에디터 전용 버튼 (런타임 외에도 동작)
    [PropertySpace(10)]
    [Button(ButtonSizes.Large), GUIColor(1f, 0.6f, 0.4f)]
    private void UpdateSelectedMonsterType()
    {
        UpdateMonsterFactory();
        Debug.Log($"몬스터 타입이 {selectedDungeonType}(으)로 업데이트되었습니다.");
    }
#endif
}