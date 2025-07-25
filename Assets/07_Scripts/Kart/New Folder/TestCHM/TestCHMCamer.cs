using System.Collections;
using Cinemachine;
using UnityEngine;

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
    [Header("Finish Camera Settings")]
    [Tooltip("�ǴϽ� ī�޶� ������")]
    public Vector3 finishCameraOffset = new Vector3(0f, 5f, -10f);
    [Tooltip("�⺻ ���¿��� ī�޶� �� ���� (�� ����)")]
    public float basePanAngleDelta = 10f; // Inspector���� ���� ������ �⺻ �� ����
    [SerializeField]
    private float maxPanLimit = 30f; // �⺻ 30�� ����. (�ʿ信 ���� ����)
    [Header("Boost Camera Settings")]
    [Tooltip("�ν��� �� ī�޶� �ڷ� �и��� �Ÿ�")]
    public float boostDistanceOffset = 5f; // �⺻��: �ڷ� 5m
                                           // ���� ���� ���¸� Ȯ���ϴ� ������Ƽ
    [Tooltip("�ν��� �� ī�޶� ���� �ö󰡴� ����")]
    public float boostHeightOffset = 2f; // �⺻��: ���� 2m
    public bool isGameFinished { get; set; }

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
        transposer = virtualCamera.GetCinemachineComponent<CinemachineTransposer>();
        if (transposer != null)
        {
            initialOffset = transposer.m_FollowOffset; // �⺻ �ʱⰪ ����
        }
        else
        {
            Debug.LogWarning("Cinemachine Transposer ������Ʈ�� ã�� �� �����ϴ�!");
        }

    }

    private void FixedUpdate()
    {        
        // ������ ����Ǿ����� ī�޶� ������Ʈ �ߴ�
        if (isGameFinished)
        {
            return;
        }
        if (kartController == null || transposer == null) return;

        if (kartController != null)
        {
            if (kartController.isKartRotating == true)
            {
                if (virtualCamera.Follow != null)
                {
                    virtualCamera.Follow = null;
                }
            }
            else
            {
                if (virtualCamera.Follow == null)
                {
                    virtualCamera.Follow = target;
                }
            }
        }

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
            float lerpFactor = kartController.isDrifting ? Time.deltaTime / (smoothTime * 1.5f) : Time.fixedDeltaTime / smoothTime;
            transposer.m_FollowOffset = Vector3.Lerp(currentOffset, targetOffset, lerpFactor); ;
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

            transposer.m_FollowOffset = Vector3.Lerp(currentOffset, targetOffset, Time.fixedDeltaTime / smoothTime);
        }
        else if (kartController.isBoostTriggered) // �ν��� ����
        {
            // ī�޶� �ڷ� �и��� ���� ���
            targetOffset += new Vector3(0f, boostHeightOffset, -boostDistanceOffset);
            transposer.m_FollowOffset = Vector3.Lerp(currentOffset, targetOffset, Time.fixedDeltaTime / (smoothTime * 0.5f)); // �� ������ ��ȯ
        }

        // �ε巯�� ��ȯ
        transposer.m_FollowOffset = Vector3.Lerp(currentOffset, targetOffset, Time.fixedDeltaTime / smoothTime);
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
    /// <summary>
    /// �ǴϽ� ī�޶� Ȱ��ȭ�ϰ�, ������ ���������� ������ �̵�.
    /// </summary>
    public void ActivateFinishCamera()
    {
        if (virtualCamera == null || target == null)
        {
            Debug.LogWarning("Virtual Camera �Ǵ� Ÿ���� �����ϴ�!");
            return;
        }

        // ���� ���� ���� ����
        isGameFinished = true;

        // CinemachineTransposer�� ������
        CinemachineTransposer transposer = virtualCamera.GetCinemachineComponent<CinemachineTransposer>();
        if (transposer == null)
        {
            Debug.LogWarning("Cinemachine Transposer ������Ʈ�� ã�� �� �����ϴ�!");
            return;
        }

        // ��ǥ ��ġ�� ������ ������ ������ ����
        Vector3 targetOffset = finishCameraOffset;
        // �ڷ�ƾ�� ���� ������ Follow Offset ����
        StartCoroutine(MoveCameraToOffset(transposer,targetOffset,2f,virtualCamera));        
    }

    private IEnumerator MoveCameraToOffset(CinemachineTransposer transposer, Vector3 targetOffset, float duration, CinemachineVirtualCamera virtualCamera)
    {
        float timer = 0f;

        // �ʱ� ������ �� ���� ���
        float radius = Mathf.Sqrt(Mathf.Pow(transposer.m_FollowOffset.x, 2) + Mathf.Pow(transposer.m_FollowOffset.z, 2));
        float startAngle = Mathf.Atan2(transposer.m_FollowOffset.z, transposer.m_FollowOffset.x); // �ʱ� ����
        float endAngle = Mathf.Atan2(targetOffset.z, targetOffset.x); // ��ǥ ����

        while (timer < duration)
        {
            timer += Time.deltaTime;

            // ���� ���� ��� (�ð��� ���� ���������� ��ȭ)
            float currentAngle = Mathf.Lerp(startAngle, endAngle, timer / duration);

            // X, Z ��ǥ�� ���� ��η� ���
            float newX = radius * Mathf.Cos(currentAngle) + 2f;
            float newZ = radius * Mathf.Sin(currentAngle);

            // Y ���� ������ ��ǥ ���������� ���� �����Ͽ� �̵�
            float newY = Mathf.Lerp(transposer.m_FollowOffset.y, targetOffset.y, timer / duration);

            // ���ο� ������ ���
            Vector3 newOffset = new Vector3(newX, newY, newZ);

            // Follow Offset ������Ʈ
            transposer.m_FollowOffset = newOffset;

            // ��ǥ �����¿� �����ϸ� �̵� ����
            if (Vector3.Distance(newOffset, targetOffset) <= 0.01f)
            {
                break;
            }

            yield return null; // ���� �����ӱ��� ���
        }

        // ���������� ��ǥ ������ �� ����
        transposer.m_FollowOffset = targetOffset;
        Debug.Log("ī�޶� ������ �����¿� �����ϰ� ������ϴ�!");

        virtualCamera.LookAt = null;
        // Cinemachine ���� ����       
        virtualCamera.m_Lens.FieldOfView = 70f;


    }
}