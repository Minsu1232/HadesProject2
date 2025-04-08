// TrainingDummy.cs - IDamageable 직접 구현
using UnityEngine;
using TMPro;
using System.Collections;
using static AttackData;

public class TrainingDummy : MonoBehaviour, IDamageable
{
    [SerializeField] private GameObject damageTextPrefab;
    [SerializeField] private GameObject hitEffectPrefab;
    [SerializeField] private float hitEffectDuration = 0.2f;
    [SerializeField] private AudioClip hitSound;

    private AudioSource audioSource;
    [SerializeField] private int requiredHitCount = 3;  // 튜토리얼 완료에 필요한 히트 수
    [SerializeField] private string tutorialCompleteDialogID = "tutorial_complete";

    private int hitCount = 0;
    private bool tutorialCompleted = false;
    private void Awake()
    {
        // 태그 설정 (MeleeDamageDealer에서 확인할 태그)
        gameObject.tag = "Dummy";

        // 필요한 컴포넌트 추가
        if (GetComponent<Collider>() == null)
        {
            gameObject.AddComponent<BoxCollider>();
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1.0f;
        }
    }

    public DamageType GetDamageType()
    {
        return DamageType.Structure; // 또는 별도의 Dummy 타입이 있다면 그것을 사용
    }

    public void TakeDamage(int damage)
    {
        // 데미지 텍스트 생성
        if (damageTextPrefab != null)
        {
            GameObject damageTextObj = Instantiate(damageTextPrefab, transform.position + Vector3.up, Quaternion.identity);
            TextMeshProUGUI damageText = damageTextObj.GetComponent<TextMeshProUGUI>();
            if (damageText != null)
            {
                damageText.text = damage.ToString();
            }

            // 애니메이션 및 자동 삭제
            StartCoroutine(AnimateDamageText(damageTextObj));
            Destroy(damageTextObj, 2f);
        }

        // 히트 이펙트 생성
        if (hitEffectPrefab != null)
        {
            GameObject hitEffect = Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
            Destroy(hitEffect, hitEffectDuration);
        }

        // 히트 사운드 재생
        if (hitSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hitSound);
        }

        // 카메라 쉐이크 (약하게)
        if (CameraShakeManager.Instance != null)
        {
            CameraShakeManager.TriggerShake(0.5f, 0.05f);
        }
        // 히트 카운트 증가 및 튜토리얼 진행 체크
        hitCount++;
        CheckTutorialProgress();
        Debug.Log($"더미가 {damage} 데미지를 받았습니다!");
    }

    public void TakeDotDamage(int dotDamage)
    {
        // DoT 데미지 처리 (일반 데미지와 동일하게 처리)
        TakeDamage(dotDamage);
    }
    private void CheckTutorialProgress()
    {
        if (!tutorialCompleted && hitCount >= requiredHitCount)
        {
            tutorialCompleted = true;

            // 일정 시간 후 다음 튜토리얼 다이얼로그 표시
            Invoke("ShowCompletionDialog", 1.5f);
        }
    }

    private void ShowCompletionDialog()
    {
        if (DialogSystem.Instance != null)
        {
            DialogSystem.Instance.StartDialog(tutorialCompleteDialogID);
        }
    }
    private IEnumerator AnimateDamageText(GameObject textObj)
    {
        if (textObj == null) yield break;

        Vector3 startPos = textObj.transform.position;
        Vector3 endPos = startPos + Vector3.up * 1.5f;
        float duration = 1.0f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            textObj.transform.position = Vector3.Lerp(startPos, endPos, t);

            // 페이드 아웃 (후반부)
            if (t > 0.5f)
            {
                TextMeshProUGUI tmp = textObj.GetComponent<TextMeshProUGUI>();
                if (tmp != null)
                {
                    Color color = tmp.color;
                    color.a = 1f - (t - 0.5f) * 2f;
                    tmp.color = color;
                }
            }

            elapsed += Time.deltaTime;
            yield return null;
        }
    }
}