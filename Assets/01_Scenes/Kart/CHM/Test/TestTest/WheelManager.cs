using UnityEngine;

public class WheelManager : MonoBehaviour
{
    [Header("Wheel Settings")]
    public GameObject[] wheels; // 0, 1: 앞바퀴 / 2, 3: 뒷바퀴
    public GameObject skidMarkPrefab; // 스키드마크 프리팹 (TrailRenderer)
    private TrailRenderer[] skidMarks; // TrailRenderer 배열

    private void Awake()
    {
       
        // 뒷바퀴(2, 3번)에만 스키드마크 연결
        skidMarks = new TrailRenderer[2];
        for (int i = 2; i < wheels.Length; i++) // 뒷바퀴만 처리
        {
            GameObject skidMarkObject = Instantiate(skidMarkPrefab, wheels[i].transform);
            skidMarks[i - 2] = skidMarkObject.GetComponent<TrailRenderer>();
            skidMarks[i - 2].emitting = false; // 초기에는 비활성화
        }
    }

    public void UpdateAndRotateWheels(float steerInput, float motorInput, float speed, bool isDrifting)
    {
        for (int i = 0; i < wheels.Length; i++)
        {
            // 바퀴 Z축 회전 처리 (굴림 효과)
            wheels[i].transform.Rotate(Vector3.forward, speed * Time.deltaTime * motorInput);

            // 앞바퀴 조향 처리 (0, 1번 바퀴)
            if (i == 0 || i == 1)
            {
                float steerAngle = steerInput * 10f; // 조향 각도
                wheels[i].transform.localEulerAngles = new Vector3(90 , steerAngle, 0);
            }

            // 뒷바퀴(2, 3번)는 굴림만 처리, 조향 없음
        }

        // 스키드마크 상태 갱신
        UpdateSkidMarks(isDrifting);
    }

    private void UpdateSkidMarks(bool isDrifting)
    {
        // 드리프트 중일 때 뒷바퀴에만 스키드마크 활성화
        for (int i = 0; i < skidMarks.Length; i++)
        {
            skidMarks[i].emitting = isDrifting;
        }
    }
}