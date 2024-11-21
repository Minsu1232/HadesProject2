using RPGCharacterAnims.Lookups;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimationController : MonoBehaviour
{
    private Animator characterAnimator;

    private void Awake()
    {
        characterAnimator = GetComponent<Animator>();
        if (characterAnimator == null)
        {
            Debug.LogError("Animator�� ������� �ʾҽ��ϴ�.");
        }
      
    }

    // �ִϸ��̼� Ʈ���� ����
    public Animator GetAnimator()
    {
        return characterAnimator;
    }

    public void SetTrigger(string triggerName)
    {
        characterAnimator?.SetTrigger(triggerName);
    }

    public void SetInteger(int hash, int value)
    {
        characterAnimator?.SetInteger(hash, value); // �ؽø� �̿��� ����
    }

    public int GetInteger(int hash)
    {
        return characterAnimator != null ? characterAnimator.GetInteger(hash) : 0; // �ؽø� �̿��� �� ��ȯ
    }

    public bool IsInState(string stateName)
    {
        if (characterAnimator == null) return false;
        return characterAnimator.GetCurrentAnimatorStateInfo(0).IsName(stateName);
    }
   
}
