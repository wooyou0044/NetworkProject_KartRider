using UnityEngine;
using Cinemachine;

public class DriftCameraController : MonoBehaviour
{
    public CinemachineVirtualCamera virtualCamera; // Cinemachine Virtual Camera
    public TestKartController kartController; // 카트 컨트롤러 참조

    [Header("Drift Camera Settings")]
    public float driftOffsetX = 5f; // 드리프트 시 X축 추가 이동량
    public float driftSmoothTime = 0.5f; // 드리프트 시 전환 속도

    private CinemachineTransposer transposer;
    private float originalOffsetX; // 기본 X 오프셋
    private float targetOffsetX; // 목표 X 오프셋
    private float currentOffsetX; // 현재 X 오프셋 (가변)

    private void Start()
    {
        // Transposer 컴포넌트 가져오기
        transposer = virtualCamera.GetCinemachineComponent<CinemachineTransposer>();
        if (transposer != null)
        {
            originalOffsetX = transposer.m_FollowOffset.x;
            currentOffsetX = originalOffsetX;
        }
        else
        {
            Debug.LogWarning("Cinemachine Transposer를 찾을 수 없습니다!");
        }
    }

    private void LateUpdate()
    {
        if (kartController == null || transposer == null) return;

        // 드리프트 상태일 때 X 오프셋 이동
        if (kartController.isDrifting)
        {
           float driftDirection = kartController.currentDriftAngle > 0 ? 1f : -1f; // 드리프트 방향
           targetOffsetX = originalOffsetX + (driftDirection * driftOffsetX);
        }
        else
        {
            targetOffsetX = originalOffsetX; // 드리프트가 아닐 경우 기본값 복원
        }

        // 부드럽게 X 오프셋 전환
        currentOffsetX = Mathf.Lerp(currentOffsetX, targetOffsetX, driftSmoothTime * Time.deltaTime);

        // Transposer의 Follow Offset 업데이트
        Vector3 followOffset = transposer.m_FollowOffset;
        followOffset.x = currentOffsetX;
        transposer.m_FollowOffset = followOffset;
    }
}
