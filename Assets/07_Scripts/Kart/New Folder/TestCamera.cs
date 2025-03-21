using UnityEngine;
using Cinemachine;

public class TestCamera : MonoBehaviour
{
    public CinemachineVirtualCamera virtualCamera; // Cinemachine Virtual Camera
    public Transform kart; // 카트 Transform
    public float driftOffsetX = 0f; // 드리프트 시 X축 추가 이동량
    public float driftOffsetZ = 0f; // 드리프트 시 Z축 추가 이동량
    public float driftSmoothTime = 0.5f; // 전환 부드러움 속도

    private CinemachineTransposer transposer;
    private Vector3 initialOffset; // 초기 Follow Offset
    private CHMTestKartController kartController; // 카트 컨트롤러 스크립트
    private float targetOffsetX; // 목표 X 오프셋
    private float currentOffsetX; // 현재 X 오프셋 (가변)

    void Start()
    {
        if (kart != null)
        {
            kartController = kart.GetComponent<CHMTestKartController>();
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

   // void LateUpdate()
   // {
   //     if (kartController == null || transposer == null) return;
//
   //     // 드리프트 상태라면 X와 Z축 이동
   //     if (kartController.isDrifting)
   //     {
   //         float driftDirection = kartController.steeringDirection; // 드리프트 방향
   //         targetOffsetX = initialOffset.x + (driftDirection * driftOffsetX);
//
   //         // Follow Offset의 Z축도 업데이트
   //         Vector3 followOffset = transposer.m_FollowOffset;
   //         followOffset.z = initialOffset.z - driftOffsetZ;
//
   //         // 부드럽게 X, Z 반영
   //         followOffset.x = Mathf.Lerp(currentOffsetX, targetOffsetX, Time.deltaTime / driftSmoothTime);
   //         transposer.m_FollowOffset = followOffset;
//
   //         // X축 현재 오프셋 저장
   //         currentOffsetX = followOffset.x;
   //     }
   //     else
   //     {
   //         // 드리프트가 해제되었을 때 초기 Offset으로 복구
   //         Vector3 followOffset = transposer.m_FollowOffset;
   //         followOffset.x = Mathf.Lerp(currentOffsetX, initialOffset.x, Time.deltaTime / driftSmoothTime);
   //         followOffset.z = Mathf.Lerp(followOffset.z, initialOffset.z, Time.deltaTime / driftSmoothTime);
   //         transposer.m_FollowOffset = followOffset;
//
   //         currentOffsetX = followOffset.x;
   //     }
   // }
}
