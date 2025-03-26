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
                // ���������� Ȱ��ȭ�� UI ��Ȱ��ȭ
                GameObject lastActiveUI = activeUIElements[activeUIElements.Count - 1];
                lastActiveUI.SetActive(false);
                activeUIElements.Remove(lastActiveUI);
            }
            else
            {
                // Ȱ��ȭ�� UI�� ���� �� ESC�� ������ ���� �Ͻ����� �޴� ǥ�� ��
                //ShowPauseMenu();
            }
        }
    }

    // �� UI ����� Ȱ��ȭ ���°� ����� �� ȣ��
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