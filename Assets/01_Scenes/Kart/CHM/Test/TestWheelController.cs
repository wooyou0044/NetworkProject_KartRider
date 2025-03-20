using UnityEngine;

public class TestWheelController : MonoBehaviour
{
    [Header("바퀴 설정")]
    [SerializeField] public Transform[] wheels; // 바퀴 객체 배열
    [SerializeField] public TrailRenderer[] skidMarks; // 스키드 마크 TrailRenderer 배열

    [Header("조향 설정")]
    [SerializeField] public float steerAngleFrontMin = -45f; // 앞바퀴 최소 조향 각도
    [SerializeField] public float steerAngleFrontMax = 45f;  // 앞바퀴 최대 조향 각도
    [SerializeField] public float steeringSensitivity = 0.5f; // 조향 민감도

    [Header("바퀴 회전 설정")]
    [SerializeField] public float wheelRotationSpeed = 100f; // 바퀴 회전 속도

    private float driftTimer = 0f; // 드리프트 타이머
    private bool isDrifting = false; // 드리프트 상태

    // 드리프트 시작 메서드
    public void StartDrift(float driftDuration)
    {
        isDrifting = true;
        driftTimer = driftDuration; // 드리프트 지속 시간 설정
        Debug.Log($"드리프트 시작: 지속 시간 = {driftDuration}초");
    }

    private void Update()
    {
        // 드리프트 지속 시간 체크
        if (isDrifting)
        {
            driftTimer -= Time.deltaTime;
            Debug.Log($"드리프트 타이머: 남은 시간 = {driftTimer}초");

            if (driftTimer <= 0f)
            {
                isDrifting = false; // 드리프트 종료
                Debug.Log("드리프트 종료");
            }
        }
    }

    // 바퀴 업데이트 및 회전
    public void UpdateAndRotateWheels(float steerInput, float motorInput, float speed, bool isDrifting)
    {
        // 앞바퀴 조향 각도 계산
        float steerAngleFrontLeft = Mathf.Clamp(steerInput * steeringSensitivity, steerAngleFrontMin, steerAngleFrontMax);
        float steerAngleFrontRight = Mathf.Clamp(steerInput * steeringSensitivity, steerAngleFrontMin, steerAngleFrontMax);

        // 앞바퀴 조향 적용
        wheels[0].localRotation = Quaternion.Euler(0, steerAngleFrontLeft , wheels[0].localRotation.eulerAngles.z);
        wheels[1].localRotation = Quaternion.Euler(0, steerAngleFrontRight , wheels[1].localRotation.eulerAngles.z);

        // 모든 바퀴 회전
        foreach (Transform wheel in wheels)
        {
            float rotationZ = motorInput * speed * Time.deltaTime * wheelRotationSpeed;
            wheel.Rotate(Vector3.back * rotationZ, Space.Self);
        }

        // 스키드 마크 상태 관리
        for (int i = 0; i < skidMarks.Length; i++)
        {
            skidMarks[i].emitting = isDrifting; // 드리프트 상태에 따라 스키드 마크 활성화
        }
    }

}
