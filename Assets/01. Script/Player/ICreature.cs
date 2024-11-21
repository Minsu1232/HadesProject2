using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICreature
{
    public void Attack();  // 공격 메서드 정의    
    public void Die(); // 죽음 처리 메서드
}
