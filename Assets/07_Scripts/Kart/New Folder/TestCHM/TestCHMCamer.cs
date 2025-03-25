using UnityEngine;
using Cinemachine;

public class TestCHMCamer : MonoBehaviour
{
    [Header("Camera References")]
    [Tooltip("Cinemachine Virtual Camera 객체")]
    public CinemachineVirtualCamera virtualCamera;
    [Tooltip("카트를 따라갈 타겟 (TestKartController가 붙은 GameObject)")]
    public Transform target;
    [Tooltip("TestKartController 스크립트 참조")]
    public TestCHMKart kartController;

    [Header("Drift Camera Settings")]
    [Tooltip("드리프트 시 최대 팬 각도 (도 단위): 드리프트 방향의 반대쪽으로 회전")]
    public float maxPanAngleDelta = 15f;
    [Tooltip("오프셋 전환 부드러움 (낮을수록 전환이 빠름)")]
    public float smoothTime = 0.3f;

    private CinemachineTransposer transposer;
    private Vector3 initialOffset;
    private float baseDistance;   // x,z 평면상의 전체 거리 (고정)
    private float baseAngle;      // 초기 오프셋의 각도 (라디안)

    private void Start()
    {
        if (virtualCamera == null)
        {
            Debug.LogError("Virtual Camera 참조가 없습니다!");
            return;
        }
    }

    private void LateUpdate()
    {
        if (kartController == null || transposer == null) return;

        Vector3 currentOffset = transposer.m_FollowOffset;
        Vector3 targetOffset = initialOffset;

        if (kartController.isDrifting)
        {
            // 드리프트 강도(0~1): 현재 드리프트 각도의 절대값 / 최대 드리프트 각도
            float driftIntensity = Mathf.Clamp01(Mathf.Abs(kartController.currentDriftAngle) / kartController.maxDriftAngle);
            // 드리프트 방향: 우측이면 +1, 좌측이면 -1
            float driftSign = Mathf.Sign(kartController.currentDriftAngle);
            // 카메라가 드리프트 반대쪽으로 팬되어야 하므로, 
            // 새 각도는 기본 각도에서 (드리프트 강도×최대 팬각, 도 단위)를 드리프트 방향의 부호에 따라 빼줍니다.
            float newAngle = baseAngle - driftSign * driftIntensity * maxPanAngleDelta * Mathf.Deg2Rad;

            // 전체 수평 거리는 baseDistance로 고정하고, 새 각도에 따라 x, z 를 계산합니다.
            targetOffset.x = baseDistance * Mathf.Sin(newAngle);
            targetOffset.z = baseDistance * Mathf.Cos(newAngle);
            targetOffset.y = initialOffset.y; // y축은 그대로 유지
        }
        else
        {
            targetOffset = initialOffset;
        }

        // 부드러운 전환
        transposer.m_FollowOffset = Vector3.Lerp(currentOffset, targetOffset, Time.deltaTime / smoothTime);
    }

    public void SetKart(GameObject kart)
    {
        Debug.Log("SetKart 호출!");
        
        if(kart == null)
        {
            Debug.LogWarning("카트 참조가 없습니다!");
            return;
        }
        
        target = kart.transform;
        kartController = kart.GetComponent<TestCHMKart>();
        
        transposer = virtualCamera.GetCinemachineComponent<CinemachineTransposer>();
        if (transposer != null)
        {
            initialOffset = transposer.m_FollowOffset;
            Vector2 horiz = new Vector2(initialOffset.x, initialOffset.z);
            baseDistance = horiz.magnitude;
            // Convention: x = r*sin(theta), z = r*cos(theta)
            baseAngle = Mathf.Atan2(initialOffset.x, initialOffset.z);
        }
        else
        {
            Debug.LogWarning("Cinemachine Transposer 컴포넌트를 찾을 수 없습니다!");
        }        
    }
}