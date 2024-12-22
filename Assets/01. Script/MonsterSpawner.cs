//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;

//public class MonsterSpawner : MonoBehaviour
//{
//    private List<SpawnPoint> spawnPoints;
//    private List<Coroutine> activeSpawnRoutines = new List<Coroutine>();

//    public void Initialize(List<SpawnPoint> points)
//    {
//        spawnPoints = points;
//        StartSpawning();
//    }

//    private void StartSpawning()
//    {
//        foreach (var spawnPoint in spawnPoints)
//        {
//            var routine = StartCoroutine(SpawnMonsterWithDelay(spawnPoint));
//            activeSpawnRoutines.Add(routine);
//        }
//    }

//    private IEnumerator SpawnMonsterWithDelay(SpawnPoint point)
//    {
//        yield return new WaitForSeconds(point.spawnDelay);

//        if (this != null)
//        {
//            MonsterManager.Instance.SpawnMonster(point.monsterId, point.position);
//        }
//    }

//    private void OnDestroy()
//    {
//        foreach (var routine in activeSpawnRoutines)
//        {
//            if (routine != null)
//            {
//                StopCoroutine(routine);
//            }
//        }
//        activeSpawnRoutines.Clear();
//    }
//}