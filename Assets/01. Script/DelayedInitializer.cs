//// ���� �ʱ�ȭ�� ���� ���� Ŭ����
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

//        // 1������ �Ŀ� �ʱ�ȭ �Ϸ�
//        StartCoroutine(DelayedInit());
//    }

//    private IEnumerator DelayedInit()
//    {
//        // �� ������ ���
//        yield return null;

//        // �ʱ�ȭ �Ϸ�
//        factory.CompleteMonsterInitialization(monsterObject, data, onMonsterCreated);

//        // �� ������Ʈ�� �� �̻� �ʿ� ����
//        Destroy(this);
//    }
//}