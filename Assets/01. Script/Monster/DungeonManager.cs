using GSpawn_Pro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// ���� ���� ���� ���� ��ũ��Ʈ
/// </summary>
public class DungeonManager : Singleton<DungeonManager>
{
    private MonsterFactoryBase monsterFactory;
    private MonsterClass currentMonster; // ���� ������ ���͸� �����ϴ� �ʵ�
    [SerializeField] private Transform player; // �÷��̾��� Transform ����

    private void Awake()
    {
        MonsterDataManager.Instance.InitializeMonsters();
    }
    private void Start()
    {
        // ���� �׸��� ���� ������ ���丮 ����
        monsterFactory = GetMonsterFactoryForDungeon("test");

        player = GameInitializer.Instance.GetPlayerClass().playerTransform.transform;
        // �׽�Ʈ�� ���� ����
        SpawnMonster(new Vector3(7.5999999f, 1.28999996f, 57.0354156f));
    }

    private MonsterFactoryBase GetMonsterFactoryForDungeon(string dungeonType)
    {
        switch (dungeonType)
        {
            //case "Forest":
            //    return new ForestMonsterFactory();
            case "test":
                return new DummyMonsterFactory();
            default:
                Debug.LogError("�� �� ���� ���� Ÿ���Դϴ�.");
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
