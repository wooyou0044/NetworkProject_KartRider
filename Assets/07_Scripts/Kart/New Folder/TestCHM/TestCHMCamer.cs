using UnityEngine;
using Cinemachine;

public class TestCHMCamer : MonoBehaviour
{
    [Header("Camera References")]
    [Tooltip("Cinemachine Virtual Camera ��ü")]
    public CinemachineVirtualCamera virtualCamera;
    [Tooltip("īƮ�� ���� Ÿ�� (TestKartController�� ���� GameObject)")]
    public Transform target;
    [Tooltip("TestKartController ��ũ��Ʈ ����")]
    public TestCHMKart kartController;

    [Header("Drift Camera Settings")]
    [Tooltip("�帮��Ʈ �� �ִ� �� ���� (�� ����): �帮��Ʈ ������ �ݴ������� ȸ��")]
    public float maxPanAngleDelta = 15f;
    [Tooltip("������ ��ȯ �ε巯�� (�������� ��ȯ�� ����)")]
    public float smoothTime = 0.3f;
    
    [Tooltip("�⺻ ���¿��� ī�޶� �� ���� (�� ����)")]
    public float basePanAngleDelta = 10f; // Inspector���� ���� ������ �⺻ �� ����
    [SerializeField]
    private float maxPanLimit = 30f; // �⺻ 30�� ����. (�ʿ信 ���� ����)
    [Header("Boost Camera Settings")]
    [Tooltip("�ν��� �� ī�޶� �ڷ� �и��� �Ÿ�")]
    public float boostDistanceOffset = 5f; // �⺻��: �ڷ� 5m

    [Tooltip("�ν��� �� ī�޶� ���� �ö󰡴� ����")]
    public float boostHeightOffset = 2f; // �⺻��: ���� 2m

    private CinemachineTransposer transposer;
    private Vector3 initialOffset;
    private float baseDistance;   // x,z ������ ��ü �Ÿ� (����)
    private float baseAngle;      // �ʱ� �������� ���� (����)

    private void Start()
    {
        if (virtualCamera == null)
        {
            Debug.LogError("Virtual Camera ������ �����ϴ�!");
            return;
        }
    }

    private void FixedUpdate()
    {
        if (kartController == null || transposer == null) return;

        Vector3 currentOffset = transposer.m_FollowOffset; // ���� ī�޶� ������
        Vector3 targetOffset = initialOffset;             // �⺻ ī�޶� ��ġ

        // ������ �¿� �Է� ���� (-1 ~ 1 ����)
        // ��: �ִ� ��0.25 �̻��� �Է� ��ȭ�� ������ ����
        float rawInput = Input.GetAxis("Horizontal") * 0.25f;
        float horizontalInput = Mathf.Clamp(rawInput, -0.25f, 0.25f);

        // ������ �ӵ��� �¿� �Է� ������ ���� ī�޶� �Ұ� ���� ����
        if (!kartController.isBoostTriggered && !kartController.isDrifting) // �⺻ ����
        {
            float basePanAngle = baseAngle + horizontalInput * basePanAngleDelta * Mathf.Deg2Rad;
            // īƮ���̴�ó�� ī�޶� �ʹ� �ش����� ������ �̵����� �ʵ��� -maxPanLimit ~ +maxPanLimit ������ ����
            float maxPanLimit = 30 * Mathf.Deg2Rad; // 30�� ������ �ִ� �Ѱ�� ����
            basePanAngle = Mathf.Clamp(basePanAngle, baseAngle - maxPanLimit, baseAngle + maxPanLimit);

            // �� ������ ������ ���
            targetOffset.x = baseDistance * Mathf.Sin(basePanAngle);
            targetOffset.z = baseDistance * Mathf.Cos(basePanAngle);
            targetOffset.y = initialOffset.y + horizontalInput * 0.3f; // ���� ȿ�� �߰�
            float lerpFactor = kartController.isDrifting ? Time.deltaTime / (smoothTime * 1.5f) : Time.deltaTime / smoothTime;
            transposer.m_FollowOffset = Vector3.Lerp(currentOffset, targetOffset, lerpFactor); ;
            Debug.Log(lerpFactor + "  :  "  + Time.deltaTime);
        }
        else if (kartController.isDrifting)
        {
            float driftIntensity = Mathf.Clamp01(Mathf.Abs(kartController.currentDriftAngle) / kartController.maxDriftAngle);
            float driftSign = Mathf.Sign(kartController.currentDriftAngle);

            // ī�޶� ���� ����ġ�� ���� �̵��� ���� �ʵ��� �⺻ ���������� ������ ���� ������ ����
            float driftAngle = baseAngle - driftSign * driftIntensity * maxPanAngleDelta * Mathf.Deg2Rad;
            // ���� Ư�� ���� ���� ������ �ݴϴ�.
            driftAngle = Mathf.Clamp(driftAngle, baseAngle - maxPanLimit, baseAngle + maxPanLimit);

            targetOffset.x = baseDistance * Mathf.Sin(driftAngle);
            targetOffset.z = baseDistance * Mathf.Cos(driftAngle);
            targetOffset.y = initialOffset.y;

            transposer.m_FollowOffset = Vector3.Lerp(currentOffset, targetOffset, Time.deltaTime / smoothTime);
        }
        else if (kartController.isBoostTriggered) // �ν��� ����
        {
            // ī�޶� �ڷ� �и��� ���� ���
            targetOffset += new Vector3(0f, boostHeightOffset, -boostDistanceOffset);
            transposer.m_FollowOffset = Vector3.Lerp(currentOffset, targetOffset, Time.deltaTime / (smoothTime * 0.5f)); // �� ������ ��ȯ
        }

        // �ε巯�� ��ȯ
        transposer.m_FollowOffset = Vector3.Lerp(currentOffset, targetOffset, Time.deltaTime / smoothTime);
    }
    public void SetKart(GameObject kart)
    {
        Debug.Log("SetKart ȣ��!");
        if (kart == null)
        {
            Debug.LogWarning("īƮ ������ �����ϴ�!");
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
            baseAngle = Mathf.Atan2(initialOffset.x, initialOffset.z); // �ʱ� ���� ���
        }
        else
        {
            Debug.LogWarning("Cinemachine Transposer ������Ʈ�� ã�� �� �����ϴ�!");
        }
    }
}