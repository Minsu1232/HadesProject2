//using System.Collections;
//using UnityEngine;
//using UnityEngine.UI;
//using TMPro;

//public class DungeonReturnManager : MonoBehaviour
//{
//    [Header("����")]
//    [SerializeField] private DungeonManager dungeonManager;
//    [SerializeField] private GameObject returnButtonPrefab;
//    [SerializeField] private Transform canvasTransform;

//    [Header("��ȯ ����")]
//    [SerializeField] private float autoReturnDelay = 30f; // �ڵ� ��ȯ �ð�
//    [SerializeField] private float checkItemsRadius = 20f; // ������ �˻� ����

//    private GameObject activeReturnButton;
//    private bool isDialogActive = false;
//    private bool isReturning = false;
//    private Coroutine autoReturnCoroutine;

//    private void Awake()
//    {
//        // ĵ���� ���� ��� (������ ã��)
//        if (canvasTransform == null)
//        {
//            Canvas mainCanvas = FindObjectOfType<Canvas>();
//            if (mainCanvas != null)
//            {
//                canvasTransform = mainCanvas.transform;
//            }
//        }

//        // ���� �Ŵ��� ���� ��� (������ ã��)
//        if (dungeonManager == null)
//        {
//            dungeonManager = FindObjectOfType<DungeonManager>();
//        }
//    }
//    private void OnEnable()
//    {
//        // ���̾�α� �̺�Ʈ ����
//        if (DialogSystem.Instance != null)
//        {            
//            DialogSystem.OnDialogStateChanged += HandleDialogStateChanged;
//        }
//    }

//    private void OnDisable()
//    {
//        if (DialogSystem.Instance != null)
//        {            
//            DialogSystem.OnDialogStateChanged -= HandleDialogStateChanged;
//        }
//    }

//    // ���̾�α� ���� ���� �ڵ鷯
//    private void HandleDialogStateChanged(bool isActive)
//    {
//        if (isActive)
//        {
//            // ���̾�α� Ȱ��ȭ �� ��ư �����
//            HideReturnButton();
//        }
//        else
//        {
//            // ���̾�α� ��Ȱ��ȭ �� ��ư �ٽ� ǥ��
//            ShowReturnButtonAgain();
//        }
//    }
//    // ���̾�α� �̺�Ʈ ó��


//    // ��ȯ ��ư ǥ��
//    public void ShowReturnButton()
//    {
//        if (returnButtonPrefab == null || activeReturnButton != null || isReturning)
//            return;

//        // ��ư ����
//        activeReturnButton = Instantiate(returnButtonPrefab, canvasTransform);

//        // ��ư Ŭ�� �̺�Ʈ ����
//        Button button = activeReturnButton.GetComponentInChildren<Button>();
//        if (button != null)
//        {
//            button.onClick.AddListener(ReturnToVillage);
//        }       

//        // �ڵ� ��ȯ Ÿ�̸� ����
//        StartAutoReturnTimer();
//    }

//    // �ʵ忡 ���� ������ Ȯ��


//    // �ڵ� ��ȯ Ÿ�̸� ����
//    private void StartAutoReturnTimer()
//    {
//        // �̹� ���� ���� Ÿ�̸� ����
//        if (autoReturnCoroutine != null)
//        {
//            StopCoroutine(autoReturnCoroutine);
//        }

//        // �� Ÿ�̸� ����
//        autoReturnCoroutine = StartCoroutine(AutoReturnCoroutine());
//    }

//    // �ڵ� ��ȯ �ڷ�ƾ
//    private IEnumerator AutoReturnCoroutine()
//    {
//        float remainingTime = autoReturnDelay;

//        // 30��, 15��, 5�� ���� �˸�
//        while (remainingTime > 0 && !isReturning)
//        {
//            // Ư�� �ð��뿡 �˸� ǥ��
//            if (remainingTime <= 30 && remainingTime > 29 ||
//                remainingTime <= 15 && remainingTime > 14 ||
//                remainingTime <= 5 && remainingTime > 4)
//            {
//                if (UIManager.Instance != null)
//                {
//                    UIManager.Instance.ShowNotification($"{Mathf.FloorToInt(remainingTime)}�� �� �ڵ����� ������ ��ȯ�մϴ�", Color.white);
//                }
//            }

//            yield return new WaitForSeconds(1f);
//            remainingTime -= 1f;

//            // ���������� ������ Ȯ��
//            if (remainingTime % 10 == 0)
//            {
//                CheckRemainingItems();
//            }
//        }

//        // Ÿ�̸� ���� �� ��ȯ
//        if (!isReturning)
//        {
//            ReturnToVillage();
//        }
//    }

//    // ������ ��ȯ ó��
//    public void ReturnToVillage()
//    {
//        if (isReturning || dungeonManager == null) return;

//        isReturning = true;

//        // ��ư ���� ����
//        if (activeReturnButton != null)
//        {
//            Button button = activeReturnButton.GetComponentInChildren<Button>();
//            if (button != null)
//            {
//                button.interactable = false;

//                TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
//                if (buttonText != null)
//                {
//                    buttonText.text = "��ȯ ��...";
//                }
//            }
//        }

//        // ���� �Ŵ����� ���� ��ȯ �޼��� ȣ��
//        StartCoroutine(ReturnToVillageProcess());
//    }

//    // ���� ��ȯ ���μ���
//    private IEnumerator ReturnToVillageProcess()
//    {
//        // ȭ�� ���̵� ȿ��
//        if (SceneTransitionManager.Instance != null)
//        {
//            SceneTransitionManager.Instance.FadeIn();
//        }

//        // ���̵� ȿ���� ���� ª�� ���
//        yield return new WaitForSeconds(0.5f);

//        // ��ư ����
//        if (activeReturnButton != null)
//        {
//            Destroy(activeReturnButton);
//            activeReturnButton = null;
//        }

//        // ���� �Ŵ����� ReturnToVillage�� async Task�̹Ƿ�
//        // ���� ȣ���ϰ� �ڷ�ƾ�� �����մϴ�
//        dungeonManager.ReturnToVillage();

//        // �� �ڷ�ƾ ������ �ڵ�� ������� �ʽ��ϴ�
//        // Scene ��ȯ���� ���� ������Ʈ�� �ı��� ���̱� �����Դϴ�
//    }

//    // ��ư ����� (���̾�α� ���� �� ȣ��)
//    public void HideReturnButton()
//    {
//        if (activeReturnButton != null)
//        {
//            activeReturnButton.SetActive(false);
//        }

//        // �ڵ� ��ȯ Ÿ�̸� �Ͻ� ����
//        if (autoReturnCoroutine != null)
//        {
//            StopCoroutine(autoReturnCoroutine);
//            autoReturnCoroutine = null;
//        }
//    }

//    // ��ư �ٽ� ǥ�� (���̾�α� ���� �� ȣ��)
//    public void ShowReturnButtonAgain()
//    {
//        if (activeReturnButton != null)
//        {
//            activeReturnButton.SetActive(true);

//            // �ڵ� ��ȯ Ÿ�̸� �����
//            StartAutoReturnTimer();
//        }
//    }
//}