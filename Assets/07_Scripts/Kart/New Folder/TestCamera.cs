using UnityEngine;
using Cinemachine;

public class TestCamera : MonoBehaviour
{
    public CinemachineVirtualCamera virtualCamera; // Cinemachine Virtual Camera
    public Transform kart; // īƮ Transform
    public float driftOffsetX = 2f; // �帮��Ʈ �� X�� �ִ� �߰� �̵���
    public float driftOffsetZ = 1f; // �帮��Ʈ �� Z�� �ִ� �߰� �̵��� (zoom out ȿ��)
    public float driftSmoothTime = 0.5f; // ��ȯ �ε巯�� �ӵ�

    private CinemachineTransposer transposer;
    private Vector3 initialOffset; // �ʱ� Follow Offset
    private TestKartController kartController; // īƮ ��Ʈ�ѷ� ��ũ��Ʈ

    // ī�޶��� ���� X �� ������ (������)
    private float currentOffsetX;

    void Start()
    {
        if (kart != null)
        {
            kartController = kart.GetComponent<TestKartController>();
        }

        // Cinemachine Transposer ������Ʈ ��������
        transposer = virtualCamera.GetCinemachineComponent<CinemachineTransposer>();
        if (transposer != null)
        {
            initialOffset = transposer.m_FollowOffset;
            currentOffsetX = initialOffset.x;
        }
        else
        {
            Debug.LogWarning("Cinemachine Transposer�� ã�� �� �����ϴ�!");
        }
    }

    void LateUpdate()
    {
        if (kartController == null || transposer == null) return;

        // KartRider ��Ÿ���� ī�޶� ȿ��: �帮��Ʈ ����(0~1)�� ���
        // ���⼭�� TestKartController���� currentDriftAngle, maxDriftAngle ������ ����Ѵٰ� �����մϴ�.
        if (kartController.isDrifting)
        {
            // �帮��Ʈ ����: ���� �帮��Ʈ ������ ���밪�� �ִ� �帮��Ʈ ������ �����ϴ�.
            float driftIntensity = Mathf.Clamp01(Mathf.Abs(kartController.currentDriftAngle) / kartController.maxDriftAngle);
            // �帮��Ʈ ����: �������̸� +1, �����̸� -1
            float driftSign = Mathf.Sign(kartController.currentDriftAngle);

            // ��ǥ X ������ ����: ������ ���� �ʱ� �����¿� driftOffsetX ��ŭ ���ϰų� ���ݴϴ�.
            float targetOffsetX = initialOffset.x + driftSign * driftIntensity * driftOffsetX;
            // ��ǥ Z ������: �ӵ����� ���� �ణ �ڷ� ���������� �մϴ�.
            float targetOffsetZ = initialOffset.z - driftIntensity * driftOffsetZ;

            // ���� FollowOffset�� �ε巴�� Lerp ó��
            Vector3 followOffset = transposer.m_FollowOffset;
            followOffset.x = Mathf.Lerp(currentOffsetX, targetOffsetX, Time.deltaTime / driftSmoothTime);
            followOffset.z = Mathf.Lerp(followOffset.z, targetOffsetZ, Time.deltaTime / driftSmoothTime);
            transposer.m_FollowOffset = followOffset;

            currentOffsetX = followOffset.x;
        }
        else
        {
            // �帮��Ʈ�� �����Ǿ����� ������ ī�޶� ���������� �ε巴�� ����
            Vector3 followOffset = transposer.m_FollowOffset;
            followOffset.x = Mathf.Lerp(currentOffsetX, initialOffset.x, Time.deltaTime / driftSmoothTime);
            followOffset.z = Mathf.Lerp(followOffset.z, initialOffset.z, Time.deltaTime / driftSmoothTime);
            transposer.m_FollowOffset = followOffset;

            currentOffsetX = followOffset.x;
        }
    }
}