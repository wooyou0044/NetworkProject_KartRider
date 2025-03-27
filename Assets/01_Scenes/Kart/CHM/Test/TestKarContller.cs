using System.Collections;
using UnityEngine;

public class TestKartController : MonoBehaviour
{
    [Header("Kart Components")]
    [SerializeField] public GameObject wheels; // �� ������Ʈ
    [SerializeField] public GameObject carBody; // īƮ �ٵ�

    [Header("Movement Settings")]
    [SerializeField] public float maxSpeed = 200f; // �ִ� �ӵ� (m/s)
    [SerializeField] public float movementForce = 200; // �⺻ �̵� ��
    [SerializeField] public float steerAngle = 800; // ���� ����

    [Header("Drift Settings")]
    [SerializeField] public float minDriftAngle = 30f; // �ִ� �帮��Ʈ ����
    [SerializeField] public float maxDriftAngle = 180f; // �ִ� �帮��Ʈ ����
    [SerializeField] public float minDriftDuration = 0.2f; // �ּ� �帮��Ʈ �ð�
    [SerializeField] public float maxDriftDuration = 2f; // �ִ� �帮��Ʈ �ð�
    [SerializeField] public float mindriftForceMultiplier = 1f; // �帮��Ʈ �� �ּ� �� ���
    [SerializeField] public float maxdriftForceMultiplier = 5f; // �帮��Ʈ �� �ִ� �� ���
    [SerializeField] public float driftForceMultiplier = 0f; // �帮��Ʈ �� �� ���
    [SerializeField] public float driftSpeedReduction = 0.7f; // �帮��Ʈ �� �ӵ� ���� ����

    [Header("Boost Settings")]
    [SerializeField] public float boostSpeed = 280f; // �ν�Ʈ �ӵ�
    [SerializeField] public float boostDuration = 1.2f; // �ν�Ʈ ���� �ð�
    [SerializeField] public int maxBoostGauge = 100; // �ִ� �ν�Ʈ ������
    [SerializeField] public float boostChargeRate = 1f; // �⺻ �ν�Ʈ ���� �ӵ�
    [SerializeField] public float driftBoostChargeRate = 5f; // �帮��Ʈ �� �ν�Ʈ ���� �ӵ�
    [SerializeField] public float boostMaxSpeed = 280f; // �ν��� �� �ִ� �ӵ�

    private WheelController wheelCtrl;
    private Rigidbody rigid;


    private Coroutine postDriftBoostCoroutine;// �ν��� �Ǵ�
    private float initialDriftSpeed; // �帮��Ʈ ���� �� �ʱ� �ӵ� ����
    public bool isDrifting = false;
    private bool isBoosting = false;
    public float currentDriftAngle = 0f;
    private float driftDuration;
    private float driftAngle;
    private int boostGauge = 0;
    private float lockedYRotation = 0f; // �帮��Ʈ �� ������ Y ȸ�� ��

    private void Awake()
    {
        wheelCtrl = wheels.GetComponent<WheelController>();
        rigid = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        float steerInput = Input.GetAxis("Horizontal");
        float motorInput = Input.GetAxis("Vertical");
        AdjustDriftParameters();
        // �帮��Ʈ ����
        if (Input.GetKeyDown(KeyCode.LeftShift) && Mathf.Abs(steerInput) > 0)
        {
            StartDrift(steerInput * driftAngle);
        }

        // �帮��Ʈ �� �߰� ���� ó��
        if (isDrifting && Input.GetKey(KeyCode.LeftShift))
        {
            currentDriftAngle += Time.deltaTime * 10f; // �帮��Ʈ ���� ����
            currentDriftAngle = Mathf.Clamp(currentDriftAngle, -maxDriftAngle, maxDriftAngle);
        }

        // �ν�Ʈ Ȱ��ȭ
        if (Input.GetKeyDown(KeyCode.LeftControl) && boostGauge >= maxBoostGauge)
        {
            StartBoost(boostDuration);
        }

        // �ν�Ʈ ������ ����
        ChargeBoostGauge();

        // �̵� ó��
        HandleKartMovement(motorInput, steerInput);
    }
    private void AdjustDriftParameters()
    {
        // ���� �ӵ��� �������� ���� ���
        float currentSpeed = rigid.velocity.magnitude;
        float speedFactor = currentSpeed / maxSpeed * 2f;

        // maxDriftAngle ���� (30 ~ 180������)
        driftAngle = Mathf.Lerp(minDriftAngle, maxDriftAngle, speedFactor);

        // driftForceMultiplier ���� (1~ 5����)
        driftForceMultiplier = Mathf.Lerp(mindriftForceMultiplier, maxdriftForceMultiplier, speedFactor);
    }

    private void HandleKartMovement(float motorInput, float steerInput)
    {
        // �ν��� ���¿� ���� �ִ� �ӵ� ����    
        float currentMaxSpeed = isBoosting ? boostMaxSpeed : maxSpeed;

        // �ִ� �ӵ� ����
        if (rigid.velocity.magnitude > currentMaxSpeed)
        {
            rigid.velocity = rigid.velocity.normalized * currentMaxSpeed;
        }

        // �⺻ �̵��� ���
        Vector3 forwardForce = transform.forward * motorInput * movementForce;

        if (isDrifting)
        {

            // �帮��Ʈ �ʱ� �ӵ� ����
            if (initialDriftSpeed == 0f)
            {
                initialDriftSpeed = rigid.velocity.magnitude; // �ʱ� �ӵ� ����
            }

            // �帮��Ʈ �ӵ� ����
            float driftSpeed = initialDriftSpeed * driftSpeedReduction;
            rigid.velocity = rigid.velocity.normalized * driftSpeed;
            // �帮��Ʈ ���� steerInput �� ������� �ﰢ ȸ��
            lockedYRotation = transform.eulerAngles.y + steerInput * steerAngle / 2.5f * Time.deltaTime;
            transform.rotation = Quaternion.Euler(0, lockedYRotation, 0);
            // ���� �� �߰�
            Vector3 lateralForce = transform.right * steerInput * driftForceMultiplier * movementForce;
            rigid.AddForce(lateralForce, ForceMode.Force);
        }
        else
        {
            // �帮��Ʈ�� �ƴ� �� �ʱ�ȭ
            initialDriftSpeed = 0f;
        }

        // �⺻ ���� �� �߰�
        rigid.AddForce(forwardForce, ForceMode.Force);

        // ȸ�� ó��
        Vector3 turnDirection = Quaternion.Euler(0, steerInput * steerAngle * Time.deltaTime, 0) * transform.forward;
        rigid.MoveRotation(Quaternion.LookRotation(turnDirection));

        // ���� ��Ʈ�ѷ� ó��
        if (wheelCtrl != null)
        {
            //wheelCtrl.UpdateAndRotateWheels(steerInput, motorInput, rigid.velocity.magnitude, isDrifting);
        }
    }

    private void StartDrift(float driftAngle)
    {
        isDrifting = true;
        currentDriftAngle = driftAngle;

        // Y�� ȸ�� ���� ����
        lockedYRotation = transform.eulerAngles.y;

        // �ӵ��� �Է°� ������� �帮��Ʈ ���� �ð� ���
        float currentSpeed = rigid.velocity.magnitude; // ���� �ӵ�
        float speedFactor = currentSpeed / maxSpeed;   // �ӵ� ���� (0 ~ 1)
        float steerInputAbs = Mathf.Abs(driftAngle / this.driftAngle); // ���� �Է� ���� (0 ~ 1)
        float influenceFactor = (speedFactor + steerInputAbs) / 2f; // �ӵ��� �Է°��� ���
        driftDuration = Mathf.Lerp(minDriftDuration, maxDriftDuration, influenceFactor); // �帮��Ʈ �ð�

        Debug.Log($"�帮��Ʈ ����: ���� = {driftAngle}, �ӵ� = {currentSpeed}, �Է� = {steerInputAbs}, �ð� = {driftDuration}��");

        // �帮��Ʈ ���� ����
        Invoke(nameof(EndDrift), driftDuration);
    }

    // �帮��Ʈ ���� �� �ڷ�ƾ���� �ν��� ����
    private void EndDrift()
    {
        isDrifting = false;
        currentDriftAngle = 0f;
        Debug.Log("�帮��Ʈ ����.");

        // ���� �ڷ�ƾ�� ���� ���̸� ����
        if (postDriftBoostCoroutine != null)
        {
            StopCoroutine(postDriftBoostCoroutine);
        }

        // �ν��� �Է� ��� �ڷ�ƾ ����
        postDriftBoostCoroutine = StartCoroutine(PostDriftBoostCoroutine());
    }

    // ���� �ν��� �Է� ��� �ڷ�ƾ
    private IEnumerator PostDriftBoostCoroutine()
    {
        float timer = 0f;
        bool boosted = false;

        Debug.Log("���� �ν��� ��� ����...");

        while (timer < 0.5f)
        {
            // �ν��� �Է� ����
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                PerformInstantBoost();
                boosted = true;
                break;
            }

            timer += Time.deltaTime;
            yield return null;
        }

        if (!boosted)
        {
            Debug.Log("���� �ν��� �Է� �ð� �ʰ�.");
        }

        // �ڷ�ƾ ����
        postDriftBoostCoroutine = null;
    }
    // ���� �ν��� ��� �߰�
    private void PerformInstantBoost()
    {
        Debug.Log("���� �ν��� Ȱ��ȭ!");

        // ���� �ӷ��� 1.2��� �ν���
        rigid.velocity *= 1.2f;

        // ���� �ν��� ���� ���� ����
        postDriftBoostCoroutine = null;
    }

    private void StartBoost(float duration)
    {
        isBoosting = true;
        boostGauge = 0;

        // �ν�Ʈ �ӵ� ����
        rigid.velocity = transform.forward * boostSpeed;

        // �ν�Ʈ ���� ����
        Invoke(nameof(EndBoost), duration);
        Debug.Log("�ν�Ʈ Ȱ��ȭ.");
    }

    private void EndBoost()
    {
        isBoosting = false;
        Debug.Log("�ν�Ʈ ����.");
    }

    private void ChargeBoostGauge()
    {
        if (isDrifting)
        {
            boostGauge += Mathf.RoundToInt(driftBoostChargeRate * Time.deltaTime);
        }
        else
        {
            boostGauge += Mathf.RoundToInt(boostChargeRate * Time.deltaTime);
        }

        boostGauge = Mathf.Clamp(boostGauge, 0, maxBoostGauge);

        if (boostGauge >= maxBoostGauge)
        {
            Debug.Log("�ν�Ʈ �غ� �Ϸ�!");
        }
    }
}
