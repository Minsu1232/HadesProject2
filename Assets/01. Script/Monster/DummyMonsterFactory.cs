using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class DummyMonsterFactory : MonsterFactoryBase
{
    public override MonsterClass CreateMonster(Vector3 spawnPosition, Action<MonsterClass> onMonsterCreated)
    {
        Addressables.LoadAssetAsync<MonsterData>("MonsterData_1").Completed += handle =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                MonsterData monsterData = handle.Result;
                monsterData.monsterPrefab.InstantiateAsync(spawnPosition, Quaternion.identity).Completed += prefabHandle =>
                {
                    if (prefabHandle.Status == AsyncOperationStatus.Succeeded)
                    {
                        GameObject monsterObject = prefabHandle.Result;

                        // MonsterClass 인스턴스 생성
                        MonsterClass monster = new DummyMonster(monsterData);

                        // MonsterStatus 컴포넌트 추가 및 초기화
                        MonsterStatus monsterStatus = monsterObject.AddComponent<MonsterStatus>();
                        monsterStatus.Initialize(monster); // MonsterClass로 초기화

                        onMonsterCreated?.Invoke(monster);
                    }
                };
            } 
        };

        return null; // 비동기 처리가 완료될 때까지 반환할 값 없음
    }
}



