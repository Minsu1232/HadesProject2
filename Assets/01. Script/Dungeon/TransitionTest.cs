using UnityEngine;

public class TransitionTest : MonoBehaviour
{
    void Update()
    {
        // F1 Ű�� ������ ���̵� ��
        if (Input.GetKeyDown(KeyCode.F1))
        {
            SceneTransitionManager.Instance.FadeIn();
        }

        // F2 Ű�� ������ ���̵� �ƿ�
        if (Input.GetKeyDown(KeyCode.F2))
        {
            SceneTransitionManager.Instance.FadeOut();
        }

        // F3 Ű�� ������ �÷��� ȿ��
        if (Input.GetKeyDown(KeyCode.F3))
        {
            SceneTransitionManager.Instance.FlashEffect(Color.white);
        }
    }
}