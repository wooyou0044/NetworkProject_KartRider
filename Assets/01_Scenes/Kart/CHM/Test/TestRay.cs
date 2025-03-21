using UnityEngine;

public class CapsuleRaycast : MonoBehaviour
{
    public float radius = 0.5f; // 캡슐의 반지름
    public float height = 3.0f; // 캡슐의 길이
    public float centerY = 0.6f; // 캡슐 중심의 Y값
    public LayerMask playerLayer; // 플레이어 레이어 설정

    void Update()
    {
        // 캡슐의 두 끝점을 Z-Axis 기준으로 계산
        Vector3 point1 = transform.position + transform.forward * (height / 2 - radius); // 앞쪽 끝
        Vector3 point2 = transform.position - transform.forward * (height / 2 - radius); // 뒤쪽 끝

        // 캡슐 캐스트 실행
        RaycastHit hit;
        bool detected = Physics.CapsuleCast(point1, point2, radius, transform.up, out hit, Mathf.Infinity, playerLayer);

        if (detected)
        {
            Debug.Log("플레이어 감지됨: " + hit.collider.name);
        }
    }

    // 기즈모를 그리는 메서드
    void OnDrawGizmos()
    {
        Gizmos.color = Color.green; // 캡슐의 색상 설정
        Vector3 point1 = transform.position + transform.forward * (height / 2 - radius);
        Vector3 point2 = transform.position - transform.forward * (height / 2 - radius);

        // 캡슐 모양 기즈모 그리기
        Gizmos.DrawWireSphere(point1, radius);
        Gizmos.DrawWireSphere(point2, radius);
        Gizmos.DrawLine(point1 + Vector3.up * radius, point2 + Vector3.up * radius);
        Gizmos.DrawLine(point1 - Vector3.up * radius, point2 - Vector3.up * radius);
        Gizmos.DrawLine(point1 + Vector3.right * radius, point2 + Vector3.right * radius);
        Gizmos.DrawLine(point1 - Vector3.right * radius, point2 - Vector3.right * radius);

        // 캐스트 방향 표시
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.up * 5.0f); // 5.0f는 레이 길이
    }
}