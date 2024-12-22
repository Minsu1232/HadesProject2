using GSpawn_Pro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 던전 몬스터 스폰 관리 스크립트
/// </summary>
public class DungeonManager : Singleton<DungeonManager>
{
    private MonsterFactoryBase monsterFactory;
    private MonsterClass currentMonster; // 현재 생성된 몬스터를 저장하는 필드
    [SerializeField] private Transform player; // 플레이어의 Transform 참조

    private void Awake()
    {
        MonsterDataManager.Instance.InitializeMonsters();
    }
    private void Start()
    {
        // 던전 테마에 따라 적절한 팩토리 선택
        monsterFactory = GetMonsterFactoryForDungeon("test");

        player = GameInitializer.Instance.GetPlayerClass().playerTransform.transform;
        // 테스트로 몬스터 생성
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
                Debug.LogError("알 수 없는 던전 타입입니다.");
                return null;
        }
    }

    private void SpawnMonster(Vector3 spawnPosition)
    {
        monsterFactory.CreateMonster(spawnPosition, monster =>
        {
            Debug.Log($"Monster {monster.GetName()} 생성 완료");         
     

            currentMonster = monster; // 현재 생성된 몬스터를 필드에 저장
        });
    }
    public MonsterClass GetMonsterClass()
    {
        return currentMonster;
    }
    // Player Transform을 반환하는 메서드
    public Transform GetPlayerTransform()
    {
        return player;
    }
}
