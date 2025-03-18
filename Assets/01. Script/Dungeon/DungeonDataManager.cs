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
        Debug.Log("���� ������ �ʱ�ȭ ����");

        try
        {
            // ���ķ� ��� ������ �Ŵ��� �ʱ�ȭ
            Task stageTask = InitializeStageData();
            Task monsterTask = InitializeMonsterData();
            Task bossTask = InitializeBossData();
            Task skillTask = InitializeSkillData();

            // ��� �ʱ�ȭ �۾� �Ϸ� ���
            await Task.WhenAll(stageTask, monsterTask, bossTask, skillTask);

            isInitialized = true;
            isInitializing = false;
            Debug.Log("���� ������ �ʱ�ȭ �Ϸ�");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"���� ������ �ʱ�ȭ ����: {e.Message}");
            isInitializing = false;
        }
    }

    // �������� ������ �ʱ�ȭ
    private async Task InitializeStageData()
    {
        if (stageDataLoader != null)
        {
            await stageDataLoader.Initialize();
            Debug.Log("�������� ������ �ʱ�ȭ �Ϸ�");
        }
        else
        {
            Debug.LogError("StageDataLoader�� ã�� �� ����");
        }
    }

    // ���� ������ �ʱ�ȭ
    private async Task InitializeMonsterData()
    {
        if (MonsterDataManager.Instance != null)
        {
            await MonsterDataManager.Instance.InitializeMonsters();
            Debug.Log("���� ������ �ʱ�ȭ �Ϸ�");
        }
        else
        {
            Debug.LogError("MonsterDataManager�� ã�� �� ����");
        }
    }

    // ���� ������ �ʱ�ȭ
    private async Task InitializeBossData()
    {
        if (BossDataManager.Instance != null)
        {
            await BossDataManager.Instance.InitializeBosses();
            Debug.Log("���� ������ �ʱ�ȭ �Ϸ�");
        }
        else
        {
            Debug.LogError("BossDataManager�� ã�� �� ����");
        }
    }

    // ��ų ������ �ʱ�ȭ
    private async Task InitializeSkillData()
    {
        if (SkillConfigManager.Instance != null)
        {
            await SkillConfigManager.Instance.Initialize();
            Debug.Log("��ų ������ �ʱ�ȭ �Ϸ�");
            Debug.Log($" ��ų ���� ����: {SkillConfigManager.Instance.GetAllSkillConfigs().Count}");
        }
        else
        {
            Debug.LogError("SkillConfigManager�� ã�� �� ����");
        }
    }

    // �ʱ�ȭ ���� Ȯ��
    public bool IsInitialized()
    {
        return isInitialized;
    }

    public StageData GetStageData(string stageID)
    {
       return stageDataLoader.GetStageData(stageID);
    }
}
