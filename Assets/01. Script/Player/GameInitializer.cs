using Sirenix.OdinInspector;
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

    [SerializeField] private PlayerClassData playerClassData;
    [SerializeField] private Transform weaponMount;

    private WeaponService weaponService;
    //[ShowInInspector]
    [SerializeField]
    private PlayerClass playerClass;

    private ICharacterAttack characterAttack;
    private Animator animator;

    private IDamageable damageable;

    private void Awake()
    {
        LoadGameData();
        InitializeComponents();
    }

    private void LoadGameData()
    {
        string filePath = $"{Application.persistentDataPath}/SaveFiles/playerData.json";
        Debug.Log($"Loading game data from: {filePath}");
        DataManager.Instance.LoadPlayerDataFromJson(filePath, playerClassData);
    }

    private void InitializeComponents()
    {
        try
        {
            animator = GetComponent<Animator>();
            characterAttack = GetComponent<ICharacterAttack>();

            if (animator == null || characterAttack == null)
            {
                throw new System.Exception("Required components are missing!");
            }

            InitializeWeaponService();
            InitializePlayerClass();

            Debug.Log("All components initialized successfully");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to initialize components: {e.Message}");
        }
    }

    private void InitializeWeaponService()
    {
        weaponService = gameObject.AddComponent<WeaponService>();
        weaponService.OnWeaponChanged += OnWeaponChanged;
        weaponService.weaponMount = weaponMount;
    }

    private void InitializePlayerClass()
    {
        playerClass = new PlayerClass(playerClassData, characterAttack, transform, animator);
        damageable = playerClass;
        if(damageable != null) Debug.Log(damageable.ToString());

        Debug.Log($"Initialized player class: {playerClassData.name}");
    }

    private void OnWeaponChanged(WeaponManager weapon)
    {
        characterAttack?.EquipWeapon(weapon);
        playerClass?.SelectWeapon(weapon);
    }

    public WeaponService GetWeaponService() => weaponService;
    public PlayerClass GetPlayerClass() => playerClass;

    private void OnDestroy()
    {
        if (weaponService != null)
        {
            weaponService.OnWeaponChanged -= OnWeaponChanged;
        }
    }
}
