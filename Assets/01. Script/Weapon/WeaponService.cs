using UnityEngine;
using System;
using System.Threading.Tasks;

public class WeaponService : MonoBehaviour
{
    [SerializeField] public Transform weaponMount;
    private WeaponFactory weaponFactory;
    private WeaponManager currentWeapon;
    private Animator characterAnimator;
    private bool isChangingWeapon = false;

    // 무기 변경 이벤트
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
        // 이미 같은 무기를 착용중인지 확인
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
            // 기존 무기 제거
            UnequipCurrentWeapon();
            // 새 무기 생성 및 설정
            var newWeapon = await weaponFactory.CreateWeapon(weaponName, gameObject.transform);
            if (newWeapon == null)
            {
                Debug.LogError($"Failed to create weapon: {weaponName}");
                return false;
            }
            // 무기 초기화 및 설정
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

    private void UnequipCurrentWeapon()
    { 
        if (currentWeapon != null)
        {
            OnWeaponUnequipped?.Invoke(currentWeapon);
            Destroy(currentWeapon);
            currentWeapon = null;
            
        }
    }

    private async Task InitializeNewWeapon(WeaponManager weapon)
    {
        if (weapon == null) return;

        weapon.InitializeWeapon(characterAnimator);
        await Task.Yield(); // 초기화를 위한 프레임 대기
        weapon.WeaponLoad(weaponMount);
    }

    public WeaponManager GetCurrentWeapon() => currentWeapon;

    // 현재 무기의 상태 확인을 위한 유틸리티 메서드들
    public bool HasWeaponEquipped() => currentWeapon != null;

    public string GetCurrentWeaponName()
    {
        return currentWeapon != null ? currentWeapon.WeaponName : "None";
    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제 및 정리
        if (currentWeapon != null)
        {
            Destroy(currentWeapon);
        }
    }
}