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
                canDealDamage = false;  // 한 번만 데미지
                // 공격 타입에 따라 다른 데미지 적용
                IAttackStrategy strategy = monsterAI.GetAttackStrategy();
                //실제 공격 실행

                strategy.ApplyDamage(player,monsterClass);
               
                
            }
        }
    }

    // 애니메이션에서 호출할 메서드
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
