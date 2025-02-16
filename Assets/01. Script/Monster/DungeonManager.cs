using GSpawn_Pro;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
        BossDungeon, // 보스 던전 추가
        RayTest

    }

    [Title("Dungeon Settings")]
    [EnumToggleButtons] // 토글 버튼 형식으로 표시
    [SerializeField]
    private DungeonType selectedDungeonType;

    private void Awake()
    {

    }

    private async void Start()
    {
        await MonsterDataManager.Instance.InitializeMonsters();
        await BossDataManager.Instance.InitializeBosses();
        // enum 값을 문자열로 변환하여 팩토리 생성
        monsterFactory = GetMonsterFactoryForDungeon(selectedDungeonType.ToString());
        player = GameInitializer.Instance.GetPlayerClass().playerTransform.transform;
        SpawnMonster(new Vector3(7.5999999f, 0f, 57.0354156f));
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
            case "JumpTest":
                return new SlimeMonsterFactory();
            case "bossdungeon":
                return new BossFactory(1);  // 보스 ID 전달
            case "raytest":
                return new RayMonsterFactory();
            default:
                Debug.LogError($"알 수 없는 던전 타입입니다: {dungeonType}");
                return null;
        }
    }

    private void SpawnMonster(Vector3 spawnPosition)
    {
        monsterFactory.CreateMonster(spawnPosition, monster =>
        {
            currentMonster = monster; // 현재 생성된 몬스터를 필드에 저장
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
}
