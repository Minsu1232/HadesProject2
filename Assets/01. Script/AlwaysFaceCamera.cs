using UnityEngine;

public class AlwaysFaceCamera : MonoBehaviour
{
    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("Main Camera not found. Please tag your camera as 'MainCamera'.");
        }
    }

    private void LateUpdate()
    {
        if (mainCamera == null)
            return;

        // ī�޶��� forward ������ �������� ��ü�� ȸ���ϵ��� �����մϴ�.
        transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward,
                         mainCamera.transform.rotation * Vector3.up);
    }
}
