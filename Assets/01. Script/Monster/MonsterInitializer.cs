//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class MonsterInitializer : MonoBehaviour
//{
//    private MonsterClass monsterClass;
//    [SerializeField] private MonsterData testData; // 인스펙터에서 할당할 테스트용 데이터
//    // Start is called before the first frame update
//    void Start()
//    {
//        monsterClass = new DummyMonster(testData);
//        Debug.Log($"소환된 몬스터는 {testData.MONSTERNAME}");
//    }

//    // Update is called once per frame
//    void Update()
//    {
        
//    }
//    public MonsterClass GetPlayerClass()
//    {
//        return monsterClass; // 다른 스크립트가 playerClass에 접근할 수 있도록 제공
//    }
//}
