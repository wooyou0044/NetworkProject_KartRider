using UnityEngine;

public class AntiRollBar : MonoBehaviour
{
    public Rigidbody carRigidbody; // ������ Rigidbody
    public Transform wheelLeft; // ���� ����
    public Transform wheelRight; // ������ ����
    public float antiRollStrength = 5000f; // ��Ƽ�� ����
    public float rayLength = 3f; // ���� ����
    public LayerMask groundLayer; // Ground ���̾ ����

    private void FixedUpdate()
    {
        // ���� ���� ���� Ȯ��
        bool leftWheelGrounded = IsWheelGrounded(wheelLeft);
        bool rightWheelGrounded = IsWheelGrounded(wheelRight);

        // �Ѹ��� �����ϴ� �� ���
        float antiRollForce = antiRollStrength;

        // �������� ���� �������� �� ����
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
        // ���̸� �Ʒ��� ��� "Ground" ���̾�� �浹 Ȯ��
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
            // ������ ������ ��� ���̿� ��Ʈ ���� ǥ��
            Gizmos.DrawLine(wheel.position, hit.point);
            Gizmos.DrawSphere(hit.point, 0.1f); // ��Ʈ ������ ���� ���� ǥ��
        }
        else
        {
            // �������� ���� ��� ���̸� ������ �׸�
            Gizmos.DrawLine(wheel.position, wheel.position - wheel.up * rayLength);
        }
    }

}