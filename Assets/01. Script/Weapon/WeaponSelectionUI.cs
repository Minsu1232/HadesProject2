using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class WeaponSelectionUI : MonoBehaviour
{
    public GameObject weaponSelectionPanel;
   [SerializeField] GameObject selectWeapon;

    public void ShowWeaponSelection()
    {
        weaponSelectionPanel.SetActive(true); // ���� ���� UI Ȱ��ȭ
    }

    private void LoadWeaponAndEquip<T>(string weaponDataAddress) where T : WeaponManager
    {
        Addressables.LoadAssetAsync<WeaponScriptableObject>(weaponDataAddress).Completed += handle =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded && handle.Result != null)
            {
                WeaponScriptableObject weaponData = handle.Result;

                T weapon = GameInitializer.Instance.gameObject.AddComponent<T>();
                if (weapon == null)
                {
                    Debug.LogError($"{typeof(T).Name} ���⸦ �����ϴ� �� �����߽��ϴ�.");
                    return;
                }

                weapon.weaponData = weaponData;
                GameInitializer.Instance.EquipWeapon(weapon);
                Debug.Log($"���� ������ �ε� �� ���� �Ϸ�: {weaponData.weaponName}");
            }
            else
            {
                Debug.LogError($"{weaponDataAddress} ���� ������ �ε忡 �����߽��ϴ�.");
            }
        };
    }

    // �� ���⺰ ���� �޼���
    public void EquipGreatSword()
    {
        LoadWeaponAndEquip<GreatSword>("GreatSwordData");
    }
}
