using UnityEngine;

public class AntiRollBar : MonoBehaviour
{
    public Rigidbody carRigidbody; // 차량의 Rigidbody
    public Transform wheelLeft; // 왼쪽 바퀴
    public Transform wheelRight; // 오른쪽 바퀴
    public float antiRollStrength = 5000f; // 안티롤 강도
    public float rayLength = 3f; // 레이 길이
    public LayerMask groundLayer; // Ground 레이어를 설정

    private void FixedUpdate()
    {
        // 바퀴 접지 상태 확인
        bool leftWheelGrounded = IsWheelGrounded(wheelLeft);
        bool rightWheelGrounded = IsWheelGrounded(wheelRight);

        // 롤링을 억제하는 힘 계산
        float antiRollForce = antiRollStrength;

        // 접지되지 않은 바퀴에만 힘 적용
        if (!leftWheelGrounded)
        {
            carRigidbody.AddForceAtPosition(wheelLeft.up * -antiRollForce, wheelLeft.position);
        }
        if (!rightWheelGrounded)
        {
            carRigidbody.AddForceAtPosition(wheelRight.up * antiRollForce, wheelRight.position);
        }
    }

    private bool IsWheelGrounded(Transform wheel)
    {
        // 레이를 아래로 쏘아 "Ground" 레이어와 충돌 확인
        RaycastHit hit;
        return Physics.Raycast(wheel.position, -wheel.up, out hit, rayLength, groundLayer);
    }
    private void OnDrawGizmos()
    {
        DrawWheelRayGizmos(wheelLeft, Color.red);
        DrawWheelRayGizmos(wheelRight, Color.blue);
    }

    private void DrawWheelRayGizmos(Transform wheel, Color gizmoColor)
    {
        Gizmos.color = gizmoColor;

        RaycastHit hit;
        if (Physics.Raycast(wheel.position, -wheel.up, out hit, rayLength, groundLayer))
        {
            // 바퀴가 접지된 경우 레이와 히트 지점 표시
            Gizmos.DrawLine(wheel.position, hit.point);
            Gizmos.DrawSphere(hit.point, 0.1f); // 히트 지점을 작은 구로 표시
        }
        else
        {
            // 접지되지 않은 경우 레이를 끝까지 그림
            Gizmos.DrawLine(wheel.position, wheel.position - wheel.up * rayLength);
        }
    }

}