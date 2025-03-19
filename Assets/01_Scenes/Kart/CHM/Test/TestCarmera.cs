using UnityEngine;
using Cinemachine;

public class DriftCameraController : MonoBehaviour
{
    public CinemachineVirtualCamera virtualCamera; // Cinemachine Virtual Camera
    public TestKartController kartController; // 드리프트 여부를 확인할 컨트롤러

    [Header("Drift Camera Settings")]
    public float driftOffsetX = 5f; // 드리프트 시 X축 추가 오프셋
    public float driftSmoothTime = 0.5f; // 드리프트 중 오프셋 전환 시간

    private CinemachineTransposer transposer;
    private float originalOffsetX; // 원래 X 오프셋
    private float targetOffsetX; // 목표 X 오프셋
    private float currentOffsetX; // 현재 X 오프셋 (보간)
    private const float fixedOffsetZ = -10f; // Z 값을 항상 -10으로 고정

    private void Start()
    {
        // Transposer 컴포넌트 가져오기
        transposer = virtualCamera.GetCinemachineComponent<CinemachineTransposer>();
        if (transposer != null)
        {
            originalOffsetX = transposer.m_FollowOffset.x;
            currentOffsetX = originalOffsetX;

            // Z 오프셋을 -10으로 고정
            Vector3 followOffset = transposer.m_FollowOffset;
            followOffset.z = fixedOffsetZ;
            transposer.m_FollowOffset = followOffset;
        }
        else
        {
            Debug.LogWarning("Cinemachine Transposer가 설정되지 않았습니다!");
        }
    }

    private void LateUpdate()
    {
        if (kartController == null || transposer == null) return;

        // 드리프트 중일 경우 X 오프셋 이동
        if (kartController.isDrifting)
        {
            float driftDirection = kartController.currentDriftAngle > 0 ? 1f : -1f; // 드리프트 방향 감지
            targetOffsetX = originalOffsetX + (driftDirection * driftOffsetX);
        }
        else
        {
            targetOffsetX = originalOffsetX; // 드리프트가 아닐 경우 복구
        }

        // 부드럽게 X 오프셋 전환
        currentOffsetX = Mathf.Lerp(currentOffsetX, targetOffsetX, driftSmoothTime * Time.deltaTime);

        // Transposer의 Follow Offset 업데이트
        Vector3 followOffset = transposer.m_FollowOffset;
        followOffset.x = currentOffsetX;
        followOffset.z = fixedOffsetZ; // Z 오프셋을 항상 -10으로 고정
        transposer.m_FollowOffset = followOffset;
    }
}
