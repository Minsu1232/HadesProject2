using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> toggleableUIElements;
    private List<GameObject> activeUIElements = new List<GameObject>();

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (activeUIElements.Count > 0)
            {
                // 마지막으로 활성화된 UI 비활성화
                GameObject lastActiveUI = activeUIElements[activeUIElements.Count - 1];
                lastActiveUI.SetActive(false);
                activeUIElements.Remove(lastActiveUI);
            }
            else
            {
                // 활성화된 UI가 없을 때 ESC를 누르면 게임 일시정지 메뉴 표시 등
                //ShowPauseMenu();
            }
        }
    }

    // 각 UI 요소의 활성화 상태가 변경될 때 호출
    public void OnUIStateChanged(GameObject uiElement, bool isActive)
    {
        if (isActive)
        {
            if (!activeUIElements.Contains(uiElement))
                activeUIElements.Add(uiElement);
        }
        else
        {
            activeUIElements.Remove(uiElement);
        }
    }
}