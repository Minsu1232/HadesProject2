using UnityEngine;
using System.Threading.Tasks;

public class GameManager : MonoBehaviour
{
    [SerializeField] private StageDataLoader stageDataLoader;
    [SerializeField] private DungeonManager dungeonManager;

    private bool isInitialized = false;

    private async void Start()
    {
        await Initialize();
    }

    public async Task Initialize()
    {
        if (isInitialized) return;

        // 스테이지 데이터 초기화
        if (stageDataLoader != null)
        {
            await stageDataLoader.Initialize();
        }
        else
        {
            Debug.LogError("StageDataLoader가 할당되지 않았습니다.");
        }

        isInitialized = true;
        Debug.Log("게임 초기화 완료");
    }

    // 특정 스테이지로 직접 진입 (디버깅/테스트용)
    public async Task DebugEnterStage(string stageID)
    {
        if (!isInitialized) 
        {
            await Initialize();
        }

        if (dungeonManager != null)
        {
            dungeonManager.LoadStage(stageID);
        }
    }
}