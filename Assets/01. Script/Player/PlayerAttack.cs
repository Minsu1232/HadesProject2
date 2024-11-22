using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AttackData;

public class PlayerAttack : MonoBehaviour
{
    private CharacterAttackBase characterAttack;
    public System.Action<AttackType> OnAttackInput; // ���� Ÿ���� �����ϴ� �̺�Ʈ

    private void Start()
    {
        characterAttack = GetComponent<CharacterAttackBase>();
        if (characterAttack == null)
        {
            Debug.LogError("CharacterAttackBase�� Player�� ������� �ʾҽ��ϴ�.");
        }
    }

    private void Update()
    {
        HandleInput();

        // �� ������ ��¡ ������Ʈ ȣ��
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
        if (Input.GetKeyDown(KeyCode.Q)) // ��ų ��� �Է�
        {
            characterAttack?.SpecialAttack();            
        }
    }
}

  
