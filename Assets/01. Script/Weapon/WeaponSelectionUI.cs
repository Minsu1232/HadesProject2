using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class WeaponSelectionUI : MonoBehaviour
{
    [SerializeField] private GameObject weaponSelectionPanel;
    private WeaponService weaponService;

    private void Start()
    {
        weaponService = GameInitializer.Instance.GetWeaponService();
    }

    public void ShowWeaponSelection()
    {
        weaponSelectionPanel.SetActive(true);
    }

    public async void SelectWeapon(string weaponName)
    {
        if (weaponService == null)
        {
            Debug.LogError("WeaponService not found!");
            return;
        }

        bool success = await weaponService.EquipWeapon(weaponName);
        if (success)
        {
            //weaponSelectionPanel.SetActive(false);
        }
    }

    // 각 무기별 버튼에 연결할 메서드들
    public void OnGreatSwordSelected()
    {
        SelectWeapon("GreatSword");
    }
}
