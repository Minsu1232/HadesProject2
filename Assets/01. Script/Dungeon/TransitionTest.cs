using UnityEngine;

public class TransitionTest : MonoBehaviour
{
    void Update()
    {
        // F1 키를 누르면 페이드 인
        if (Input.GetKeyDown(KeyCode.F1))
        {
            SceneTransitionManager.Instance.FadeIn();
        }

        // F2 키를 누르면 페이드 아웃
        if (Input.GetKeyDown(KeyCode.F2))
        {
            SceneTransitionManager.Instance.FadeOut();
        }

        // F3 키를 누르면 플래시 효과
        if (Input.GetKeyDown(KeyCode.F3))
        {
            SceneTransitionManager.Instance.FlashEffect(Color.white);
        }
    }
}