using UnityEngine;

public class TestCamera : MonoBehaviour
{
    [Header("Target Settings")]
    [Tooltip("카메라가 따라갈 Kart 오브젝트입니다.")]
    public Transform target; // 따라갈 대상 (Kart)

    [Header("Camera Offset Settings")]
    [Tooltip("카메라와 Kart 간의 위치 오프셋입니다.")]
    public Vector3 offset = new Vector3(0f, 5f, -10f); // 기본 오프셋 값
    [Tooltip("카메라 이동 속도입니다.")]
    public float followSpeed = 10f;
    [Tooltip("카메라 회전 속도입니다.")]
    public float rotationSmoothSpeed = 5f;

    private void LateUpdate()
    {
        if (target == null)
        {
            Debug.LogWarning("FollowCamera: Target이 설정되지 않았습니다!");
            return;
        }

        // 위치 계산 (Kart의 위치 + 오프셋)
        Vector3 desiredPosition = target.position + target.rotation * offset; // 대상 회전에 따라 오프셋 조정
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);
        transform.position = smoothedPosition;

        // 회전 계산 (대상의 회전 방향을 따름)
        Quaternion desiredRotation = Quaternion.Euler(0, target.eulerAngles.y, 0); // Y축 중심 회전
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotationSmoothSpeed * Time.deltaTime);
    }
}
