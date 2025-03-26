using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    // �̱��� �ν��Ͻ�
    public static UIManager Instance { get; private set; }

    // ���� �ִ� UI �˾����� �����ϴ� ����
    private Stack<GameObject> activeUIStack = new Stack<GameObject>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        // ESC Ű �Է� ����
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseTopUI();
        }
    }

    // UI�� Ȱ��ȭ ���ÿ� ���
    public void RegisterActiveUI(GameObject uiPanel)
    {
        // �̹� ���ÿ� �ִ��� Ȯ���ϰ� �ߺ� ��� ����
        if (activeUIStack.Contains(uiPanel))
        {
            // �̹� ���ÿ� �ִٸ� �ֻ����� �������ϱ� ���� ���� �׸� ����
            List<GameObject> tempList = new List<GameObject>();
            while (activeUIStack.Count > 0)
            {
                GameObject ui = activeUIStack.Pop();
                if (ui != uiPanel)
                    tempList.Add(ui);
            }

            // �ٽ� ���ÿ� �ֱ� (����)
            for (int i = tempList.Count - 1; i >= 0; i--)
            {
                activeUIStack.Push(tempList[i]);
            }
        }

        // ���ÿ� �߰�
        activeUIStack.Push(uiPanel);

        Debug.Log($"UI ���: {uiPanel.name}, ���� Ȱ�� UI ��: {activeUIStack.Count}");
    }

    // UI�� Ȱ��ȭ ���ÿ��� ����
    public void UnregisterActiveUI(GameObject uiPanel)
    {
        // �ش� UI�� ���ÿ� �ִ��� Ȯ���ϰ� ����
        if (activeUIStack.Contains(uiPanel))
        {
            List<GameObject> tempList = new List<GameObject>();
            while (activeUIStack.Count > 0)
            {
                GameObject ui = activeUIStack.Pop();
                if (ui != uiPanel)
                    tempList.Add(ui);
            }

            // �ٽ� ���ÿ� �ֱ� (����)
            for (int i = tempList.Count - 1; i >= 0; i--)
            {
                activeUIStack.Push(tempList[i]);
            }

            Debug.Log($"UI ����: {uiPanel.name}, ���� Ȱ�� UI ��: {activeUIStack.Count}");
        }
    }

    // UI â ����
    public void OpenUI(GameObject uiPanel)
    {
        // ������ �̹� ���� �������� Ȯ��
        if (uiPanel.activeSelf)
            return;

        uiPanel.SetActive(true);
        RegisterActiveUI(uiPanel);
    }

    // �ֻ��� UI �ݱ�
    public void CloseTopUI()
    {
        if (activeUIStack.Count > 0)
        {
            GameObject topUI = activeUIStack.Pop();
            topUI.SetActive(false);
            Debug.Log($"�ֻ��� UI �ݱ�: {topUI.name}, ���� Ȱ�� UI ��: {activeUIStack.Count}");
        }
    }

    // Ư�� UI �ݱ�
    public void CloseUI(GameObject uiPanel)
    {
        if (uiPanel.activeSelf)
        {
            uiPanel.SetActive(false);
            UnregisterActiveUI(uiPanel);
        }
    }

    // ��� UI �ݱ�
    public void CloseAllUI()
    {
        while (activeUIStack.Count > 0)
        {
            GameObject ui = activeUIStack.Pop();
            ui.SetActive(false);
        }
        Debug.Log("��� UI �ݱ� �Ϸ�");
    }

    // ���� Ȱ��ȭ�� UI ��� ��ȸ (������)
    public List<GameObject> GetActiveUIs()
    {
        return new List<GameObject>(activeUIStack);
    }
}