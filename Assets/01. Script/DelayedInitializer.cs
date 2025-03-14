//// 지연 초기화를 위한 헬퍼 클래스
//using System;
//using System.Collections;
//using UnityEngine;

//public class DelayedInitializer : MonoBehaviour
//{
//    private MonsterFactoryBase factory;
//    private GameObject monsterObject;
//    private ICreatureData data;
//    private Action<IMonsterClass> onMonsterCreated;

//    public void Initialize(MonsterFactoryBase factory, GameObject monsterObject, ICreatureData data, Action<IMonsterClass> onMonsterCreated)
//    {
//        this.factory = factory;
//        this.monsterObject = monsterObject;
//        this.data = data;
//        this.onMonsterCreated = onMonsterCreated;

//        // 1프레임 후에 초기화 완료
//        StartCoroutine(DelayedInit());
//    }

//    private IEnumerator DelayedInit()
//    {
//        // 한 프레임 대기
//        yield return null;

//        // 초기화 완료
//        factory.CompleteMonsterInitialization(monsterObject, data, onMonsterCreated);

//        // 이 컴포넌트는 더 이상 필요 없음
//        Destroy(this);
//    }
//}