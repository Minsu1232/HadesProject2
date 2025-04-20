using UnityEngine;
using System;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Linq;
using System.Collections.Generic;

public class WeaponService : MonoBehaviour
{
    public Transform weaponMount;          // ���� �޼� ����Ʈ
    public Transform rightWeaponMount;     // ������ ����Ʈ
    private WeaponFactory weaponFactory;
    private WeaponManager currentWeapon;
    private Animator characterAnimator;
    private bool isChangingWeapon = false;

    // ���� ���� �̺�Ʈ
    public event Action<WeaponManager> OnWeaponChanged;
    public event Action<WeaponManager> OnWeaponUnequipped;

    private void Awake()
    {
        Debug.Log("WeaponService Awake - Adding WeaponFactory");
        weaponFactory = gameObject.AddComponent<WeaponFactory>();
        characterAnimator = GetComponent<Animator>();
        if (weaponFactory == null)
        {
            Debug.LogError("Failed to create WeaponFactory!");
        }
    }

    private void Initialize()
    {
       
        

        //if (weaponMount == null)
        //{
        //    weaponMount = transform.Find("WeaponMount");
        //    if (weaponMount == null)
        //    {
        //        Debug.LogError("WeaponMount not found. Please assign it in the inspector.");
        //    }
        //}
    }

    public async Task<bool> EquipWeapon(string weaponName)
    {
        // �̹� ���� ���⸦ ���������� Ȯ��
        if (currentWeapon != null && currentWeapon.WeaponName == weaponName)
        {
            Debug.Log($"Already equipped the same weapon: {weaponName}");
            return false;
        }

        if (isChangingWeapon)
        {
            Debug.LogWarning("Weapon change already in progress");
            return false;
        }
        isChangingWeapon = true;
        try
        {
            // ���� ���� ���� - �񵿱� ��� �߰�
            await UnequipCurrentWeapon();

            // �� ���� ���� �� ����
            var newWeapon = await weaponFactory.CreateWeapon(weaponName, gameObject.transform);
            if (newWeapon == null)
            {
                Debug.LogError($"Failed to create weapon: {weaponName}");
                return false;
            }
            // ���� �ʱ�ȭ �� ����
            currentWeapon = newWeapon;
            await InitializeNewWeapon(currentWeapon);
            OnWeaponChanged?.Invoke(currentWeapon);
            Debug.Log($"Successfully equipped weapon: {weaponName}");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error equipping weapon: {e.Message}");
            return false;
        }
        finally
        {
            isChangingWeapon = false;
        }
    }
    public async Task UnequipCurrentWeapon()
    {
        if (currentWeapon == null)
        {
            Debug.Log("������ ���Ⱑ �����ϴ�.");
            return;
        }

        string weaponName = currentWeapon.WeaponName;
        Debug.Log($"���� ���� ����: {weaponName}");

        // 1. �ִϸ��̼� ��Ʈ�ѷ� �񵿱� �ε� �� �Ϸ� ���
        await LoadDefaultAnimatorControllerAsync();

        // 2. DamageDealer ������Ʈ ã��
        var damageDealers = new List<MonoBehaviour>();
        foreach (var mb in gameObject.GetComponentsInChildren<MonoBehaviour>())
        {
            if (mb is IDamageDealer)
            {
                damageDealers.Add(mb);
            }
        }
        
       

        // 3. ���� ���� �� �̺�Ʈ �߻�
        OnWeaponUnequipped?.Invoke(currentWeapon);

        // CharacterAttackBase������ ���� ���� ����
        var attackBase = GetComponent<CharacterAttackBase>();
        if (attackBase != null)
        {
            attackBase.UnequipWeapon();
        }
        // PlayerClass������ ���� ���� ����
        if (GameInitializer.Instance != null && GameInitializer.Instance.GetPlayerClass() != null)
        {
            GameInitializer.Instance.GetPlayerClass().ClearWeapon();
        }

        currentWeapon = null;

        // 5. DamageDealer ������Ʈ ����
        foreach (var dealer in damageDealers)
        {
            if (dealer != null)
            {
                Debug.Log($"���� ���� DamageDealer ����: {dealer.name}");
                Destroy(dealer.gameObject);
            }
        }
        // 6. ���������� ���� ������Ʈ ��ü ����
        var weapon = GetComponent<WeaponBase>();
        if(weapon != null)
        {
            Destroy(weapon);
        }
       
       
 


    }

    // �񵿱� �ִϸ����� ��Ʈ�ѷ� �ε� �޼���
    private async Task LoadDefaultAnimatorControllerAsync()
    {
        Debug.Log("�⺻ �ִϸ����� ��Ʈ�ѷ� �ε� ����...");

        if (characterAnimator == null)
        {
            Debug.LogWarning("Animator�� null�Դϴ�. �⺻ �ִϸ��̼��� �ε����� �ʽ��ϴ�.");
            return;
        }

        try
        {
            var handle = Addressables.LoadAssetAsync<RuntimeAnimatorController>("VillageAnimationController");
            await handle.Task;

            if (handle.Status == AsyncOperationStatus.Succeeded && handle.Result != null)
            {
                // �ε� ���� �� ����
                if (characterAnimator != null) // �ٽ� üũ
                {
                    characterAnimator.runtimeAnimatorController = handle.Result;
                    Debug.Log("�⺻ �ִϸ����� ��Ʈ�ѷ� �ε� �Ϸ�: VillageAnimationController");
                }
            }
            else
            {
                Debug.LogError($"�⺻ �ִϸ����� ��Ʈ�ѷ� �ε� ����: VillageAnimationController, ����: {handle.Status}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"�ִϸ����� ��Ʈ�ѷ� �ε� �� ���� �߻�: {e.Message}");
        }
    }

    private async Task InitializeNewWeapon(WeaponManager weapon)
    {
        if (weapon == null) return;

        weapon.InitializeWeapon(characterAnimator);
        await Task.Yield(); // �ʱ�ȭ�� ���� ������ ���
        weapon.WeaponLoad(weaponMount); 
    }
    public void ClearWeaponReferences()
    {
        currentWeapon = null;
        isChangingWeapon = false;
        Debug.Log("��� ���� ������ �ʱ�ȭ�Ǿ����ϴ�.");
    }
    public WeaponManager GetCurrentWeapon() => currentWeapon;

    // ���� ������ ���� Ȯ���� ���� ��ƿ��Ƽ �޼����
    public bool HasWeaponEquipped() => currentWeapon != null;

    public string GetCurrentWeaponName()
    {
        return currentWeapon != null ? currentWeapon.WeaponName : "None";
    }

    private void OnDestroy()
    {
        // �̺�Ʈ ���� ���� �� ����
        if (currentWeapon != null)
        {
            Destroy(currentWeapon);
        }
    }
}