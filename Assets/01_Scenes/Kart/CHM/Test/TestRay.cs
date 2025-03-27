using UnityEngine;

public class CapsuleRaycast : MonoBehaviour
{
    public float radius = 0.5f; // ĸ���� ������
    public float height = 3.0f; // ĸ���� ����
    public float centerY = 0.6f; // ĸ�� �߽��� Y��
    public LayerMask playerLayer; // �÷��̾� ���̾� ����

    void Update()
    {
        // ĸ���� �� ������ Z-Axis �������� ���
        Vector3 point1 = transform.position + transform.forward * (height / 2 - radius); // ���� ��
        Vector3 point2 = transform.position - transform.forward * (height / 2 - radius); // ���� ��

        // ĸ�� ĳ��Ʈ ����
        RaycastHit hit;
        bool detected = Physics.CapsuleCast(point1, point2, radius, transform.up, out hit, Mathf.Infinity, playerLayer);

        if (detected)
        {
            Debug.Log("�÷��̾� ������: " + hit.collider.name);
        }
    }

    // ����� �׸��� �޼���
    void OnDrawGizmos()
    {
        Gizmos.color = Color.green; // ĸ���� ���� ����
        Vector3 point1 = transform.position + transform.forward * (height / 2 - radius);
        Vector3 point2 = transform.position - transform.forward * (height / 2 - radius);

        // ĸ�� ��� ����� �׸���
        Gizmos.DrawWireSphere(point1, radius);
        Gizmos.DrawWireSphere(point2, radius);
        Gizmos.DrawLine(point1 + Vector3.up * radius, point2 + Vector3.up * radius);
        Gizmos.DrawLine(point1 - Vector3.up * radius, point2 - Vector3.up * radius);
        Gizmos.DrawLine(point1 + Vector3.right * radius, point2 + Vector3.right * radius);
        Gizmos.DrawLine(point1 - Vector3.right * radius, point2 - Vector3.right * radius);

        // ĳ��Ʈ ���� ǥ��
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.up * 5.0f); // 5.0f�� ���� ����
    }
}