// 토글형 UI 컴포넌트에 추가하는 스크립트
using UnityEngine;

public class ToggleableUI : MonoBehaviour
{
    private void OnEnable()
    {
        // UI가 활성화되면 UIManager에 등록
        UIManager.Instance.RegisterActiveUI(gameObject);
    }

    private void OnDisable()
    {
        // UI가 비활성화되면 UIManager에서 제거
        UIManager.Instance.UnregisterActiveUI(gameObject);
    }
}