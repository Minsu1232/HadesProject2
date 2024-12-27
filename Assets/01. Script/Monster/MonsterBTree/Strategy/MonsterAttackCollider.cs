using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterAttackCollider : MonoBehaviour
{
    private MonsterClass monsterClass;
    private MonsterAI monsterAI;
    private bool canDealDamage = false;
    PlayerClass player;
    Animator animator;
    private void Start()
    {   
        //animator = GetComponent<Animator>();
        monsterAI = GetComponent<MonsterAI>();
        monsterClass = GetComponentInParent<MonsterStatus>().GetMonsterClass();
        player = GameInitializer.Instance.GetPlayerClass();
    }
  
    private void OnTriggerEnter(Collider other)
    {

        if (!canDealDamage) return;

        if (other.gameObject.CompareTag("Player"))
        {   
            
            if (player != null)
            {
                canDealDamage = false;  // �� ���� ������
                // ���� Ÿ�Կ� ���� �ٸ� ������ ����
                IAttackStrategy strategy = monsterAI.GetAttackStrategy();
                //���� ���� ����

                strategy.ApplyDamage(player,monsterClass);
               
                
            }
        }
    }

    // �ִϸ��̼ǿ��� ȣ���� �޼���
    public void EnableDamage()
    {
        canDealDamage = true;
        //currentAttackType = isSkill ? AttackData.AttackType.Charge : AttackData.AttackType.Normal;
    }

    public void DisableDamage()
    {
        canDealDamage = false;
    }
}
