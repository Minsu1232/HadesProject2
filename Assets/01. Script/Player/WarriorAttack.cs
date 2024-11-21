

//using UnityEngine;

//public class WarriorAttack : CharacterAttackBase
//{


//    public override void SkillAttack(int skillIndex)
//    {
//        Debug.Log($"Warrior 스킬 {skillIndex} 사용!");
//        // Warrior의 스킬 로직 (ex: 전사 특유의 분노 증가)
//    }
//    public void ActivateCollider()
//    {
//        currentWeapon?.ActivateCollider();
//    }
    
//    public void DeactivateCollider()
//    {
//        currentWeapon.DeactivateCollider();
//    }
//    public void ComboStepUpdate(int step)
//    {
//        // comboStep을 업데이트
//        comboStep = step;       

//        // 현재 comboStep에 맞춰 공격 실행
//        if(currentWeapon != null)
//        {
//            currentWeapon.OnAttack(transform, comboStep);
//        }
        
//    }
//}
