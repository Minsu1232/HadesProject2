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
        weaponSelectionPanel.SetActive(true); // 무기 선택 UI 활성화
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
                    Debug.LogError($"{typeof(T).Name} 무기를 생성하는 데 실패했습니다.");
                    return;
                }

                weapon.weaponData = weaponData;
                GameInitializer.Instance.EquipWeapon(weapon);
                Debug.Log($"무기 데이터 로드 및 장착 완료: {weaponData.weaponName}");
            }
            else
            {
                Debug.LogError($"{weaponDataAddress} 무기 데이터 로드에 실패했습니다.");
            }
        };
    }

    // 각 무기별 장착 메서드
    public void EquipGreatSword()
    {
        LoadWeaponAndEquip<GreatSword>("GreatSwordData");
    }
}
