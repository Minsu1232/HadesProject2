//using static AttackData;

//public class ArmorEliteAbility : IEliteAbility
//{
//    public string AbilityName => "��ȭ ���";
//    public string Description => "������ 30% ����";

//    private const float DEFENSE_BOOST = 0.3f;
   

//    public void ApplyAbility(MonsterStatus monsterStatus)
//    {
//        // �⺻ ���� 30% ����
//        int defenseBoost = (int)(monsterStatus.CurrentDefense * DEFENSE_BOOST);
//        monsterStatus.ModifyDefense(defenseBoost);       

//        // �ð��� ȿ�� (��: ũ�� �ణ ����)
//        monsterStatus.transform.localScale *= 1.2f;
//    }

//    public void OnHit(MonsterStatus monsterStatus, int damage, AttackType attackType)
//    {
//        // �ƸӰ� ���� ���� ��Ʈ ��� ����
//        if (monsterStatus.GetMonsterClass().CurrentArmor > 0)
//        {
//            var CreatureAI = monsterStatus.GetComponent<CreatureAI>();
//            if (CreatureAI != null)
//            {
//                // �ǰ� ��� ��� ����
//                return;
//            }
//        }
//    }

//    public void OnAttack(MonsterStatus monsterStatus)
//    {
//        // �ʿ��� ��� ���ݽ� ȿ�� ����
//    }

//    public void OnDeath(MonsterStatus monsterStatus)
//    {
//        // ����� ȿ�� ����
//    }
//}