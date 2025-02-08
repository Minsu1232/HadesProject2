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
                canDealDamage = false;  // 한 번만 데미지
                // 공격 타입에 따라 다른 데미지 적용
                IAttackStrategy strategy = CreatureAI.GetAttackStrategy();
                //실제 공격 실행

                strategy.ApplyDamage(player,monsterClass);
               
                
            }
        }
    }

    // 애니메이션에서 호출할 메서드
    public void EnableDamage()
    {  //  현재 몬스터의 공격 전략을 가져와 lastAttackTime을 갱신
    //    IAttackStrategy strategy = CreatureAI.GetAttackStrategy();
    //    if (strategy is BasePhysicalAttackStrategy physicalAttackStrategy)
    //    {
    //        physicalAttackStrategy.UpdateLastAttackTime(); // 공격 시작 시 타이머 갱신
    //    }
        canDealDamage = true;
        //currentAttackType = isSkill ? AttackData.AttackType.Charge : AttackData.AttackType.Normal;
    }

    public void DisableDamage()
    {
        canDealDamage = false;
      
    }
}
