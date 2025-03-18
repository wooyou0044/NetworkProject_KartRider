using UnityEngine;

public class TestWheelController : MonoBehaviour
{
    [Header("Wheels Settings")]
    [SerializeField] public Transform[] wheels; // 바퀴 배열
    [SerializeField] public TrailRenderer[] skidMarks; // 스키드 마크용 테일 렌더러 배열

    [Header("Steering Settings")]
    [SerializeField] public float steerAngleFrontMin = -45f; // 앞바퀴 최소 조향 각도
    [SerializeField] public float steerAngleFrontMax = 45f;  // 앞바퀴 최대 조향 각도
    [SerializeField] public float steeringSensitivity = 1.0f; // 조향 민감도
    

    [Header("Wheel Rotation Settings")]
    [SerializeField] public float wheelRotationSpeed = 100f; // 바퀴 회전 속도

    public void UpdateAndRotateWheels(float steerInput, float motorInput, float speed, bool isDrifting)
    {
        // 앞바퀴 좌우 조향 각도 계산
        float steerAngleFrontLeft = Mathf.Clamp(steerInput * steeringSensitivity, steerAngleFrontMin, steerAngleFrontMax);
        float steerAngleFrontRight = Mathf.Clamp(steerInput * steeringSensitivity, steerAngleFrontMin, steerAngleFrontMax);

        // 앞바퀴 조향 처리
        wheels[0].localRotation = Quaternion.Euler(0, steerAngleFrontLeft, wheels[0].localRotation.eulerAngles.z);
        wheels[1].localRotation = Quaternion.Euler(0, steerAngleFrontRight, wheels[1].localRotation.eulerAngles.z);

        // 바퀴 Z축 회전 처리
        foreach (Transform wheel in wheels)
        {
            float rotationZ = motorInput * speed * Time.deltaTime * wheelRotationSpeed;
            wheel.Rotate(Vector3.back * rotationZ, Space.Self);
        }

        // 드리프트 중 스키드 마크 활성화
        for (int i = 0; i < skidMarks.Length; i++)
        {
            if (isDrifting)
            {
                skidMarks[i].emitting = true; // 스키드 마크 표시
            }
            else
            {
                skidMarks[i].emitting = false; // 스키드 마크 비활성화
            }
        }
    }
}
