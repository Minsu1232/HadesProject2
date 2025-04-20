using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class Chronofracture : WeaponBase
{
    private MeleeDamageDealer damageDealer;
    private bool isSpecialAttacking = false;

    // 시간 잔상 효과 관련 변수
    private List<TimeResidue> timeResidues = new List<TimeResidue>();
    private float residueDuration = 0.5f; // 잔상 지속 시간
    public override PlayerClass.WeaponType weaponType => PlayerClass.WeaponType.Chronofracture;
    // 시간 메아리 관련 변수
    private Dictionary<IDamageable, int> echoStacks = new Dictionary<IDamageable, int>();
    private float echoDebuffDuration = 3.0f; // 메아리 지속 시간

    private GameObject _secondWeaponInstance; // 두 번째 검(오른손)
    private MeleeDamageDealer secondDamageDealer; // 두 번째 검의 데미지 딜러
    private Collider secondWeaponCollider; // 두 번째 검의 콜라이더

    // 두 개의 다른 잔상 프리팹 필드 선언
    [SerializeField] private GameObject blueTimeResidueEffectPrefab; // 파란색(과거) 검 잔상
    [SerializeField] private GameObject redTimeResidueEffectPrefab;  // 빨간색(미래) 검 잔상

    // 차지 공격용 검기 프리팹
    [SerializeField] private GameObject chargeBladeWavePrefab; // 차지 공격 시 사용할 검기
    public GameObject specialBladeWavePrefab; // 스페셜 공격 시 사용할 이펙트

    protected override void InitializeComponents()
    {
        if (_weaponInstance != null)
        {
            // 왼손 무기(빨간색) 초기화
            damageDealer = _weaponInstance.GetComponent<MeleeDamageDealer>();
            if (damageDealer == null)
            {
                damageDealer = _weaponInstance.AddComponent<MeleeDamageDealer>();
            }
            damageDealer.Initialize(this, 0);

            // 이벤트 연결
            damageDealer.OnFinalDamageCalculated += OnDamageDealt;
        }

        if (_secondWeaponInstance != null)
        {
            // 오른손 무기(파란색) 초기화
            secondDamageDealer = _secondWeaponInstance.GetComponent<MeleeDamageDealer>();
            if (secondDamageDealer == null)
            {
                secondDamageDealer = _secondWeaponInstance.AddComponent<MeleeDamageDealer>();
            }
            secondDamageDealer.Initialize(this, 0);
            secondDamageDealer.OnFinalDamageCalculated += OnDamageDealt;
        }
        chargeBladeWavePrefab = GameInitializer.Instance.chargeBladeWavePrefab;
        blueTimeResidueEffectPrefab = GameInitializer.Instance.chronoRightEffect;
        redTimeResidueEffectPrefab = GameInitializer.Instance.chronoLeftEffect;
        specialBladeWavePrefab = GameInitializer.Instance.chronoSpecialEffectPrefab;
        // 차지 및 특수공격 컴포넌트 초기화 (검기 프리팹 전달)
        chargeComponent = new ChronofractureCharge(this, chargeBladeWavePrefab);
        specialAttackComponent = new ChronofractureSpecialAttack(this);
    }

    // 데미지가 적용될 때 호출되는 이벤트 핸들러
    private void OnDamageDealt(int damage, ICreatureStatus target)
    {
        if (target is IDamageable damageable)
        {
            // 시간 메아리 스택 추가
            AddEchoStack(damageable);
        }
    }

    // 시간 메아리 스택 추가 메서드 (외부에서도 호출 가능하도록 public으로 변경)
    public void AddEchoStackToTarget(IDamageable target)
    {
        AddEchoStack(target);
    }

    public override void OnAttack(Transform origin, int comboStep)
    {
        if (damageDealer != null)
        {
            damageDealer.Initialize(this, comboStep);
        }

        if (secondDamageDealer != null)
        {
            secondDamageDealer.Initialize(this, comboStep);
        }

        //// 공격 종류와 콤보에 따라 각 검의 잔상 생성
        //if (IsChargeAttack)
        //{
        //    // 차지 공격은 양손 모두 잔상 생성
        //    CreateTimeResidue(origin.position, origin.rotation, comboStep, true); // 왼손
        //    CreateTimeResidue(origin.position, origin.rotation, comboStep, false); // 오른손
        //}
        //else
        //{
        //    // 일반 공격은 콤보 숫자에 따라 번갈아가며 잔상 생성
        //    bool isLeftBlade = (comboStep % 2 == 1); // 홀수 콤보는 왼손, 짝수 콤보는 오른손
        //    CreateTimeResidue(origin.position, origin.rotation, comboStep, isLeftBlade);
        //}
    }

    // 시간 잔상 생성 메서드
    private void CreateTimeResidue(Vector3 position, Quaternion rotation, int comboStep, bool isLeftBlade)
    {
        // 검 종류에 따라 적절한 프리팹 선택
        GameObject prefabToUse = isLeftBlade ? redTimeResidueEffectPrefab : blueTimeResidueEffectPrefab;

        if (prefabToUse == null)
        {
            Debug.LogError("시간 잔상 프리팹이 설정되지 않았습니다!");
            return;
        }

        // 프리팹 인스턴스화
        GameObject residueObj = Instantiate(prefabToUse, position, rotation);

        // TimeResidue 컴포넌트 가져오기 또는 추가
        TimeResidue residue = residueObj.GetComponent<TimeResidue>();
        if (residue == null)
        {
            residue = residueObj.AddComponent<TimeResidue>();
        }

        // 데미지 딜러 선택
        float damage;
        if (isLeftBlade)
        {
            // 왼손(빨간색) 검의 데미지
            damage = damageDealer != null ? damageDealer.GetDamage() * 0.3f : 5f;
        }
        else
        {
            // 오른손(파란색) 검의 데미지
            damage = secondDamageDealer != null ? secondDamageDealer.GetDamage() * 0.3f : 5f;
        }

        // 초기화
        residue.Initialize(this, residueDuration, comboStep, damage);
        timeResidues.Add(residue);

        // 콤보에 따른 색상 변경 - 각 검마다 다른 색상 범위 사용
        float intensity = Mathf.Clamp01((float)comboStep / 5f);
        if (isLeftBlade)
        {
            // 빨간색 검은 빨간색에서 밝은 빨간색으로 변화
            residue.SetColor(Color.Lerp(Color.red, new Color(1f, 0.5f, 0.5f), intensity));
        }
        else
        {
            // 파란색 검은 파란색에서 청록색으로 변화
            residue.SetColor(Color.Lerp(Color.blue, Color.cyan, intensity));
        }
    }

    // 시간 메아리 스택 추가 메서드
    private void AddEchoStack(IDamageable target)
    {
        if (!echoStacks.ContainsKey(target))
        {
            echoStacks[target] = 0;
        }

        // 최대 3스택까지만 중첩
        if (echoStacks[target] < 3)
        {
            echoStacks[target]++;

            // 대상이 ICreatureStatus를 구현한다면 디버프 적용
            if (target is ICreatureStatus creature)
            {
                // 각 스택당 15% 이동속도 감소, 15% 데미지 증가
                float slowAmount = 0.15f * echoStacks[target];
                float damageAmp = 0.15f * echoStacks[target];

                // 디버프 적용 (구현 필요)
                // creature.ApplyDebuff(slowAmount, damageAmp, echoDebuffDuration);

                Debug.Log($"시간 메아리 적용: {echoStacks[target]} 스택 (감속: {slowAmount * 100}%, 데미지 증폭: {damageAmp * 100}%)");
            }
        }
    }

    public override void SpecialAttack()
    {
        // 게이지가 100% 찼는지 확인
        if (CurrentGage < 100)
        {
            Debug.Log($"특수 공격 게이지가 부족합니다: {CurrentGage}/100");
            return;
        }

        Debug.Log("시간 융합 타격 특수 스킬 발동!");
        if (specialAttackComponent != null)
        {
            specialAttackComponent.Execute();
            // 게이지 소모 (specialAttackComponent의 WeaponresetGage 값 사용)
            ResetGage(specialAttackComponent.WeaponresetGage);
        }
        else
        {
            Debug.LogError("특수 공격 컴포넌트가 초기화되지 않았습니다.");
        }
    }

    // 애니메이션 이벤트
    public void PlayVFX()
    {
        if (specialAttackComponent != null)
        {
            specialAttackComponent.PlayVFX();
            specialAttackComponent.PlaySound();
        }
    }

    public MeleeDamageDealer ReturnDealer()
    {
        return damageDealer;
    }

    public MeleeDamageDealer ReturnSecondDealer()
    {
        return secondDamageDealer;
    }

    public override GameObject WeaponLoad(Transform parentTransform)
    {
        // 왼손 무기 생성 (기존 weaponMount는 왼손)
        Transform leftHand = parentTransform; // 기존 weaponMount

        // 오른손 마운트 찾기
        Transform rightHand = null;
        WeaponService weaponService = GameInitializer.Instance.GetWeaponService();
        if (weaponService != null && weaponService.rightWeaponMount != null)
        {
            rightHand = weaponService.rightWeaponMount;
        }
        else
        {
            Debug.LogWarning("오른손 마운트를 찾을 수 없습니다. 쌍검을 한 손에만 장착합니다.");
        }

        // 왼손 무기(빨간색) 로드 - 기존 마운트에 장착
        Addressables.LoadAssetAsync<GameObject>("ChronofractureRed").Completed += handle =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded && handle.Result != null)
            {
                _weaponInstance = Instantiate(handle.Result, leftHand);

                // 스크린샷 기준 위치 적용 (빨간 검 위치)
                _weaponInstance.transform.localPosition = new Vector3(13.3f, -10.7f, 21.1f);
                _weaponInstance.transform.localEulerAngles = new Vector3(-49.46f, 106.755f, -245.507f);

                // 콜라이더 설정
                weaponCollider = _weaponInstance.GetComponent<Collider>();
                if (weaponCollider != null)
                {
                    weaponCollider.enabled = false;
                    weaponCollider.isTrigger = true;
                }

                // 첫 번째 무기 초기화
                InitializeFirstWeapon();

                // 오른손 무기 로드
                if (rightHand != null)
                {
                    LoadRightWeapon(rightHand);
                }
                else
                {
                    // 오른손 마운트가 없는 경우 컴포넌트 초기화
                    InitializeComponents();
                }
            }
            else
            {
                Debug.LogError("ChronofractureRed 무기 모델 로드 실패!");
            }
        };

        return _weaponInstance;
    }

    private void LoadRightWeapon(Transform rightHand)
    {
        // 오른손 무기(파란색) 로드
        Addressables.LoadAssetAsync<GameObject>("ChronofractureBlue").Completed += handle =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded && handle.Result != null)
            {
                _secondWeaponInstance = Instantiate(handle.Result, rightHand);

                // 스크린샷 기준 위치 적용 (파란 검 위치)
                _secondWeaponInstance.transform.localPosition = new Vector3(-5f, 5.5f, -13.2f);
                _secondWeaponInstance.transform.localEulerAngles = new Vector3(-43.557f, 109.631f, -78.784f);

                // 콜라이더 설정
                secondWeaponCollider = _secondWeaponInstance.GetComponent<Collider>();
                if (secondWeaponCollider != null)
                {
                    secondWeaponCollider.enabled = false;
                    secondWeaponCollider.isTrigger = true;
                }

                // 두 번째 무기 초기화
                InitializeSecondWeapon();

                // 모든 무기 로드 완료 후 컴포넌트 초기화
                InitializeComponents();
            }
            else
            {
                Debug.LogError("ChronofractureBlue 무기 모델 로드 실패!");
            }
        };
    }

    private void InitializeFirstWeapon()
    {
        // 첫 번째 검(빨간색) 초기화
        damageDealer = _weaponInstance.GetComponent<MeleeDamageDealer>();
        if (damageDealer == null)
        {
            damageDealer = _weaponInstance.AddComponent<MeleeDamageDealer>();
        }
        damageDealer.Initialize(this, 0);
        damageDealer.OnFinalDamageCalculated += OnDamageDealt;
    }

    private void InitializeSecondWeapon()
    {
        // 두 번째 검(파란색) 초기화
        secondDamageDealer = _secondWeaponInstance.GetComponent<MeleeDamageDealer>();
        if (secondDamageDealer == null)
        {
            secondDamageDealer = _secondWeaponInstance.AddComponent<MeleeDamageDealer>();
        }
        secondDamageDealer.Initialize(this, 0);
        secondDamageDealer.OnFinalDamageCalculated += OnDamageDealt;
    }

    // 애니메이션 이벤트에서 호출될 메서드들
    public void ActivateLeftCollider()
    {
        if (weaponCollider != null)
        {
            weaponCollider.enabled = true;
        }
    }

    public void DeactivateLeftCollider()
    {
        if (weaponCollider != null)
        {
            weaponCollider.enabled = false;
            if (damageDealer != null)
            {
                damageDealer.ClearDamagedMonsters();
            }
        }
    }

    public void ActivateRightCollider()
    {
        if (secondWeaponCollider != null)
        {
            secondWeaponCollider.enabled = true;
        }
    }

    public void DeactivateRightCollider()
    {
        if (secondWeaponCollider != null)
        {
            secondWeaponCollider.enabled = false;
            if (secondDamageDealer != null)
            {
                secondDamageDealer.ClearDamagedMonsters();
            }
        }
    }

    public override void ActivateCollider()
    {
        // 양손 콜라이더 모두 활성화 (특수 공격 등에서 사용)
        ActivateLeftCollider();
        ActivateRightCollider();
    }

    public override void DeactivateCollider()
    {
        // 양손 콜라이더 모두 비활성화
        DeactivateLeftCollider();
        DeactivateRightCollider();
    }

    // 왼손 잔상 생성 메서드 (애니메이션 이벤트에서 호출)
    public void CreateLeftBladeResidue()
    {
        if (weaponCollider != null && _weaponInstance != null)
        {
            CreateTimeResidue(_weaponInstance.transform.position, _weaponInstance.transform.rotation, 0, true);
        }
    }

    // 오른손 잔상 생성 메서드 (애니메이션 이벤트에서 호출)
    public void CreateRightBladeResidue()
    {
        if (secondWeaponCollider != null && _secondWeaponInstance != null)
        {
            CreateTimeResidue(_secondWeaponInstance.transform.position, _secondWeaponInstance.transform.rotation, 0, false);
        }
    }

    // 메모리 정리
    private void OnDestroy()
    {
        // 잔상 객체들 정리
        foreach (var residue in timeResidues)
        {
            if (residue != null)
            {
                Destroy(residue.gameObject);
            }
        }
        timeResidues.Clear();

        // 이벤트 해제
        if (damageDealer != null)
        {
            damageDealer.OnFinalDamageCalculated -= OnDamageDealt;
        }

        if (secondDamageDealer != null)
        {
            secondDamageDealer.OnFinalDamageCalculated -= OnDamageDealt;
        }
    }
}