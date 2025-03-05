using System;
using UnityEngine;
using DG.Tweening;

public class HowlSkillEffect : ISkillEffect
{    // �߰�: ȿ�� �Ϸ� �ݹ�
    public event Action OnEffectCompleted;

    private GameObject howlEffectPrefab;
    private GameObject impactPrefab;
    private AudioClip howlSound;
    private float radius;
    private float essenceAmount;
    private float duration;
    private float damage;
    private ICreatureStatus monsterStatus;
    private GameObject effectInstance;
    private GameObject impactInstance;
    private Transform bossTransform;
    private Tween delayTween;
    private bool isFullyInitialized = false;

    private GameObject howlIndicator;
    private GameObject indicatorPrefab; // �ε������� ������
    private float currentPrepareTime = 0f; // ���� �غ� �ð�
    private bool isPreparing = false; // �غ� ���� �÷���
    public HowlSkillEffect(GameObject effectPrefab, GameObject impactPrefab, AudioClip sound, float radius,
                      float essenceAmount, float duration, float damage, Transform transform, string eu, GameObject indicatorPrefab)
    {
     Debug.Log($"HowlSkillEffect ������: howlEffectPrefab={effectPrefab != null}, impactPrefab={impactPrefab != null}, transform={transform != null}");
     this.howlEffectPrefab = effectPrefab;
    this.impactPrefab = impactPrefab;
    this.howlSound = sound;
    this.radius = radius;
    this.essenceAmount = essenceAmount;
    this.duration = duration;
    this.damage = damage;
    this.bossTransform = transform;
    this.indicatorPrefab = indicatorPrefab;
    Debug.Log(eu);
}

    // ���� Initialize �޼��� ���� (ȣȯ��)
    public void Initialize(ICreatureStatus status, Transform target)
    {
        // �⺻ ������ ��� 1.0���� ���� �޼��� ȣ��
        InitializeInternal(status, target, 1.0f, 1.0f);
    }

    // ���ο� Initialize �޼��� (������ ��� �߰�)
    public void Initialize(ICreatureStatus status, Transform target, float damageMultiplier, float durationMultiplier)
    {
        InitializeInternal(status, target, damageMultiplier, durationMultiplier);
    }

    // ���� �ʱ�ȭ ������ ����ϴ� ���� �޼���
    private void InitializeInternal(ICreatureStatus status, Transform target, float damageMultiplier, float durationMultiplier)
    {
       
        try
        {
            if (status == null)
            {
                Debug.LogError("HowlSkillEffect.Initialize: status is null");
                return;
            }

            this.monsterStatus = status;
            

            // ��� ���� (ProjectileSkillEffect�� ������ ����)
            this.damage = Mathf.RoundToInt(damage * damageMultiplier);
            this.duration = duration * durationMultiplier;
            // �ּ����� ��ȿ�� �˻�
            if (status != null)
            {
                isFullyInitialized = true;
            }
            else
            {
                Debug.LogWarning("HowlSkillEffect: �̿��� �ʱ�ȭ - status�� null�Դϴ�");
            }
            Debug.Log($"HowlSkillEffect �ʱ�ȭ �Ϸ�: ������={damage}, ���ӽð�={duration}");
        }
        catch (Exception e)
        {
            Debug.LogError($"HowlSkillEffect.Initialize ����: {e.Message}\n{e.StackTrace}");
        }
    }
     
    public void Execute()
    {
        Debug.Log($"Execute ����: howlEffectPrefab={howlEffectPrefab != null}, bossTransform={bossTransform != null}");
        try
        {// Ÿ�� ���� ����
            if (bossTransform == null && monsterStatus != null)
            {
                bossTransform = monsterStatus.GetMonsterTransform();
                Debug.Log("HowlSkillEffect: �������� ���� transform ����");
            }

            // �ʼ� ���� ��ȿ�� �˻�
            if (monsterStatus == null || bossTransform == null)
            {
                Debug.LogError("HowlSkillEffect.Execute: �ʼ� ������ null�Դϴ�");
                return;
            }
            // �ε������� ����
            if (indicatorPrefab != null)
            {
                howlIndicator = GameObject.Instantiate(
                    indicatorPrefab,
                    bossTransform.position + new Vector3(0, 0.5f, 0), // ���� ��ġ���� �ణ ����
                    Quaternion.Euler(90f, 0f, 0f) // �ٴ��� ���ϵ��� ȸ��
                );

                // �ε������� ũ�� ���� (�ݰ� ����)
                howlIndicator.transform.localScale = new Vector3(radius * 2, radius * 2, 1f);

                // �ε������� ���� �ʱ�ȭ
                var renderer = howlIndicator.GetComponentInChildren<Renderer>();
                if (renderer != null)
                {
                    // _FillAmount �Ӽ� �ʱ�ȭ
                    renderer.material.SetFloat("_FillAmount", 0f);

                    // �ε������� ä��� �ִϸ��̼�
                    DOTween.To(
                        () => 0f,
                        (value) => renderer.material.SetFloat("_FillAmount", value),
                        1f, // 1.0���� ä��
                        duration
                    ).SetEase(Ease.Linear);
                }
            }
            Vector3 howlTransform = bossTransform.position;
            howlTransform.y += 5f;  // y���� 5��ŭ �ø�
            
            // �Ͽ︵ ����Ʈ ���� (��� ǥ��)
            if (howlEffectPrefab != null && bossTransform != null)
            {
                Debug.Log(bossTransform.gameObject.ToString());
                effectInstance = GameObject.Instantiate(
                    howlEffectPrefab,
                    howlTransform,
                    Quaternion.identity
                );

                // �Ͽ︵ ����Ʈ�� ª�� ǥ���ϰ� ���� (3�ʷ� ���� ����, �ʿ�� ����)
                DOTween.Sequence()
                    .AppendInterval(3.0f)
                    .OnComplete(() => {
                        if (effectInstance != null)
                        {
                            GameObject.Destroy(effectInstance);
                        }
                    })
                    .SetLink(effectInstance);
            }
            else
            {
                Debug.LogError("HowlSkillEffect.Execute: howlEffectPrefab �Ǵ� bossTransform�� null�Դϴ�.");
            }

            // ���� ���
            if (howlSound != null && bossTransform != null)
            {
                AudioSource.PlayClipAtPoint(howlSound, bossTransform.position, 1.0f);
            }

            // DOTween�� ����Ͽ� ������ ����Ʈ ȿ�� ����
            delayTween = DOTween.Sequence()
                .AppendInterval(duration)
                .OnComplete(ApplyEffectNow);

            // ���� Ʈ�������� ������ SetLink ���
            if (bossTransform != null)
            {
                delayTween.SetLink(bossTransform.gameObject);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"HowlSkillEffect.Execute ����: {e.Message}\n{e.StackTrace}");
        }
    }

    // �෹�̼� �� ���� ȿ�� ���� ����
    private void ApplyEffectNow()
    {
        GameObject.Destroy(howlIndicator);
        Debug.Log("��ų�� ���� ȿ�� ������!!!!");
        try
        {
            if (bossTransform == null)
            {
                Debug.LogError("HowlSkillEffect.ApplyEffectNow: bossTransform�� null�Դϴ�.");
                return;
            }
            Vector3 impact = bossTransform.position;
            impact.y += 1.3f;  // y���� 1��ŭ �ø�
            // ����Ʈ ����Ʈ ���� (�෹�̼� �� ǥ��)
            if (impactPrefab != null)
            {
                impactInstance = GameObject.Instantiate(
                    impactPrefab,
                    impact,
                    Quaternion.identity
                    
                );
                Debug.Log("�����ռ�ȯ");
                // ����Ʈ ����Ʈ ũ�⸦ radius�� �°� ����
                // �⺻ ũ�⸦ 1�̶�� ������ �� radius�� ����Ͽ� ����
                float scaleMultiplier = radius / 3.0f; // 5�� �⺻ �ݰ� ������ ����, �ʿ信 ���� ����
                impactInstance.transform.localScale = new Vector3(
                    scaleMultiplier,
                    scaleMultiplier,
                    scaleMultiplier
                );
                // ����Ʈ ����Ʈ�� 3�� ���� ǥ�� �� ���� (�ʿ�� ����)
                DOTween.Sequence()
                    .AppendInterval(3.0f)
                    .OnComplete(() => {
                        if (impactInstance != null)
                        {
                            GameObject.Destroy(impactInstance);
                        }
                    })
                    .SetLink(impactInstance);
            }
           
            // ���� �� �÷��̾� ����
            Collider[] hitColliders = Physics.OverlapSphere(
                bossTransform.position,
                radius,
                LayerMask.GetMask("Player")
            );
            OnEffectCompleted?.Invoke();
            // �÷��̾�� Essence ���� �� ������ ����
            foreach (var hitCollider in hitColliders)
            {
                if (hitCollider.CompareTag("Player"))
                {
                    // Essence ����
                    var bossMonster = monsterStatus.GetMonsterClass() as AlexanderBoss;
                    if (bossMonster != null)
                    {
                        bossMonster.InflictEssence(essenceAmount);
                        Debug.Log($"���¢������ Essence {essenceAmount} ����!");
                        // PlayerManager ���� �߾� �����ڸ� ���� PlayerClass �ν��Ͻ� ����
                        var playerClass = GameInitializer.Instance.GetPlayerClass();
                        playerClass.TakeDamage((int)damage);
                        
                        return;
                    }
                   
                    else
                    {
                        Debug.LogWarning("HowlSkillEffect.ApplyEffectNow: �÷��̾� ������Ʈ�� PlayerHealth ������Ʈ�� �����ϴ�.");
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"HowlSkillEffect.ApplyEffectNow ����: {e.Message}\n{e.StackTrace}");
        }
    }

    public void OnComplete()
    {
        // ���� ���� DOTween �ִϸ��̼� ����
        if (delayTween != null && delayTween.IsActive())
        {
            delayTween.Kill();
        }

        // ����Ʈ ����
        if (effectInstance != null)
        {
            GameObject.Destroy(effectInstance);
        }

        //if (impactInstance != null)
        //{
        //    GameObject.Destroy(impactInstance);
        //}
        if(howlIndicator != null)
        {
            GameObject.Destroy(howlIndicator);
        }
    }
}