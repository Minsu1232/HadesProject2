//using UnityEngine;
//using UnityEngine.AddressableAssets;
//using UnityEngine.ResourceManagement.AsyncOperations;
//using System.Collections.Generic;
//using System.Threading.Tasks;

//public class StageLoader : MonoBehaviour
//{
//    [SerializeField] private Transform stageContainer;
//    private GameObject currentStage;
//    private List<AsyncOperationHandle> loadedAssets = new List<AsyncOperationHandle>();

//    public async Task<bool> LoadStage(StageData stageData)
//    {
//        try
//        {
//            await CleanupCurrentStage();
//            LoadingUI.Instance.Show();
//            LoadingUI.Instance.UpdateProgress(0.1f);

//            var loadOperation = Addressables.LoadAssetAsync<GameObject>(stageData.stagePrefab);
//            loadedAssets.Add(loadOperation);
//            await loadOperation.Task;
//            LoadingUI.Instance.UpdateProgress(0.4f);

//            if (loadOperation.Status != AsyncOperationStatus.Succeeded)
//            {
//                Debug.LogError($"Failed to load stage prefab: {stageData.stagePrefab}");
//                return false;
//            }

//            currentStage = Instantiate(loadOperation.Result, stageContainer);
//            LoadingUI.Instance.UpdateProgress(0.6f);

//            SetupPlayer(stageData.playerSpawnPoint);
//            LoadingUI.Instance.UpdateProgress(0.7f);

//            await SetupMonsterSpawners(stageData.monsterSpawnPoints);
//            LoadingUI.Instance.UpdateProgress(0.9f);

//            SetupClearCondition(stageData);
//            LoadingUI.Instance.UpdateProgress(1f);

//            await Task.Delay(500);
//            LoadingUI.Instance.Hide();

//            return true;
//        }
//        catch (System.Exception e)
//        {
//            Debug.LogError($"Error loading stage: {e.Message}");
//            LoadingUI.Instance.Hide();
//            return false;
//        }
//    }

//    private async Task CleanupCurrentStage()
//    {
//        if (currentStage != null)
//        {
//            Destroy(currentStage);
//            currentStage = null;
//        }

//        foreach (var handle in loadedAssets)
//        {
//            Addressables.Release(handle);
//        }
//        loadedAssets.Clear();

//        await Task.CompletedTask;
//    }

//    private void SetupPlayer(Vector3 spawnPoint)
//    {
//        GameObject player = GameObject.FindGameObjectWithTag("Player");
//        if (player != null)
//        {
//            player.transform.position = spawnPoint;
//        }
//        else
//        {
//            Debug.LogWarning("Player not found in scene!");
//        }
//    }

//    private async Task SetupMonsterSpawners(List<SpawnPoint> spawnPoints)
//    {
//        var monsterSpawner = currentStage.GetComponent<MonsterSpawner>();
//        if (monsterSpawner == null)
//        {
//            monsterSpawner = currentStage.AddComponent<MonsterSpawner>();
//        }

//        var uniqueMonsterIds = new HashSet<int>();
//        foreach (var spawnPoint in spawnPoints)
//        {
//            uniqueMonsterIds.Add(spawnPoint.monsterId);
//        }

//        foreach (int monsterId in uniqueMonsterIds)
//        {
//            await MonsterManager.Instance.PreloadMonsterAsync(monsterId);
//        }

//        monsterSpawner.Initialize(spawnPoints);
//    }

//    private void SetupClearCondition(StageData stageData)
//    {
//        var clearCondition = currentStage.GetComponent<StageClearCondition>();
//        if (clearCondition == null)
//        {
//            clearCondition = currentStage.AddComponent<StageClearCondition>();
//        }

//        clearCondition.Initialize(
//            stageData.clearCondition,
//            stageData.clearRequirement,
//            () => StageManager.Instance.OnStageClear()
//        );
//    }
//}