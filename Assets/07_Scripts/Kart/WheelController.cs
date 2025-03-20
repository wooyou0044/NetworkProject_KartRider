using UnityEngine;

public class WheelController : MonoBehaviour
{
    public float maxTorque = 10f; // 모터의 최대 토크
    public float maxSteerAngle = 30f; // 최대 조향 각도
    public Transform[] wheels; // 바퀴의 트랜스폼 배열 (0: 왼쪽 앞바퀴, 1: 왼쪽 뒷바퀴, 2: 오른쪽 앞바퀴, 3: 오른쪽 뒷바퀴)

    [Header("Steering Settings")]
    [Tooltip("최소 조향 각도")]
    public float steerAngleFrontMin = -45f;
    [Tooltip("최대 조향 각도")]
    public float steerAngleFrontMax = 45f;

    //[Tooltip("스키드 마크 효과")]
    //public GameObject[] skidMarks;

    void Start()
    {
        // 스키드마크 초기 비활성화
        //SetSkidMarkActive(false);
    }

    //public void SetSkidMarkActive(bool isActive)
    //{
    //    foreach (GameObject skidMark in skidMarks)
    //    {
    //        //skidMark.GetComponent<TrailRenderer>().emitting = isActive;
    //    }
    //}

    public void RotateWheel(float steerInput, float steeringSensitivity)
    {
        // 조향 각도 계산
        float steerAngleFrontLeft = Mathf.Lerp(steerAngleFrontMin, steerAngleFrontMax, (steerInput + 1) / 2) * steeringSensitivity;
        float steerAngleFrontRight = Mathf.Lerp(steerAngleFrontMin, steerAngleFrontMax, (steerInput + 1) / 2) * steeringSensitivity;

        // 앞 바퀴 회전
        wheels[0].localRotation = Quaternion.Euler(0, steerAngleFrontLeft - 90, wheels[0].localRotation.eulerAngles.z);
        wheels[2].localRotation = Quaternion.Euler(0, steerAngleFrontRight - 90, wheels[1].localRotation.eulerAngles.z);
    }

    public void UpdateWheelRotation(float motorInput, float speed)
    {
        foreach (Transform wheel in wheels)
        {
            wheel.Rotate(Vector3.back * motorInput * speed);
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        for (int i = 0; i < wheels.Length; i++)
        {
            Gizmos.DrawRay(wheels[i].position, -transform.up * 1f);
        }
    }
}
