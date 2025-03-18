using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class DungeonDataManager : Singleton<DungeonDataManager>
{
   [SerializeField] StageDataLoader stageDataLoader;
    private bool isInitialized = false;
    private bool isInitializing = false;

    private async void Start()
    {
       await Initialize();
       
    }

    public async Task Initialize()
    {
        if (isInitialized || isInitializing) return;

        isInitializing = true;
        Debug.Log("게임 데이터 초기화 시작");

        try
        {
            // 병렬로 모든 데이터 매니저 초기화
            Task stageTask = InitializeStageData();
            Task monsterTask = InitializeMonsterData();
            Task bossTask = InitializeBossData();
            Task skillTask = InitializeSkillData();

            // 모든 초기화 작업 완료 대기
            await Task.WhenAll(stageTask, monsterTask, bossTask, skillTask);

            isInitialized = true;
            isInitializing = false;
            Debug.Log("게임 데이터 초기화 완료");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"게임 데이터 초기화 실패: {e.Message}");
            isInitializing = false;
        }
    }

    // 스테이지 데이터 초기화
    private async Task InitializeStageData()
    {
        if (stageDataLoader != null)
        {
            await stageDataLoader.Initialize();
            Debug.Log("스테이지 데이터 초기화 완료");
        }
        else
        {
            Debug.LogError("StageDataLoader를 찾을 수 없음");
        }
    }

    // 몬스터 데이터 초기화
    private async Task InitializeMonsterData()
    {
        if (MonsterDataManager.Instance != null)
        {
            await MonsterDataManager.Instance.InitializeMonsters();
            Debug.Log("몬스터 데이터 초기화 완료");
        }
        else
        {
            Debug.LogError("MonsterDataManager를 찾을 수 없음");
        }
    }

    // 보스 데이터 초기화
    private async Task InitializeBossData()
    {
        if (BossDataManager.Instance != null)
        {
            await BossDataManager.Instance.InitializeBosses();
            Debug.Log("보스 데이터 초기화 완료");
        }
        else
        {
            Debug.LogError("BossDataManager를 찾을 수 없음");
        }
    }

    // 스킬 데이터 초기화
    private async Task InitializeSkillData()
    {
        if (SkillConfigManager.Instance != null)
        {
            await SkillConfigManager.Instance.Initialize();
            Debug.Log("스킬 데이터 초기화 완료");
            Debug.Log($" 스킬 구성 개수: {SkillConfigManager.Instance.GetAllSkillConfigs().Count}");
        }
        else
        {
            Debug.LogError("SkillConfigManager를 찾을 수 없음");
        }
    }

    // 초기화 상태 확인
    public bool IsInitialized()
    {
        return isInitialized;
    }

    public StageData GetStageData(string stageID)
    {
       return stageDataLoader.GetStageData(stageID);
    }
}
