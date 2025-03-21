using UnityEngine;

public class CHMTestWheelController : MonoBehaviour
{
    public float maxTorque = 10f; // 최대 토크
    public float maxSteerAngle = 30f; // 최대 조향 각도
    public Transform[] wheels; // 바퀴 트랜스폼 배열 (0: 왼쪽 앞바퀴, 1: 오른쪽 앞바퀴, 2: 왼쪽 뒷바퀴, 3: 오른쪽 뒷바퀴)

    [Header("Steering Settings")]
    [Tooltip("최소 조향 각도")]
    public float steerAngleFrontMin = -45f;
    [Tooltip("최대 조향 각도")]
    public float steerAngleFrontMax = 45f;

    [Tooltip("스키드 마크 효과")]
    public GameObject[] skidMarks;

    // 카트 컨트롤러 참조
    private CHMTestKartController kartController;

    void Start()
    {
        // 카트 컨트롤러 참조 가져오기
        kartController = GetComponentInParent<CHMTestKartController>();

        // 스키드 마크 초기 비활성화
        SetSkidMarkActive(false);
    }


    public void SetSkidMarkActive(bool isActive)
    {
        foreach (GameObject skidMark in skidMarks)
        {
            skidMark.GetComponent<TrailRenderer>().emitting = isActive;
        }
    }

    public void RotateWheel(float steerInput, float steeringSensitivity)
    {
        float steerAngleFrontLeft = Mathf.Lerp(steerAngleFrontMin, steerAngleFrontMax, (steerInput + 1) / 2) * steeringSensitivity;
        float steerAngleFrontRight = Mathf.Lerp(steerAngleFrontMin, steerAngleFrontMax, (steerInput + 1) / 2) * steeringSensitivity;

        wheels[0].localRotation = Quaternion.Euler(0, steerAngleFrontLeft - 90, wheels[0].localRotation.eulerAngles.z);
        wheels[2].localRotation = Quaternion.Euler(0, steerAngleFrontRight - 90, wheels[1].localRotation.eulerAngles.z);
    }


}
