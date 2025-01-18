using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterAttackCollider : MonoBehaviour
{
    private MonsterClass monsterClass;
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
    {
        canDealDamage = true;
        //currentAttackType = isSkill ? AttackData.AttackType.Charge : AttackData.AttackType.Normal;
    }

    public void DisableDamage()
    {
        canDealDamage = false;
    }
}
