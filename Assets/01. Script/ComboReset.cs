using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComboReset : StateMachineBehaviour
{
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        var characterAttack = animator.GetComponent<CharacterAttackBase>();
        if (characterAttack != null)
        {
            characterAttack.comboStep = 0;  // �̵� ���·� ������ �� comboStep �ʱ�ȭ
            Debug.Log("1�� �ʱ�ȭ��");
            
        }
    }
}
