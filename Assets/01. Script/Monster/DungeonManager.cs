using GSpawn_Pro;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// ���� ���� ���� ���� ��ũ��Ʈ
/// </summary>
public class DungeonManager : Singleton<DungeonManager>
{
    private MonsterFactoryBase monsterFactory;
    private MonsterClass currentMonster;
    [SerializeField] private Transform player;

    // ���� Ÿ���� enum���� ����
    public enum DungeonType
    {
        Test,
        SpiderTest,
        SlimeTest

    }

    [Title("Dungeon Settings")]
    [EnumToggleButtons] // ��� ��ư �������� ǥ��
    [SerializeField]
    private DungeonType selectedDungeonType;

    private async void Awake()
    {
       await MonsterDataManager.Instance.InitializeMonsters();
    }

    private void Start()
    {
        // enum ���� ���ڿ��� ��ȯ�Ͽ� ���丮 ����
        monsterFactory = GetMonsterFactoryForDungeon(selectedDungeonType.ToString());
        player = GameInitializer.Instance.GetPlayerClass().playerTransform.transform;
        SpawnMonster(new Vector3(7.5999999f, 0f, 57.0354156f));
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

            default:
                Debug.LogError($"�� �� ���� ���� Ÿ���Դϴ�: {dungeonType}");
                return null;
        }
    }

    private void SpawnMonster(Vector3 spawnPosition)
    {
        monsterFactory.CreateMonster(spawnPosition, monster =>
        {
            Debug.Log($"Monster {monster.GetName()} ���� �Ϸ�");         
     

            currentMonster = monster; // ���� ������ ���͸� �ʵ忡 ����
        });
    }
    public MonsterClass GetMonsterClass()
    {
        return currentMonster;
    }
    // Player Transform�� ��ȯ�ϴ� �޼���
    public Transform GetPlayerTransform()
    {
        return player;
    }
}
