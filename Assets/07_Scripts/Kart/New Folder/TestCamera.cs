using UnityEngine;
using Cinemachine;

public class TestCamera : MonoBehaviour
{
    public CinemachineVirtualCamera virtualCamera; // Cinemachine Virtual Camera
    public Transform kart; // 카트 Transform
    public float driftOffsetX = 2f; // 드리프트 시 X축 최대 추가 이동량
    public float driftOffsetZ = 1f; // 드리프트 시 Z축 최대 추가 이동량 (zoom out 효과)
    public float driftSmoothTime = 0.5f; // 전환 부드러움 속도

    private CinemachineTransposer transposer;
    private Vector3 initialOffset; // 초기 Follow Offset
    private TestKartController kartController; // 카트 컨트롤러 스크립트

    // 카메라의 가변 X 축 오프셋 (보간용)
    private float currentOffsetX;

    void Start()
    {
        if (kart != null)
        {
            kartController = kart.GetComponent<TestKartController>();
        }

        // Cinemachine Transposer 컴포넌트 가져오기
        transposer = virtualCamera.GetCinemachineComponent<CinemachineTransposer>();
        if (transposer != null)
        {
            initialOffset = transposer.m_FollowOffset;
            currentOffsetX = initialOffset.x;
        }
        else
        {
            Debug.LogWarning("Cinemachine Transposer를 찾을 수 없습니다!");
        }
    }

    void LateUpdate()
    {
        if (kartController == null || transposer == null) return;

        // KartRider 스타일의 카메라 효과: 드리프트 강도(0~1)를 계산
        // 여기서는 TestKartController에서 currentDriftAngle, maxDriftAngle 변수를 사용한다고 가정합니다.
        if (kartController.isDrifting)
        {
            // 드리프트 강도: 현재 드리프트 각도의 절대값을 최대 드리프트 각도로 나눕니다.
            float driftIntensity = Mathf.Clamp01(Mathf.Abs(kartController.currentDriftAngle) / kartController.maxDriftAngle);
            // 드리프트 방향: 오른쪽이면 +1, 왼쪽이면 -1
            float driftSign = Mathf.Sign(kartController.currentDriftAngle);

            // 목표 X 오프셋 보간: 강도에 따라 초기 오프셋에 driftOffsetX 만큼 더하거나 빼줍니다.
            float targetOffsetX = initialOffset.x + driftSign * driftIntensity * driftOffsetX;
            // 목표 Z 오프셋: 속도감을 위해 약간 뒤로 물러나도록 합니다.
            float targetOffsetZ = initialOffset.z - driftIntensity * driftOffsetZ;

            // 현재 FollowOffset을 부드럽게 Lerp 처리
            Vector3 followOffset = transposer.m_FollowOffset;
            followOffset.x = Mathf.Lerp(currentOffsetX, targetOffsetX, Time.deltaTime / driftSmoothTime);
            followOffset.z = Mathf.Lerp(followOffset.z, targetOffsetZ, Time.deltaTime / driftSmoothTime);
            transposer.m_FollowOffset = followOffset;

            currentOffsetX = followOffset.x;
        }
        else
        {
            // 드리프트가 해제되었으면 원래의 카메라 오프셋으로 부드럽게 복귀
            Vector3 followOffset = transposer.m_FollowOffset;
            followOffset.x = Mathf.Lerp(currentOffsetX, initialOffset.x, Time.deltaTime / driftSmoothTime);
            followOffset.z = Mathf.Lerp(followOffset.z, initialOffset.z, Time.deltaTime / driftSmoothTime);
            transposer.m_FollowOffset = followOffset;

            currentOffsetX = followOffset.x;
        }
    }
}