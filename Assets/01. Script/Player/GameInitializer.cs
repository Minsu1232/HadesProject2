using System.Collections;
using System.Collections.Generic;
using System.Net;
using Unity.VisualScripting;
using UnityEngine;
/// <summary>
/// 게임 시작시 플레이어 캐릭터 데이터를 유니티 라이프 사이클과 연동하기 위한 스크립트
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
        // JSON 데이터를 불러와 testData에 적용
        string filePath = Application.persistentDataPath + "/SaveFiles/playerData.json";
        Debug.Log("파일 경로: " + filePath);
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

        // PlayerClassData를 통해 PlayerClass를 직접 생성
        playerClass = new PlayerClass(testData, characterAttack, rb, playerTransform, animator);

        Debug.Log($"Initialized class: {playerClass._playerClassData.name}");
    }

    public void EquipWeapon(IWeapon weapon)
    {
        if (characterAttack == null)
        {
            Debug.LogError("characterAttack이 초기화되지 않았습니다.");
            return;
        }
        if (weapon == null)
        {
            Debug.LogError("전달된 weapon이 null입니다.");
            return;
        }

        Debug.Log($"장착된 무기: {weapon.GetType().Name} 초기화 시작");

        if (currentWeapon != null && currentWeapon is Component currentWeaponComponent)
        {
            Destroy(currentWeaponComponent);
            Debug.Log("기존 무기 제거 완료");
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
            Debug.LogError("무기 컴포넌트를 Player에 추가하는 데 실패했습니다.");
        }
    }

    public IWeapon GetCurrentWeapon() => currentWeapon;
    public PlayerClass GetPlayerClass() => playerClass;
}
