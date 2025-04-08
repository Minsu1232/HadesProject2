using UnityEngine;
using System;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Linq;
using System.Collections.Generic;

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

    public void UnequipCurrentWeapon()
    {
        if (currentWeapon == null)
        {
            Debug.Log("장착된 무기가 없습니다.");
            return;
        }

        string weaponName = currentWeapon.WeaponName; // 로그용 이름 미리 저장
        Debug.Log($"무기 해제 시작: {weaponName}");

        // 1. 먼저 애니메이션 컨트롤러 비동기 로드 시작
        LoadDefaultAnimatorController();

        // 2. DamageDealer 컴포넌트 찾기 (무기 제거 전)
        var damageDealers = new List<MonoBehaviour>();
        foreach (var mb in gameObject.GetComponentsInChildren<MonoBehaviour>())
        {
            if (mb is IDamageDealer)
            {
                damageDealers.Add(mb);
            }
        }

        // 3. 무기 제거 전 이벤트 발생
        OnWeaponUnequipped?.Invoke(currentWeapon);

        // 4. 현재 무기 참조 로컬 변수에 저장 후 초기화
        WeaponManager weaponToDestroy = currentWeapon;
        currentWeapon = null; // 먼저 참조 제거

        // 5. DamageDealer 컴포넌트 제거
        foreach (var dealer in damageDealers)
        {
            if (dealer != null)
            {
                Debug.Log($"무기 관련 DamageDealer 제거: {dealer.name}");
                Destroy(dealer.gameObject);
            }
        }

        // 6. 마지막으로 무기 컴포넌트 자체 제거
        if (weaponToDestroy != null)
        {
            Destroy(weaponToDestroy);
            Debug.Log($"무기 해제 완료: {weaponName}");
        }
    }

    private void LoadDefaultAnimatorController()
    {
        if (characterAnimator == null)
        {
            Debug.LogWarning("Animator가 null입니다. 기본 애니메이션을 로드하지 않습니다.");
            return;
        }

        // 비동기 로드 시작
        Debug.Log("기본 애니메이터 컨트롤러 로드 시작...");
        Addressables.LoadAssetAsync<RuntimeAnimatorController>("VillageAnimationController").Completed += handle =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded && handle.Result != null)
            {
                // 로드 성공 시 적용
                if (characterAnimator != null) // 이중 체크
                {
                    characterAnimator.runtimeAnimatorController = handle.Result;
                    Debug.Log("기본 애니메이터 컨트롤러 로드 완료: VillageAnimationController");
                }
                else
                {
                    Debug.LogWarning("애니메이터 컨트롤러를 로드했으나 Animator가 이미 null입니다.");
                }
            }
            else
            {
                Debug.LogError($"기본 애니메이터 컨트롤러 로드 실패: VillageAnimationController, 상태: {handle.Status}");
            }
        };
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