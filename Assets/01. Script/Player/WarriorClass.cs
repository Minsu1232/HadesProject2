//using System;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using DG.Tweening;

//public class WarriorClass : PlayerClass
//{
//    private int rage; // 분노 게이지, 공격 시 점점 증가해 스킬 강화에 사용 가능

  

//    private WeaponType selectWeapon = WeaponType.None;
//    public WarriorClass(PlayerClassData data, ICharacterAttack characterAttack, Rigidbody rb, Transform playerTransform, Animator animator)
//        : base(data,characterAttack ,rb, playerTransform, animator) 
//    {
//        rage = 0;
//    }
//    public override void LevelUp()
//    {
       
//    }
//    public override void Attack()
//    {
//        Debug.Log("Warrior가 근접 공격을 합니다!");
//        // 근접 공격 로직 추가
//        ModifyPower(attackAmount: (rage / 10)); // 분노 게이지를 추가 피해로 활용
//        rage += 5; // 공격 시 분노 증가
//    }

//    // Warrior의 스킬 사용 방식
//    public override void UseSkill(int index)
//    {
       
//    }
  
//    // Warrior만의 방어력 강화
//    public override void TakeDamage(int damage)
//    {        
//        base.TakeDamage(damage); // 피해 감소 후 처리
//        Debug.Log($"Warrior가 {damage} 피해를 받았습니다!");
//    }
//    public override void Dash(Vector3 direction)
//    {
//        if (isDashing) return;
//        isDashing = true;

//        Vector3 dashTarget = rb.position + direction; // 최종 대시 목표 설정
       

//        rb.DOMove(dashTarget, 0.2f)
//            .SetEase(Ease.OutQuad)
//            .OnComplete(() =>
//            {
               
//                isDashing = false;
//            });
//    }
//    public override void Die()
//    {
//        base.Die();
//        Debug.Log("Warrior가 전사했습니다. 전용 죽음 이펙트를 표시합니다.");
//        // Warrior만의 죽음 처리 로직 추가 가능
//    }
//}
