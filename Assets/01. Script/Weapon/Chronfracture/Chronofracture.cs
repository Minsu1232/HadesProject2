using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class Chronofracture : WeaponBase
{
    private MeleeDamageDealer damageDealer;
    private bool isSpecialAttacking = false;

    // �ð� �ܻ� ȿ�� ���� ����
    private List<TimeResidue> timeResidues = new List<TimeResidue>();
    private float residueDuration = 0.5f; // �ܻ� ���� �ð�
    public override PlayerClass.WeaponType weaponType => PlayerClass.WeaponType.Chronofracture;
    // �ð� �޾Ƹ� ���� ����
    private Dictionary<IDamageable, int> echoStacks = new Dictionary<IDamageable, int>();
    private float echoDebuffDuration = 3.0f; // �޾Ƹ� ���� �ð�

    private GameObject _secondWeaponInstance; // �� ��° ��(������)
    private MeleeDamageDealer secondDamageDealer; // �� ��° ���� ������ ����
    private Collider secondWeaponCollider; // �� ��° ���� �ݶ��̴�

    // �� ���� �ٸ� �ܻ� ������ �ʵ� ����
    [SerializeField] private GameObject blueTimeResidueEffectPrefab; // �Ķ���(����) �� �ܻ�
    [SerializeField] private GameObject redTimeResidueEffectPrefab;  // ������(�̷�) �� �ܻ�

    // ���� ���ݿ� �˱� ������
    [SerializeField] private GameObject chargeBladeWavePrefab; // ���� ���� �� ����� �˱�
    public GameObject specialBladeWavePrefab; // ����� ���� �� ����� ����Ʈ

    protected override void InitializeComponents()
    {
        if (_weaponInstance != null)
        {
            // �޼� ����(������) �ʱ�ȭ
            damageDealer = _weaponInstance.GetComponent<MeleeDamageDealer>();
            if (damageDealer == null)
            {
                damageDealer = _weaponInstance.AddComponent<MeleeDamageDealer>();
            }
            damageDealer.Initialize(this, 0);

            // �̺�Ʈ ����
            damageDealer.OnFinalDamageCalculated += OnDamageDealt;
        }

        if (_secondWeaponInstance != null)
        {
            // ������ ����(�Ķ���) �ʱ�ȭ
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
        // ���� �� Ư������ ������Ʈ �ʱ�ȭ (�˱� ������ ����)
        chargeComponent = new ChronofractureCharge(this, chargeBladeWavePrefab);
        specialAttackComponent = new ChronofractureSpecialAttack(this);
    }

    // �������� ����� �� ȣ��Ǵ� �̺�Ʈ �ڵ鷯
    private void OnDamageDealt(int damage, ICreatureStatus target)
    {
        if (target is IDamageable damageable)
        {
            // �ð� �޾Ƹ� ���� �߰�
            AddEchoStack(damageable);
        }
    }

    // �ð� �޾Ƹ� ���� �߰� �޼��� (�ܺο����� ȣ�� �����ϵ��� public���� ����)
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

        //// ���� ������ �޺��� ���� �� ���� �ܻ� ����
        //if (IsChargeAttack)
        //{
        //    // ���� ������ ��� ��� �ܻ� ����
        //    CreateTimeResidue(origin.position, origin.rotation, comboStep, true); // �޼�
        //    CreateTimeResidue(origin.position, origin.rotation, comboStep, false); // ������
        //}
        //else
        //{
        //    // �Ϲ� ������ �޺� ���ڿ� ���� �����ư��� �ܻ� ����
        //    bool isLeftBlade = (comboStep % 2 == 1); // Ȧ�� �޺��� �޼�, ¦�� �޺��� ������
        //    CreateTimeResidue(origin.position, origin.rotation, comboStep, isLeftBlade);
        //}
    }

    // �ð� �ܻ� ���� �޼���
    private void CreateTimeResidue(Vector3 position, Quaternion rotation, int comboStep, bool isLeftBlade)
    {
        // �� ������ ���� ������ ������ ����
        GameObject prefabToUse = isLeftBlade ? redTimeResidueEffectPrefab : blueTimeResidueEffectPrefab;

        if (prefabToUse == null)
        {
            Debug.LogError("�ð� �ܻ� �������� �������� �ʾҽ��ϴ�!");
            return;
        }

        // ������ �ν��Ͻ�ȭ
        GameObject residueObj = Instantiate(prefabToUse, position, rotation);

        // TimeResidue ������Ʈ �������� �Ǵ� �߰�
        TimeResidue residue = residueObj.GetComponent<TimeResidue>();
        if (residue == null)
        {
            residue = residueObj.AddComponent<TimeResidue>();
        }

        // ������ ���� ����
        float damage;
        if (isLeftBlade)
        {
            // �޼�(������) ���� ������
            damage = damageDealer != null ? damageDealer.GetDamage() * 0.3f : 5f;
        }
        else
        {
            // ������(�Ķ���) ���� ������
            damage = secondDamageDealer != null ? secondDamageDealer.GetDamage() * 0.3f : 5f;
        }

        // �ʱ�ȭ
        residue.Initialize(this, residueDuration, comboStep, damage);
        timeResidues.Add(residue);

        // �޺��� ���� ���� ���� - �� �˸��� �ٸ� ���� ���� ���
        float intensity = Mathf.Clamp01((float)comboStep / 5f);
        if (isLeftBlade)
        {
            // ������ ���� ���������� ���� ���������� ��ȭ
            residue.SetColor(Color.Lerp(Color.red, new Color(1f, 0.5f, 0.5f), intensity));
        }
        else
        {
            // �Ķ��� ���� �Ķ������� û�ϻ����� ��ȭ
            residue.SetColor(Color.Lerp(Color.blue, Color.cyan, intensity));
        }
    }

    // �ð� �޾Ƹ� ���� �߰� �޼���
    private void AddEchoStack(IDamageable target)
    {
        if (!echoStacks.ContainsKey(target))
        {
            echoStacks[target] = 0;
        }

        // �ִ� 3���ñ����� ��ø
        if (echoStacks[target] < 3)
        {
            echoStacks[target]++;

            // ����� ICreatureStatus�� �����Ѵٸ� ����� ����
            if (target is ICreatureStatus creature)
            {
                // �� ���ô� 15% �̵��ӵ� ����, 15% ������ ����
                float slowAmount = 0.15f * echoStacks[target];
                float damageAmp = 0.15f * echoStacks[target];

                // ����� ���� (���� �ʿ�)
                // creature.ApplyDebuff(slowAmount, damageAmp, echoDebuffDuration);

                Debug.Log($"�ð� �޾Ƹ� ����: {echoStacks[target]} ���� (����: {slowAmount * 100}%, ������ ����: {damageAmp * 100}%)");
            }
        }
    }

    public override void SpecialAttack()
    {
        // �������� 100% á���� Ȯ��
        if (CurrentGage < 100)
        {
            Debug.Log($"Ư�� ���� �������� �����մϴ�: {CurrentGage}/100");
            return;
        }

        Debug.Log("�ð� ���� Ÿ�� Ư�� ��ų �ߵ�!");
        if (specialAttackComponent != null)
        {
            specialAttackComponent.Execute();
            // ������ �Ҹ� (specialAttackComponent�� WeaponresetGage �� ���)
            ResetGage(specialAttackComponent.WeaponresetGage);
        }
        else
        {
            Debug.LogError("Ư�� ���� ������Ʈ�� �ʱ�ȭ���� �ʾҽ��ϴ�.");
        }
    }

    // �ִϸ��̼� �̺�Ʈ
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
        // �޼� ���� ���� (���� weaponMount�� �޼�)
        Transform leftHand = parentTransform; // ���� weaponMount

        // ������ ����Ʈ ã��
        Transform rightHand = null;
        WeaponService weaponService = GameInitializer.Instance.GetWeaponService();
        if (weaponService != null && weaponService.rightWeaponMount != null)
        {
            rightHand = weaponService.rightWeaponMount;
        }
        else
        {
            Debug.LogWarning("������ ����Ʈ�� ã�� �� �����ϴ�. �ְ��� �� �տ��� �����մϴ�.");
        }

        // �޼� ����(������) �ε� - ���� ����Ʈ�� ����
        Addressables.LoadAssetAsync<GameObject>("ChronofractureRed").Completed += handle =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded && handle.Result != null)
            {
                _weaponInstance = Instantiate(handle.Result, leftHand);

                // ��ũ���� ���� ��ġ ���� (���� �� ��ġ)
                _weaponInstance.transform.localPosition = new Vector3(13.3f, -10.7f, 21.1f);
                _weaponInstance.transform.localEulerAngles = new Vector3(-49.46f, 106.755f, -245.507f);

                // �ݶ��̴� ����
                weaponCollider = _weaponInstance.GetComponent<Collider>();
                if (weaponCollider != null)
                {
                    weaponCollider.enabled = false;
                    weaponCollider.isTrigger = true;
                }

                // ù ��° ���� �ʱ�ȭ
                InitializeFirstWeapon();

                // ������ ���� �ε�
                if (rightHand != null)
                {
                    LoadRightWeapon(rightHand);
                }
                else
                {
                    // ������ ����Ʈ�� ���� ��� ������Ʈ �ʱ�ȭ
                    InitializeComponents();
                }
            }
            else
            {
                Debug.LogError("ChronofractureRed ���� �� �ε� ����!");
            }
        };

        return _weaponInstance;
    }

    private void LoadRightWeapon(Transform rightHand)
    {
        // ������ ����(�Ķ���) �ε�
        Addressables.LoadAssetAsync<GameObject>("ChronofractureBlue").Completed += handle =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded && handle.Result != null)
            {
                _secondWeaponInstance = Instantiate(handle.Result, rightHand);

                // ��ũ���� ���� ��ġ ���� (�Ķ� �� ��ġ)
                _secondWeaponInstance.transform.localPosition = new Vector3(-5f, 5.5f, -13.2f);
                _secondWeaponInstance.transform.localEulerAngles = new Vector3(-43.557f, 109.631f, -78.784f);

                // �ݶ��̴� ����
                secondWeaponCollider = _secondWeaponInstance.GetComponent<Collider>();
                if (secondWeaponCollider != null)
                {
                    secondWeaponCollider.enabled = false;
                    secondWeaponCollider.isTrigger = true;
                }

                // �� ��° ���� �ʱ�ȭ
                InitializeSecondWeapon();

                // ��� ���� �ε� �Ϸ� �� ������Ʈ �ʱ�ȭ
                InitializeComponents();
            }
            else
            {
                Debug.LogError("ChronofractureBlue ���� �� �ε� ����!");
            }
        };
    }

    private void InitializeFirstWeapon()
    {
        // ù ��° ��(������) �ʱ�ȭ
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
        // �� ��° ��(�Ķ���) �ʱ�ȭ
        secondDamageDealer = _secondWeaponInstance.GetComponent<MeleeDamageDealer>();
        if (secondDamageDealer == null)
        {
            secondDamageDealer = _secondWeaponInstance.AddComponent<MeleeDamageDealer>();
        }
        secondDamageDealer.Initialize(this, 0);
        secondDamageDealer.OnFinalDamageCalculated += OnDamageDealt;
    }

    // �ִϸ��̼� �̺�Ʈ���� ȣ��� �޼����
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
        // ��� �ݶ��̴� ��� Ȱ��ȭ (Ư�� ���� ��� ���)
        ActivateLeftCollider();
        ActivateRightCollider();
    }

    public override void DeactivateCollider()
    {
        // ��� �ݶ��̴� ��� ��Ȱ��ȭ
        DeactivateLeftCollider();
        DeactivateRightCollider();
    }

    // �޼� �ܻ� ���� �޼��� (�ִϸ��̼� �̺�Ʈ���� ȣ��)
    public void CreateLeftBladeResidue()
    {
        if (weaponCollider != null && _weaponInstance != null)
        {
            CreateTimeResidue(_weaponInstance.transform.position, _weaponInstance.transform.rotation, 0, true);
        }
    }

    // ������ �ܻ� ���� �޼��� (�ִϸ��̼� �̺�Ʈ���� ȣ��)
    public void CreateRightBladeResidue()
    {
        if (secondWeaponCollider != null && _secondWeaponInstance != null)
        {
            CreateTimeResidue(_secondWeaponInstance.transform.position, _secondWeaponInstance.transform.rotation, 0, false);
        }
    }

    // �޸� ����
    private void OnDestroy()
    {
        // �ܻ� ��ü�� ����
        foreach (var residue in timeResidues)
        {
            if (residue != null)
            {
                Destroy(residue.gameObject);
            }
        }
        timeResidues.Clear();

        // �̺�Ʈ ����
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