using System.Collections;
using System.Collections.Generic;
using System.Net;
using Unity.VisualScripting;
using UnityEngine;
/// <summary>
/// ���� ���۽� �÷��̾� ĳ���� �����͸� ����Ƽ ������ ����Ŭ�� �����ϱ� ���� ��ũ��Ʈ
/// </summary>
public class GameInitializer : Singleton<GameInitializer>
{

    private PlayerClass playerClass;
    private ICharacterAttack characterAttack;
    [SerializeField] private Transform weaponTransform;
    private IWeapon currentWeapon;

    [SerializeField] private PlayerClassData testData;
    private Animator animator;


    private void Awake()
    {
        // JSON �����͸� �ҷ��� testData�� ����
        string filePath = Application.persistentDataPath + "/SaveFiles/playerData.json";
        Debug.Log("���� ���: " + filePath);
        DataManager.Instance.LoadPlayerDataFromJson(filePath, testData);

        InitializePlayer();
        currentWeapon = null;
    }

    private void InitializePlayer()
    {
        animator = GetComponent<Animator>();
        Rigidbody rb = gameObject.GetComponent<Rigidbody>() ?? gameObject.AddComponent<Rigidbody>();
        Transform playerTransform = transform;
        characterAttack = GetComponent<ICharacterAttack>();

        // PlayerClassData�� ���� PlayerClass�� ���� ����
        playerClass = new PlayerClass(testData, characterAttack, rb, playerTransform, animator);

        Debug.Log($"Initialized class: {playerClass._playerClassData.name}");
    }

    public void EquipWeapon(IWeapon weapon)
    {
        if (characterAttack == null)
        {
            Debug.LogError("characterAttack�� �ʱ�ȭ���� �ʾҽ��ϴ�.");
            return;
        }
        if (weapon == null)
        {
            Debug.LogError("���޵� weapon�� null�Դϴ�.");
            return;
        }

        Debug.Log($"������ ����: {weapon.GetType().Name} �ʱ�ȭ ����");

        if (currentWeapon != null && currentWeapon is Component currentWeaponComponent)
        {
            Destroy(currentWeaponComponent);
            Debug.Log("���� ���� ���� �Ϸ�");
        }

        currentWeapon = weapon;

        if (currentWeapon != null)
        {
            characterAttack.EquipWeapon(currentWeapon);
            currentWeapon.WeaponLoad(weaponTransform);
            playerClass.SelectWeapon(currentWeapon);

            
        }
        else
        {
            Debug.LogError("���� ������Ʈ�� Player�� �߰��ϴ� �� �����߽��ϴ�.");
        }
    }

    public IWeapon GetCurrentWeapon() => currentWeapon;
    public PlayerClass GetPlayerClass() => playerClass;
}
