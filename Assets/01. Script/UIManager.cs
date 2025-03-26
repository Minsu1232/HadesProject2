using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    // 싱글톤 인스턴스
    public static UIManager Instance { get; private set; }

    // 열려 있는 UI 팝업들을 추적하는 스택
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
        // ESC 키 입력 감지
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseTopUI();
        }
    }

    // UI를 활성화 스택에 등록
    public void RegisterActiveUI(GameObject uiPanel)
    {
        // 이미 스택에 있는지 확인하고 중복 등록 방지
        if (activeUIStack.Contains(uiPanel))
        {
            // 이미 스택에 있다면 최상위로 재정렬하기 위해 기존 항목 제거
            List<GameObject> tempList = new List<GameObject>();
            while (activeUIStack.Count > 0)
            {
                GameObject ui = activeUIStack.Pop();
                if (ui != uiPanel)
                    tempList.Add(ui);
            }

            // 다시 스택에 넣기 (역순)
            for (int i = tempList.Count - 1; i >= 0; i--)
            {
                activeUIStack.Push(tempList[i]);
            }
        }

        // 스택에 추가
        activeUIStack.Push(uiPanel);

        Debug.Log($"UI 등록: {uiPanel.name}, 현재 활성 UI 수: {activeUIStack.Count}");
    }

    // UI를 활성화 스택에서 제거
    public void UnregisterActiveUI(GameObject uiPanel)
    {
        // 해당 UI가 스택에 있는지 확인하고 제거
        if (activeUIStack.Contains(uiPanel))
        {
            List<GameObject> tempList = new List<GameObject>();
            while (activeUIStack.Count > 0)
            {
                GameObject ui = activeUIStack.Pop();
                if (ui != uiPanel)
                    tempList.Add(ui);
            }

            // 다시 스택에 넣기 (역순)
            for (int i = tempList.Count - 1; i >= 0; i--)
            {
                activeUIStack.Push(tempList[i]);
            }

            Debug.Log($"UI 제거: {uiPanel.name}, 현재 활성 UI 수: {activeUIStack.Count}");
        }
    }

    // UI 창 열기
    public void OpenUI(GameObject uiPanel)
    {
        // 기존에 이미 열린 상태인지 확인
        if (uiPanel.activeSelf)
            return;

        uiPanel.SetActive(true);
        RegisterActiveUI(uiPanel);
    }

    // 최상위 UI 닫기
    public void CloseTopUI()
    {
        if (activeUIStack.Count > 0)
        {
            GameObject topUI = activeUIStack.Pop();
            topUI.SetActive(false);
            Debug.Log($"최상위 UI 닫기: {topUI.name}, 남은 활성 UI 수: {activeUIStack.Count}");
        }
    }

    // 특정 UI 닫기
    public void CloseUI(GameObject uiPanel)
    {
        if (uiPanel.activeSelf)
        {
            uiPanel.SetActive(false);
            UnregisterActiveUI(uiPanel);
        }
    }

    // 모든 UI 닫기
    public void CloseAllUI()
    {
        while (activeUIStack.Count > 0)
        {
            GameObject ui = activeUIStack.Pop();
            ui.SetActive(false);
        }
        Debug.Log("모든 UI 닫기 완료");
    }

    // 현재 활성화된 UI 목록 조회 (디버깅용)
    public List<GameObject> GetActiveUIs()
    {
        return new List<GameObject>(activeUIStack);
    }
}