using System.Collections;
using UnityEngine;

public class CHMTestKartController : MonoBehaviour
{
    [Header("Speed Settings")]
    [SerializeField] private float maxSpeed = 200f; // km/h ���� �ִ� �ӵ�
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float deceleration = 15f;

    [Header("Steering Settings")]
    [SerializeField] private float steeringForce = 200f;

    [Header("Drift Settings")]
    [SerializeField] private float driftSpeedReduction = 0.8f; // �帮��Ʈ ���۽� �ӵ� ���� ����
    [SerializeField] private float driftForce = 50f;           // �帮��Ʈ �� Ⱦ�� (AddForce ���)
    [SerializeField] private float driftSteerStrength = 0.5f;    // �帮��Ʈ �� �ణ�� ���� ȿ��

    [Header("Motor Force")]
    [SerializeField] private float motorForce = 500f;

    // ��Ű�� ��ũ ���� ȿ���� ���� ����(������Ʈ�� ���� ����)
    public CHMTestWheelController CHM;

    private Rigidbody rigid;
    private float currentSpeed = 0f; // km/h ����
    private float inputVertical;
    private float inputHorizontal;
    private bool isDrifting = false;

    // �帮��Ʈ ���� ����
    private float driftStartTime;
    private Vector3 driftDirection;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        // �Է� �� �޾ƿ���
        inputVertical = Input.GetAxis("Vertical");
        inputHorizontal = Input.GetAxis("Horizontal");

        // �帮��Ʈ ���� ����:
        // - LeftShift ����, �¿� �Է��� �ְ�, ���� �ӵ��� ���� �̻��� ��
        if (Input.GetKeyDown(KeyCode.LeftShift) && Mathf.Abs(inputHorizontal) > 0.1f && Mathf.Abs(currentSpeed) > maxSpeed * 0.2f)
        {
            StartDrift();
        }

        // �帮��Ʈ ���� ����: LeftShift Ű ������ ��
        if (Input.GetKeyUp(KeyCode.LeftShift) && isDrifting)
        {
            StopDrift();
        }
    }

    private void FixedUpdate()
    {
        // �帮��Ʈ ���� �ƴҶ��� �Ϲ� �̵��� ���� ó��
        if (!isDrifting)
        {
            HandleAcceleration();
            HandleSteering();
        }
    }

    // ����/������ rigidbody.velocity ���� ���� ���� (km/h -> m/s ��ȯ)
    private void HandleAcceleration()
    {
        if (inputVertical != 0)
        {
            currentSpeed += inputVertical * acceleration * Time.fixedDeltaTime;
            currentSpeed = Mathf.Clamp(currentSpeed, -maxSpeed, maxSpeed);
        }
        else
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0, deceleration * Time.fixedDeltaTime);
        }

        Vector3 newVelocity = transform.forward * (currentSpeed / 3.6f);
        rigid.velocity = new Vector3(newVelocity.x, rigid.velocity.y, newVelocity.z);
    }

    // �Ϲ� �̵� �� ���� ó�� (�ӵ��� ���� ���� ����)
    private void HandleSteering()
    {
        float steerAngle = inputHorizontal * GetSpeedProportionalValue() * steeringForce;
        Quaternion turnRotation = Quaternion.Euler(0, steerAngle * Time.fixedDeltaTime, 0);
        rigid.MoveRotation(rigid.rotation * turnRotation);
    }

    // �ӵ��� ���� ���� ���� ����: �ӵ��� �������� ���� ����
    private float GetSpeedProportionalValue()
    {
        return Mathf.Clamp(1 - (Mathf.Abs(currentSpeed) / maxSpeed), 0.3f, 1f);
    }

    // �帮��Ʈ ����
    private void StartDrift()
    {
        isDrifting = true;
        driftStartTime = Time.time;
        driftDirection = transform.forward; // �帮��Ʈ ���� �� ���� ���� ����

        // �帮��Ʈ ���۵Ǹ� ���� �ӵ��� ����Ͽ� �ӵ� ���� ����
        currentSpeed *= driftSpeedReduction;
        rigid.velocity = transform.forward * (currentSpeed / 3.6f);

        if (CHM != null)
            CHM.SetSkidMarkActive(isDrifting);

        // �帮��Ʈ �� AddForce�� �̿��� Ⱦ ���� �� ����
        StartCoroutine(DriftRoutine());
    }

    // �帮��Ʈ �� ����: �¿� �Է¿� ���� Ⱦ�� �߰� �� �ణ�� ���� ����
    private IEnumerator DriftRoutine()
    {
        while (isDrifting)
        {
            float lateralForce = inputHorizontal * driftForce;
            rigid.AddForce(transform.right * lateralForce, ForceMode.Acceleration);

            float driftSteer = inputHorizontal * driftSteerStrength;
            transform.Rotate(0, driftSteer, 0);

            yield return null;
        }
    }

    // �帮��Ʈ ����: �帮��Ʈ ȿ�� ���� �� �ӵ��� �����ϰ� ���� ���� ���� ����
    private void StopDrift()
    {
        isDrifting = false;

        if (CHM != null)
            CHM.SetSkidMarkActive(isDrifting);

        // �帮��Ʈ ���� �� �ε巯�� �ӵ� ���� ó��
        StartCoroutine(SmoothSpeedRecovery());
        // �帮��Ʈ ���� �� ������ ���� �̵� ����(����)�� �ٶ󺸵��� ȸ�� ����
        StartCoroutine(AlignForwardCoroutine());
    }

    // �帮��Ʈ ���� �� �ε巴�� �ӵ��� ����(��, 20% �߰� ����)
    private IEnumerator SmoothSpeedRecovery()
    {
        float recoveryTime = 0.5f;
        float targetSpeed = currentSpeed * 0.8f;
        float elapsedTime = 0;

        while (elapsedTime < recoveryTime)
        {
            currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, elapsedTime / recoveryTime);
            elapsedTime += Time.deltaTime;

            Vector3 newVelocity = transform.forward * (currentSpeed / 3.6f);
            rigid.velocity = new Vector3(newVelocity.x, rigid.velocity.y, newVelocity.z);

            yield return null;
        }

        currentSpeed = targetSpeed;
    }

    // �帮��Ʈ ���� �� ���� �̵� ����(velocity ����)�� ���� ������ ���� �����ϵ��� �ϴ� �ڷ�ƾ
    private IEnumerator AlignForwardCoroutine()
    {
        float alignmentDuration = 1.0f;
        float elapsed = 0f;
        Quaternion initialRotation = transform.rotation;

        // ������ �̵� ���̸� �� ������ ��ǥ ȸ��������, ���� ���¸� ���� forward ����
        Vector3 velocity = rigid.velocity;
        if (velocity.sqrMagnitude < 0.1f)
        {
            velocity = transform.forward;
        }
        velocity.y = 0;
        velocity.Normalize();
        Quaternion targetRotation = Quaternion.LookRotation(velocity, Vector3.up);

        while (elapsed < alignmentDuration)
        {
            transform.rotation = Quaternion.Slerp(initialRotation, targetRotation, elapsed / alignmentDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.rotation = targetRotation;
    }
}
