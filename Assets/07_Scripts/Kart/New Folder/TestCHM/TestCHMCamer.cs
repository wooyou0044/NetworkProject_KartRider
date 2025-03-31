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
    
    [Tooltip("기본 상태에서 카메라 팬 각도 (도 단위)")]
    public float basePanAngleDelta = 10f; // Inspector에서 조정 가능한 기본 팬 각도
    [SerializeField]
    private float maxPanLimit = 30f; // 기본 30도 제한. (필요에 따라 조정)
    [Header("Boost Camera Settings")]
    [Tooltip("부스터 시 카메라가 뒤로 밀리는 거리")]
    public float boostDistanceOffset = 5f; // 기본값: 뒤로 5m

    [Tooltip("부스터 시 카메라가 위로 올라가는 높이")]
    public float boostHeightOffset = 2f; // 기본값: 위로 2m

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

    private void FixedUpdate()
    {
        if (kartController == null || transposer == null) return;

        if (kartController != null)
        {
            if (kartController.isKartRotating == true)
            {
                Debug.Log("isKartRotating 들어옴");
                if (virtualCamera.Follow != null)
                {
                    virtualCamera.Follow = null;
                }
            }
            else
            {
                Debug.Log("isKartRotating은 False 들어옴");
                if (virtualCamera.Follow == null)
                {
                    virtualCamera.Follow = target;
                }
            }
        }

        Vector3 currentOffset = transposer.m_FollowOffset; // 현재 카메라 오프셋
        Vector3 targetOffset = initialOffset;             // 기본 카메라 위치

        // 차량의 좌우 입력 감지 (-1 ~ 1 범위)
        // 예: 최대 ±0.25 이상의 입력 변화가 없도록 제한
        float rawInput = Input.GetAxis("Horizontal") * 0.25f;
        float horizontalInput = Mathf.Clamp(rawInput, -0.25f, 0.25f);

        // 차량의 속도와 좌우 입력 비율에 따라 카메라 팬과 기울기 적용
        if (!kartController.isBoostTriggered && !kartController.isDrifting) // 기본 상태
        {
            float basePanAngle = baseAngle + horizontalInput * basePanAngleDelta * Mathf.Deg2Rad;
            // 카트라이더처럼 카메라가 너무 극단적인 각도로 이동하지 않도록 -maxPanLimit ~ +maxPanLimit 범위로 제한
            float maxPanLimit = 30 * Mathf.Deg2Rad; // 30도 정도를 최대 한계로 예시
            basePanAngle = Mathf.Clamp(basePanAngle, baseAngle - maxPanLimit, baseAngle + maxPanLimit);

            // 팬 각도와 오프셋 계산
            targetOffset.x = baseDistance * Mathf.Sin(basePanAngle);
            targetOffset.z = baseDistance * Mathf.Cos(basePanAngle);
            targetOffset.y = initialOffset.y + horizontalInput * 0.3f; // 기울기 효과 추가
            float lerpFactor = kartController.isDrifting ? Time.deltaTime / (smoothTime * 1.5f) : Time.deltaTime / smoothTime;
            transposer.m_FollowOffset = Vector3.Lerp(currentOffset, targetOffset, lerpFactor); ;
        }
        else if (kartController.isDrifting)
        {
            float driftIntensity = Mathf.Clamp01(Mathf.Abs(kartController.currentDriftAngle) / kartController.maxDriftAngle);
            float driftSign = Mathf.Sign(kartController.currentDriftAngle);

            // 카메라 팬이 지나치게 많은 이동을 하지 않도록 기본 각도에서만 한정된 범위 내에서 보정
            float driftAngle = baseAngle - driftSign * driftIntensity * maxPanAngleDelta * Mathf.Deg2Rad;
            // 역시 특정 범위 내로 제한해 줍니다.
            driftAngle = Mathf.Clamp(driftAngle, baseAngle - maxPanLimit, baseAngle + maxPanLimit);

            targetOffset.x = baseDistance * Mathf.Sin(driftAngle);
            targetOffset.z = baseDistance * Mathf.Cos(driftAngle);
            targetOffset.y = initialOffset.y;

            transposer.m_FollowOffset = Vector3.Lerp(currentOffset, targetOffset, Time.deltaTime / smoothTime);
        }
        else if (kartController.isBoostTriggered) // 부스터 상태
        {
            // 카메라 뒤로 밀리고 위로 상승
            targetOffset += new Vector3(0f, boostHeightOffset, -boostDistanceOffset);
            transposer.m_FollowOffset = Vector3.Lerp(currentOffset, targetOffset, Time.deltaTime / (smoothTime * 0.5f)); // 더 빠르게 전환
        }

        // 부드러운 전환
        transposer.m_FollowOffset = Vector3.Lerp(currentOffset, targetOffset, Time.deltaTime / smoothTime);
    }
    public void SetKart(GameObject kart)
    {
        Debug.Log("SetKart 호출!");
        if (kart == null)
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
            baseAngle = Mathf.Atan2(initialOffset.x, initialOffset.z); // 초기 각도 계산
        }
        else
        {
            Debug.LogWarning("Cinemachine Transposer 컴포넌트를 찾을 수 없습니다!");
        }
    }
}