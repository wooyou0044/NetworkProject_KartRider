using UnityEngine;

public class CHMTestWheelController : MonoBehaviour
{

    [Header("Steering Settings")]
    [Tooltip("최소 조향 각도")]
    public float steerAngleFrontMin = -45f;
    [Tooltip("최대 조향 각도")]
    public float steerAngleFrontMax = 45f;

    [Tooltip("스키드 마크 효과")]
    public GameObject[] skidMarks;

    public float maxTorque = 30f; // 최대 토크
    public float maxSteerAngle = 30f; // 최대 조향 각도
    public Transform[] wheels; // 바퀴 트랜스폼 배열 (0: 왼쪽 앞바퀴, 1: 오른쪽 앞바퀴, 2: 왼쪽 뒷바퀴, 3: 오른쪽 뒷바퀴)

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
  
    public void UpdateAndRotateWheels(float steerInput, float motorInput, float speed, bool isDrifting)
    {
        float steeringSensitivity = 1.0f; // 필요에 따라 조정

        // 전면 바퀴(왼쪽 앞, 오른쪽 앞)를 조향합니다.
        if (wheels.Length >= 2)
        {
            // 왼쪽 앞바퀴
            float leftSteerAngle = Mathf.Lerp(steerAngleFrontMin, steerAngleFrontMax, (steerInput + 1f) / 2f) * steeringSensitivity;
            wheels[0].localRotation = Quaternion.Euler(90f, leftSteerAngle, wheels[0].localRotation.eulerAngles.z);

            // 오른쪽 앞바퀴
            float rightSteerAngle = Mathf.Lerp(steerAngleFrontMin, steerAngleFrontMax, (steerInput + 1f) / 2f) * steeringSensitivity;
            wheels[1].localRotation = Quaternion.Euler(90f, rightSteerAngle, wheels[1].localRotation.eulerAngles.z);
        }

        // 전체 바퀴에 대해 회전(회전축은 로컬 X축)을 적용합니다.
        // 회전각은 속도와 motorInput(전진 입력)을 반영하도록 합니다.
        float spinAngle = Mathf.Abs(speed) * Time.deltaTime * maxTorque; // (20f는 임의의 상수로 상황에 맞게 조정)
        foreach (Transform wheel in wheels)
        {
            // 전진이면 앞으로, 후진이면 반대로 회전하도록 (필요 시 추가 조건 처리 가능)
            wheel.Rotate(Vector3.forward, spinAngle, Space.Self);
        }

        // 드리프트 상태라면 스키드 마크 효과 활성화
        SetSkidMarkActive(isDrifting);
    }

}
