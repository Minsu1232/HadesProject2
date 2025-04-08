// TrainingDummy.cs - IDamageable ���� ����
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
    [SerializeField] private int requiredHitCount = 3;  // Ʃ�丮�� �Ϸῡ �ʿ��� ��Ʈ ��
    [SerializeField] private string tutorialCompleteDialogID = "tutorial_complete";

    private int hitCount = 0;
    private bool tutorialCompleted = false;
    private void Awake()
    {
        // �±� ���� (MeleeDamageDealer���� Ȯ���� �±�)
        gameObject.tag = "Dummy";

        // �ʿ��� ������Ʈ �߰�
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
        return DamageType.Structure; // �Ǵ� ������ Dummy Ÿ���� �ִٸ� �װ��� ���
    }

    public void TakeDamage(int damage)
    {
        // ������ �ؽ�Ʈ ����
        if (damageTextPrefab != null)
        {
            GameObject damageTextObj = Instantiate(damageTextPrefab, transform.position + Vector3.up, Quaternion.identity);
            TextMeshProUGUI damageText = damageTextObj.GetComponent<TextMeshProUGUI>();
            if (damageText != null)
            {
                damageText.text = damage.ToString();
            }

            // �ִϸ��̼� �� �ڵ� ����
            StartCoroutine(AnimateDamageText(damageTextObj));
            Destroy(damageTextObj, 2f);
        }

        // ��Ʈ ����Ʈ ����
        if (hitEffectPrefab != null)
        {
            GameObject hitEffect = Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
            Destroy(hitEffect, hitEffectDuration);
        }

        // ��Ʈ ���� ���
        if (hitSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hitSound);
        }

        // ī�޶� ����ũ (���ϰ�)
        if (CameraShakeManager.Instance != null)
        {
            CameraShakeManager.TriggerShake(0.5f, 0.05f);
        }
        // ��Ʈ ī��Ʈ ���� �� Ʃ�丮�� ���� üũ
        hitCount++;
        CheckTutorialProgress();
        Debug.Log($"���̰� {damage} �������� �޾ҽ��ϴ�!");
    }

    public void TakeDotDamage(int dotDamage)
    {
        // DoT ������ ó�� (�Ϲ� �������� �����ϰ� ó��)
        TakeDamage(dotDamage);
    }
    private void CheckTutorialProgress()
    {
        if (!tutorialCompleted && hitCount >= requiredHitCount)
        {
            tutorialCompleted = true;

            // ���� �ð� �� ���� Ʃ�丮�� ���̾�α� ǥ��
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

            // ���̵� �ƿ� (�Ĺݺ�)
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