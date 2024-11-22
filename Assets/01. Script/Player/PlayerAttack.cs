using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AttackData;

public class PlayerAttack : MonoBehaviour
{
    private CharacterAttackBase characterAttack;
    public System.Action<AttackType> OnAttackInput; // 공격 타입을 전달하는 이벤트

    private void Start()
    {
        characterAttack = GetComponent<CharacterAttackBase>();
        if (characterAttack == null)
        {
            Debug.LogError("CharacterAttackBase가 Player에 연결되지 않았습니다.");
        }
    }

    private void Update()
    {
        HandleInput();

        // 매 프레임 차징 업데이트 호출
        characterAttack?.UpdateCharge(Time.deltaTime);
    }

    private void HandleInput()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            characterAttack?.BasicAttack();
            OnAttackInput?.Invoke(AttackType.Normal);
        }
        if (Input.GetButtonDown("Fire2"))
        {
            characterAttack?.ChargingAttack();
        }
        if (Input.GetButtonUp("Fire2"))
        {
            characterAttack?.ReleaseCharge();
            OnAttackInput?.Invoke(AttackType.Charge);
        }
        if (Input.GetKeyDown(KeyCode.Q)) // 스킬 사용 입력
        {
            characterAttack?.SpecialAttack();            
        }
    }
}

  
