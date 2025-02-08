using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterAttackCollider : MonoBehaviour
{
    private IMonsterClass monsterClass;
    private CreatureAI CreatureAI;
    private bool canDealDamage = false;
    PlayerClass player;
    Animator animator;
    private void Start()
    {   
        //animator = GetComponent<Animator>();
        CreatureAI = GetComponent<CreatureAI>();
        monsterClass = GetComponentInParent<MonsterStatus>().GetMonsterClass();
        player = GameInitializer.Instance.GetPlayerClass();

        //if(monsterClass is BossMonster)
        //{
        //    monsterClass.Get
        //}
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
                IAttackStrategy strategy = CreatureAI.GetAttackStrategy();
                //���� ���� ����

                strategy.ApplyDamage(player,monsterClass);
               
                
            }
        }
    }

    // �ִϸ��̼ǿ��� ȣ���� �޼���
    public void EnableDamage()
    {  //  ���� ������ ���� ������ ������ lastAttackTime�� ����
    //    IAttackStrategy strategy = CreatureAI.GetAttackStrategy();
    //    if (strategy is BasePhysicalAttackStrategy physicalAttackStrategy)
    //    {
    //        physicalAttackStrategy.UpdateLastAttackTime(); // ���� ���� �� Ÿ�̸� ����
    //    }
        canDealDamage = true;
        //currentAttackType = isSkill ? AttackData.AttackType.Charge : AttackData.AttackType.Normal;
    }

    public void DisableDamage()
    {
        canDealDamage = false;
      
    }
}
