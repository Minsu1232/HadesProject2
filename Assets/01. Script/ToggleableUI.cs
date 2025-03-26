// ����� UI ������Ʈ�� �߰��ϴ� ��ũ��Ʈ
using UnityEngine;

public class ToggleableUI : MonoBehaviour
{
    private void OnEnable()
    {
        // UI�� Ȱ��ȭ�Ǹ� UIManager�� ���
        UIManager.Instance.RegisterActiveUI(gameObject);
    }

    private void OnDisable()
    {
        // UI�� ��Ȱ��ȭ�Ǹ� UIManager���� ����
        UIManager.Instance.UnregisterActiveUI(gameObject);
    }
}