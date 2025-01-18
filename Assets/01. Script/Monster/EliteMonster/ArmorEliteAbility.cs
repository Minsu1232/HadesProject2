//using static AttackData;

//public class ArmorEliteAbility : IEliteAbility
//{
//    public string AbilityName => "강화 방어";
//    public string Description => "방어력이 30% 증가";

//    private const float DEFENSE_BOOST = 0.3f;
   

//    public void ApplyAbility(MonsterStatus monsterStatus)
//    {
//        // 기본 방어력 30% 증가
//        int defenseBoost = (int)(monsterStatus.CurrentDefense * DEFENSE_BOOST);
//        monsterStatus.ModifyDefense(defenseBoost);       

//        // 시각적 효과 (예: 크기 약간 증가)
//        monsterStatus.transform.localScale *= 1.2f;
//    }

//    public void OnHit(MonsterStatus monsterStatus, int damage, AttackType attackType)
//    {
//        // 아머가 있을 때는 히트 모션 무시
//        if (monsterStatus.GetMonsterClass().CurrentArmor > 0)
//        {
//            var CreatureAI = monsterStatus.GetComponent<CreatureAI>();
//            if (CreatureAI != null)
//            {
//                // 피격 모션 재생 안함
//                return;
//            }
//        }
//    }

//    public void OnAttack(MonsterStatus monsterStatus)
//    {
//        // 필요한 경우 공격시 효과 구현
//    }

//    public void OnDeath(MonsterStatus monsterStatus)
//    {
//        // 사망시 효과 구현
//    }
//}