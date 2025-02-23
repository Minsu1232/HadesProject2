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

        // 카메라의 forward 방향을 기준으로 객체가 회전하도록 설정합니다.
        transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward,
                         mainCamera.transform.rotation * Vector3.up);
    }
}
