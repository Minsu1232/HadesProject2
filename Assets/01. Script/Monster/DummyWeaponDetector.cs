//// ���� ������ ������ ��ũ��Ʈ
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
//        Debug.Log("�浹");
//        // ���� �ݶ��̴� ���� ����
//        MeleeDamageDealer damageDealer = other.GetComponent<MeleeDamageDealer>();
//        if (damageDealer != null)
//        {
//            Debug.Log("�浹 �� ����");
//            // ������ ���
//            int damage = damageDealer.GetDamage();

//            // ������ ����
//            dummyParent.TakeDamage(damage, other.ClosestPoint(transform.position));
//        }
//    }
//}