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

        // �������� ������ �ʱ�ȭ
        if (stageDataLoader != null)
        {
            await stageDataLoader.Initialize();
        }
        else
        {
            Debug.LogError("StageDataLoader�� �Ҵ���� �ʾҽ��ϴ�.");
        }

        isInitialized = true;
        Debug.Log("���� �ʱ�ȭ �Ϸ�");
    }

    // Ư�� ���������� ���� ���� (�����/�׽�Ʈ��)
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