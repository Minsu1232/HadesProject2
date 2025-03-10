using UnityEngine;
using UnityEngine.Playables;
using Cinemachine;

public class AlexanderCutsceneController : MonoBehaviour
{
    [SerializeField] private PlayableDirector timelineDirector;
    [SerializeField] private GameObject letterboxTop;
    [SerializeField] private GameObject letterboxBottom;
    [SerializeField] private CinemachineVirtualCamera mainGameplayCamera;

    private CameraFollow originalCameraScript;

    public void PlayCutscene()
    {
        // �����÷��� ī�޶� ��Ȱ��ȭ
        if (mainGameplayCamera.gameObject.activeSelf)
        {
            originalCameraScript = Camera.main.GetComponent<CameraFollow>();
            if (originalCameraScript != null)
                originalCameraScript.enabled = false;

            mainGameplayCamera.gameObject.SetActive(false);
        }

        // ���͹ڽ� Ȱ��ȭ
        letterboxTop.SetActive(true);
        letterboxBottom.SetActive(true);

        // Ÿ�Ӷ��� ���
        timelineDirector.Play();

        // Ÿ�Ӷ��� �Ϸ� �̺�Ʈ ������ �߰�
        timelineDirector.stopped += OnTimelineFinished;
    }

    private void OnTimelineFinished(PlayableDirector director)
    {
        // �̺�Ʈ ������ ����
        timelineDirector.stopped -= OnTimelineFinished;

        // �����÷��� ī�޶� ����
        mainGameplayCamera.gameObject.SetActive(true);
        if (originalCameraScript != null)
            originalCameraScript.enabled = true;

        // ���͹ڽ� ��Ȱ��ȭ
        letterboxTop.SetActive(false);
        letterboxBottom.SetActive(false);

        // ���� ������ ���� ��ȣ �� �����÷��� ���� ó��
        BossAI bossAI = FindObjectOfType<BossAI>();
        if (bossAI != null)
        {
            // ���� ������ ���� ���� ����
        }
    }
}