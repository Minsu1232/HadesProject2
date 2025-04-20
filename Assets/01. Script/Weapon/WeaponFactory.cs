using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class WeaponFactory : MonoBehaviour
{
    private Dictionary<string, Type> weaponTypes;

    private void Awake()
    {
        InitializeWeaponTypes();
    }

    private void InitializeWeaponTypes()
    {
        Debug.Log("Initializing WeaponTypes Dictionary");
        weaponTypes = new Dictionary<string, Type>()
        {
            { "GreatSword", typeof(GreatSword) },
            { "Chronofracture", typeof(Chronofracture)}
        };

        foreach (var type in weaponTypes)
        {
            Debug.Log($"Registered weapon type: {type.Key} -> {type.Value.Name}");
        }
    }

    public async Task<WeaponManager> CreateWeapon(string weaponName, Transform parent)
    {
        try
        {
            Debug.Log($"Starting weapon creation for: {weaponName}");

            // ���� ������ �ε� �α�
            Debug.Log($"Loading weapon data for: {weaponName}Data");
            var weaponData = await LoadWeaponData($"{weaponName}Data");
            if (weaponData == null)
            {
                Debug.LogError($"Failed to load weapon data for {weaponName}Data - weaponData is null");
                return null;
            }

            // ���� Ÿ�� Ȯ�� �α�
            Debug.Log($"Checking weapon type for: {weaponName}");
            if (!weaponTypes.TryGetValue(weaponName, out Type weaponType))
            {
                Debug.LogError($"Unknown weapon type: {weaponName}. Registered types are: {string.Join(", ", weaponTypes.Keys)}");
                return null;
            }

            // ������Ʈ ���� �α�
            Debug.Log($"Creating weapon component of type: {weaponType.Name}");
            var weapon = parent.gameObject.AddComponent(weaponType) as WeaponManager;
            if (weapon == null)
            {
                Debug.LogError($"Failed to create weapon component for {weaponName} - AddComponent returned null");
                return null;
            }

            // ������ ���� �α�
            Debug.Log($"Setting weapon data for: {weaponName}");
            weapon.weaponData = weaponData;

            return weapon;
        }
        catch (Exception e)
        {
            Debug.LogError($"Weapon creation failed for {weaponName}:\nError: {e.Message}\nStack Trace: {e.StackTrace}");
            return null;
        }
    }

    private async Task<WeaponScriptableObject> LoadWeaponData(string address)
    {
        try
        {
            Debug.Log($"Starting to load weapon data from address: {address}");
            var handle = Addressables.LoadAssetAsync<WeaponScriptableObject>(address);
            await handle.Task;

            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"Failed to load weapon data at address: {address}, Status: {handle.Status}");
                return null;
            }

            if (handle.Result == null)
            {
                Debug.LogError($"Loaded weapon data is null for address: {address}");
                return null;
            }

            Debug.Log($"Successfully loaded weapon data for: {address}");
            return handle.Result;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading weapon data for {address}:\nError: {e.Message}\nStack Trace: {e.StackTrace}");
            return null;
        }
    }

    // ���ο� ���� Ÿ�� ����� ���� �޼���
    public void RegisterWeaponType(string weaponName, Type weaponType)
    {
        if (weaponType.IsSubclassOf(typeof(WeaponManager)))
        {
            weaponTypes[weaponName] = weaponType;
            Debug.Log($"Registered new weapon type: {weaponName}");
        }
        else
        {
            Debug.LogError($"Type {weaponType.Name} is not a valid WeaponManager subclass");
        }
    }
}