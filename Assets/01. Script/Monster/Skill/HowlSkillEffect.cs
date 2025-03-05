using System;
using UnityEngine;
using DG.Tweening;

public class HowlSkillEffect : ISkillEffect
{    // 추가: 효과 완료 콜백
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
    private GameObject indicatorPrefab; // 인디케이터 프리팹
    private float currentPrepareTime = 0f; // 현재 준비 시간
    private bool isPreparing = false; // 준비 상태 플래그
    public HowlSkillEffect(GameObject effectPrefab, GameObject impactPrefab, AudioClip sound, float radius,
                      float essenceAmount, float duration, float damage, Transform transform, string eu, GameObject indicatorPrefab)
    {
     Debug.Log($"HowlSkillEffect 생성자: howlEffectPrefab={effectPrefab != null}, impactPrefab={impactPrefab != null}, transform={transform != null}");
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

    // 기존 Initialize 메서드 유지 (호환성)
    public void Initialize(ICreatureStatus status, Transform target)
    {
        // 기본 데미지 계수 1.0으로 내부 메서드 호출
        InitializeInternal(status, target, 1.0f, 1.0f);
    }

    // 새로운 Initialize 메서드 (데미지 계수 추가)
    public void Initialize(ICreatureStatus status, Transform target, float damageMultiplier, float durationMultiplier)
    {
        InitializeInternal(status, target, damageMultiplier, durationMultiplier);
    }

    // 실제 초기화 로직을 담당하는 내부 메서드
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
            

            // 계수 적용 (ProjectileSkillEffect와 동일한 패턴)
            this.damage = Mathf.RoundToInt(damage * damageMultiplier);
            this.duration = duration * durationMultiplier;
            // 최소한의 유효성 검사
            if (status != null)
            {
                isFullyInitialized = true;
            }
            else
            {
                Debug.LogWarning("HowlSkillEffect: 미완전 초기화 - status가 null입니다");
            }
            Debug.Log($"HowlSkillEffect 초기화 완료: 데미지={damage}, 지속시간={duration}");
        }
        catch (Exception e)
        {
            Debug.LogError($"HowlSkillEffect.Initialize 오류: {e.Message}\n{e.StackTrace}");
        }
    }
     
    public void Execute()
    {
        Debug.Log($"Execute 시작: howlEffectPrefab={howlEffectPrefab != null}, bossTransform={bossTransform != null}");
        try
        {// 타겟 동적 결정
            if (bossTransform == null && monsterStatus != null)
            {
                bossTransform = monsterStatus.GetMonsterTransform();
                Debug.Log("HowlSkillEffect: 동적으로 보스 transform 설정");
            }

            // 필수 참조 유효성 검사
            if (monsterStatus == null || bossTransform == null)
            {
                Debug.LogError("HowlSkillEffect.Execute: 필수 참조가 null입니다");
                return;
            }
            // 인디케이터 생성
            if (indicatorPrefab != null)
            {
                howlIndicator = GameObject.Instantiate(
                    indicatorPrefab,
                    bossTransform.position + new Vector3(0, 0.5f, 0), // 보스 위치에서 약간 위로
                    Quaternion.Euler(90f, 0f, 0f) // 바닥을 향하도록 회전
                );

                // 인디케이터 크기 설정 (반경 기준)
                howlIndicator.transform.localScale = new Vector3(radius * 2, radius * 2, 1f);

                // 인디케이터 재질 초기화
                var renderer = howlIndicator.GetComponentInChildren<Renderer>();
                if (renderer != null)
                {
                    // _FillAmount 속성 초기화
                    renderer.material.SetFloat("_FillAmount", 0f);

                    // 인디케이터 채우기 애니메이션
                    DOTween.To(
                        () => 0f,
                        (value) => renderer.material.SetFloat("_FillAmount", value),
                        1f, // 1.0까지 채움
                        duration
                    ).SetEase(Ease.Linear);
                }
            }
            Vector3 howlTransform = bossTransform.position;
            howlTransform.y += 5f;  // y축을 5만큼 올림
            
            // 하울링 이펙트 생성 (즉시 표시)
            if (howlEffectPrefab != null && bossTransform != null)
            {
                Debug.Log(bossTransform.gameObject.ToString());
                effectInstance = GameObject.Instantiate(
                    howlEffectPrefab,
                    howlTransform,
                    Quaternion.identity
                );

                // 하울링 이펙트는 짧게 표시하고 삭제 (3초로 임의 설정, 필요시 조정)
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
                Debug.LogError("HowlSkillEffect.Execute: howlEffectPrefab 또는 bossTransform이 null입니다.");
            }

            // 사운드 재생
            if (howlSound != null && bossTransform != null)
            {
                AudioSource.PlayClipAtPoint(howlSound, bossTransform.position, 1.0f);
            }

            // DOTween을 사용하여 지연된 임팩트 효과 적용
            delayTween = DOTween.Sequence()
                .AppendInterval(duration)
                .OnComplete(ApplyEffectNow);

            // 보스 트랜스폼이 있으면 SetLink 사용
            if (bossTransform != null)
            {
                delayTween.SetLink(bossTransform.gameObject);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"HowlSkillEffect.Execute 오류: {e.Message}\n{e.StackTrace}");
        }
    }

    // 듀레이션 후 실제 효과 적용 로직
    private void ApplyEffectNow()
    {
        GameObject.Destroy(howlIndicator);
        Debug.Log("스킬의 실제 효과 시작함!!!!");
        try
        {
            if (bossTransform == null)
            {
                Debug.LogError("HowlSkillEffect.ApplyEffectNow: bossTransform이 null입니다.");
                return;
            }
            Vector3 impact = bossTransform.position;
            impact.y += 1.3f;  // y축을 1만큼 올림
            // 임팩트 이펙트 생성 (듀레이션 후 표시)
            if (impactPrefab != null)
            {
                impactInstance = GameObject.Instantiate(
                    impactPrefab,
                    impact,
                    Quaternion.identity
                    
                );
                Debug.Log("프리팹소환");
                // 임팩트 이펙트 크기를 radius에 맞게 조정
                // 기본 크기를 1이라고 가정할 때 radius에 비례하여 조정
                float scaleMultiplier = radius / 3.0f; // 5는 기본 반경 값으로 가정, 필요에 따라 조정
                impactInstance.transform.localScale = new Vector3(
                    scaleMultiplier,
                    scaleMultiplier,
                    scaleMultiplier
                );
                // 임팩트 이펙트는 3초 동안 표시 후 삭제 (필요시 조정)
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
           
            // 범위 내 플레이어 감지
            Collider[] hitColliders = Physics.OverlapSphere(
                bossTransform.position,
                radius,
                LayerMask.GetMask("Player")
            );
            OnEffectCompleted?.Invoke();
            // 플레이어에게 Essence 증가 및 데미지 적용
            foreach (var hitCollider in hitColliders)
            {
                if (hitCollider.CompareTag("Player"))
                {
                    // Essence 증가
                    var bossMonster = monsterStatus.GetMonsterClass() as AlexanderBoss;
                    if (bossMonster != null)
                    {
                        bossMonster.InflictEssence(essenceAmount);
                        Debug.Log($"울부짖음으로 Essence {essenceAmount} 증가!");
                        // PlayerManager 같은 중앙 관리자를 통해 PlayerClass 인스턴스 접근
                        var playerClass = GameInitializer.Instance.GetPlayerClass();
                        playerClass.TakeDamage((int)damage);
                        
                        return;
                    }
                   
                    else
                    {
                        Debug.LogWarning("HowlSkillEffect.ApplyEffectNow: 플레이어 오브젝트에 PlayerHealth 컴포넌트가 없습니다.");
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"HowlSkillEffect.ApplyEffectNow 오류: {e.Message}\n{e.StackTrace}");
        }
    }

    public void OnComplete()
    {
        // 진행 중인 DOTween 애니메이션 정리
        if (delayTween != null && delayTween.IsActive())
        {
            delayTween.Kill();
        }

        // 이펙트 정리
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