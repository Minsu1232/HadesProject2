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
            Debug.LogError("Animator가 연결되지 않았습니다.");
        }
      
    }

    // 애니메이션 트리거 설정
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
        characterAnimator?.SetInteger(hash, value); // 해시를 이용한 설정
    }

    public int GetInteger(int hash)
    {
        return characterAnimator != null ? characterAnimator.GetInteger(hash) : 0; // 해시를 이용한 값 반환
    }

    public bool IsInState(string stateName)
    {
        if (characterAnimator == null) return false;
        return characterAnimator.GetCurrentAnimatorStateInfo(0).IsName(stateName);
    }
   
}
