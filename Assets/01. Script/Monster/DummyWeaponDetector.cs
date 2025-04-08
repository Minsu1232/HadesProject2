//// 무기 데미지 감지용 스크립트
//using UnityEngine;

//public class DummyWeaponDetector : MonoBehaviour
//{
//    private TrainingDummy dummyParent;

//    private void Awake()
//    {
//        dummyParent = GetComponent<TrainingDummy>();
//    }

//    private void OnTriggerEnter(Collider other)
//    {
//        Debug.Log("충돌");
//        // 무기 콜라이더 감지 로직
//        MeleeDamageDealer damageDealer = other.GetComponent<MeleeDamageDealer>();
//        if (damageDealer != null)
//        {
//            Debug.Log("충돌 후 들어옴");
//            // 데미지 계산
//            int damage = damageDealer.GetDamage();

//            // 데미지 적용
//            dummyParent.TakeDamage(damage, other.ClosestPoint(transform.position));
//        }
//    }
//}