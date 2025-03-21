using System.Collections;
using UnityEngine;

public class TestCHMKart : MonoBehaviour
{
    #region Serialized Fields

    [Header("Kart Components")]
    [SerializeField] private GameObject wheels;
    [SerializeField] private GameObject carBody;

    [Header("Movement Settings")]
    [SerializeField] private float maxSpeed = 200f;
    [SerializeField] private float movementForce = 20f;
    [SerializeField] private float steerAngle = 200f;

    [Header("Drift Settings")]
    [SerializeField] private float minDriftAngle = 30f;
    [SerializeField] public float maxDriftAngle = 180f;
    [SerializeField] private float minDriftDuration = 0.2f;
    [SerializeField] private float maxDriftDuration = 2f;
    [SerializeField] private float minDriftForceMultiplier = 1f;
    [SerializeField] private float maxDriftForceMultiplier = 5f;
    [SerializeField] private float driftSpeedReduction = 0.7f;

    [Header("Boost Settings")]
    [SerializeField] private float boostSpeed = 280f;
    [SerializeField] private float boostDuration = 1.2f;
    [SerializeField] private int maxBoostGauge = 100;
    [SerializeField] private float boostChargeRate = 1f;
    [SerializeField] private float driftBoostChargeRate = 5f;
    [SerializeField] private float boostMaxSpeed = 280f;

    // 수정 속도와 아래로 누르는 힘을 조정할 수 있도록 변수화합니다.
    [SerializeField] private float rotationCorrectionSpeed = 5f;
    [SerializeField] private float downwardForce = 10f;

    #endregion

    #region Private Fields

    private CHMTestWheelController wheelCtrl;
    private Rigidbody rigid;

    // Drift and Boost related state
    private Coroutine postDriftBoostCoroutine;
    private float initialDriftSpeed;
    private bool isBoosting = false;
    public bool isDrifting = false;
    public float currentDriftAngle = 0f;
    private float driftDuration;
    private float currentDriftThreshold;   // 속도에 따른 드리프트 입력 기준 값
    private float driftForceMultiplier;     // 동적으로 계산된 드리프트 힘 배수
    private int boostGauge = 0;
    private float lockedYRotation = 0f;     // 드리프트 중 고정된 Y 회전 값

    #endregion

    #region Unity Methods

    private void Awake()
    {
        wheelCtrl = wheels.GetComponent<CHMTestWheelController>();
        rigid = GetComponent<Rigidbody>();
    }
    private void FixedUpdate()
    {
        // 기존 이동 처리 후에 공중 회전 보정 처리
        CorrectAirborneRotation();
    }
    private void Update()
    {
        float steerInput = Input.GetAxis("Horizontal");
        float motorInput = Input.GetAxis("Vertical");

        // 속도에 따른 드리프트 관련 파라미터 갱신
        AdjustDriftParameters();

        // 드리프트 시작: LeftShift 키와 조향 입력이 있을 때
        if (Input.GetKeyDown(KeyCode.LeftShift) && Mathf.Abs(steerInput) > 0)
        {
            StartDrift(steerInput * currentDriftThreshold);
        }

        // 드리프트 중 추가 입력 처리
        if (isDrifting && Input.GetKey(KeyCode.LeftShift))
        {
            currentDriftAngle += Time.deltaTime * 10f;
            currentDriftAngle = Mathf.Clamp(currentDriftAngle, -maxDriftAngle, maxDriftAngle);
        }

        // 기본 부스트 (LeftControl) 처리
        if (Input.GetKeyDown(KeyCode.LeftControl) && boostGauge >= maxBoostGauge)
        {
            StartBoost(boostDuration);
        }

        ChargeBoostGauge();
        HandleKartMovement(motorInput, steerInput);
    }

    #endregion

    #region Drift and Boost Methods

    private void AdjustDriftParameters()
    {
        // 현재 속도를 기준으로 0~1 비율 (민감도 2배)
        float currentSpeed = rigid.velocity.magnitude;
        float speedFactor = Mathf.Clamp01(currentSpeed / maxSpeed * 2f);
        // 최소 ~ 최대 드리프트 각도 사이 선형 보간 (드리프트 시작 시 기준 입력)
        currentDriftThreshold = Mathf.Lerp(minDriftAngle, maxDriftAngle, speedFactor);
        // 드리프트 힘 배수도 속도에 따라 조정
        driftForceMultiplier = Mathf.Lerp(minDriftForceMultiplier, maxDriftForceMultiplier, speedFactor);
    }

    private void StartDrift(float driftInputAngle)
    {
        isDrifting = true;
        currentDriftAngle = driftInputAngle;
        lockedYRotation = transform.eulerAngles.y;  // 드리프트 시작 시 회전값 고정

        // 속도와 조향 입력 기반으로 드리프트 지속 시간 계산
        float currentSpeed = rigid.velocity.magnitude;
        float speedFactor = Mathf.Clamp01(currentSpeed / maxSpeed);
        float steerFactor = Mathf.Abs(driftInputAngle / currentDriftThreshold);
        float influenceFactor = (speedFactor + steerFactor) / 2f;
        driftDuration = Mathf.Lerp(minDriftDuration, maxDriftDuration, influenceFactor);

        Debug.Log($"Drift Started: InputAngle={driftInputAngle}, Speed={currentSpeed}, SteerFactor={steerFactor}, Duration={driftDuration:F2}s");

        // 드리프트 종료 예약
        Invoke(nameof(EndDrift), driftDuration);
    }

    private void EndDrift()
    {
        isDrifting = false;
        currentDriftAngle = 0f;
        Debug.Log("Drift Ended.");

        // 드리프트 종료 후 0.5초 동안 순간 부스트 입력 대기 (코루틴)
        if (postDriftBoostCoroutine != null)
            StopCoroutine(postDriftBoostCoroutine);
        postDriftBoostCoroutine = StartCoroutine(PostDriftBoostCoroutine());

        initialDriftSpeed = 0f;
    }

    private IEnumerator PostDriftBoostCoroutine()
    {
        float timer = 0f;
        bool boosted = false;
        Debug.Log("Waiting for instant boost input...");

        while (timer < 0.5f)
        {
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
            Debug.Log("Instant boost input window expired.");
        postDriftBoostCoroutine = null;
    }

    private void PerformInstantBoost()
    {
        Debug.Log("Instant Boost Activated!");
        rigid.velocity *= 1.2f;  // 현재 속력의 1.2배 증가
        postDriftBoostCoroutine = null;
    }

    private void StartBoost(float duration)
    {
        isBoosting = true;
        boostGauge = 0;
        rigid.velocity = transform.forward * boostSpeed;
        Debug.Log("Boost Activated.");
        Invoke(nameof(EndBoost), duration);
    }

    private void EndBoost()
    {
        isBoosting = false;
        Debug.Log("Boost Ended.");
    }

    private void ChargeBoostGauge()
    {
        if (isDrifting)
            boostGauge += Mathf.RoundToInt(driftBoostChargeRate * Time.deltaTime);
        else
            boostGauge += Mathf.RoundToInt(boostChargeRate * Time.deltaTime);

        boostGauge = Mathf.Clamp(boostGauge, 0, maxBoostGauge);

        if (boostGauge >= maxBoostGauge)
            Debug.Log("Boost Gauge Full!");
    }

    #endregion

    #region Movement Handling

    private void HandleKartMovement(float motorInput, float steerInput)
    {
        float currentMaxSpeed = isBoosting ? boostMaxSpeed : maxSpeed;

        if (isDrifting)
        {
            // 드리프트 시작 시 초기 속도 기록 (최초 한 번)
            if (initialDriftSpeed == 0f)
            {
                initialDriftSpeed = rigid.velocity.magnitude;
            }

            // 드리프트 동안 속도 감소 적용
            float driftSpeed = initialDriftSpeed * driftSpeedReduction;

            // steering 입력을 누적하여 드리프트 방향을 업데이트 (조향 효과 적용)
            lockedYRotation += steerInput * (steerAngle / 2.5f) * Time.deltaTime;
            // 새로운 진행 방향을 계산 (Y축 회전)
            Quaternion driftRotation = Quaternion.Euler(0f, lockedYRotation, 0f);
            Vector3 driftDirection = driftRotation * Vector3.forward;

            // velocity를 부드럽게 새로운 driftDirection으로 전환 (Lerp로 자연스럽게)
            rigid.velocity = Vector3.Lerp(rigid.velocity, driftDirection * driftSpeed, Time.deltaTime * 5f);

            // 필요하면 추가 측면 힘도 적용 (드리프트 느낌 강화)
            Vector3 lateralForce = transform.right * steerInput * driftForceMultiplier * movementForce;
            rigid.AddForce(lateralForce, ForceMode.Force);
        }
        else
        {
            // 평상시(드리프트 아닐 때): 이동은 velocity 할당으로 처리
            initialDriftSpeed = 0f;
            Vector3 targetVelocity = transform.forward * motorInput * movementForce;
            rigid.velocity = Vector3.Lerp(rigid.velocity, targetVelocity, Time.deltaTime * 5f);

            // 드리프트가 아닌 경우엔 회전은 일반적으로 처리
            lockedYRotation = transform.eulerAngles.y; // 보정
        }

        // 최대 속도 제한
        if (rigid.velocity.magnitude > currentMaxSpeed)
        {
            rigid.velocity = rigid.velocity.normalized * currentMaxSpeed;
        }

        // 부드러운 방향 전환 적용 (비록 이미 드리프트 중일 때는 개별적으로 회전 처리됨)
        Vector3 turnDirection = Quaternion.Euler(0, steerInput * steerAngle * Time.deltaTime, 0) * transform.forward;
        rigid.MoveRotation(Quaternion.LookRotation(turnDirection));


        // 필요 시 바퀴 회전 업데이트 (WheelController)
        if (wheelCtrl != null)
            wheelCtrl.UpdateAndRotateWheels(steerInput, motorInput, rigid.velocity.magnitude, isDrifting);
    }

    #endregion    
    private void CorrectAirborneRotation()
    {
        // 지면에 닿아있지 않으면 보정
        if (!IsGrounded())
        {
            // 현재 회전값을 가져와서, x와 z는 천천히 0으로 보간합니다. (y는 그대로 유지)
            Vector3 currentEuler = transform.eulerAngles;
            float correctedX = Mathf.LerpAngle(currentEuler.x, 0f, Time.deltaTime * rotationCorrectionSpeed);
            float correctedZ = Mathf.LerpAngle(currentEuler.z, 0f, Time.deltaTime * rotationCorrectionSpeed);

            transform.rotation = Quaternion.Euler(correctedX, currentEuler.y, correctedZ);

            // 필요 시 아래쪽으로 힘을 추가하여 떨어지도록 할 수 있음.
            rigid.AddForce(Vector3.down * downwardForce, ForceMode.Acceleration);
        }
    }

    private bool IsGrounded()
    {
        // 간단한 Raycast를 사용하여 kart의 하단이 지면에 닿았는지 확인합니다.
        float rayDistance = 0.8f; // 필요에 따라 조정 가능
        return Physics.Raycast(transform.position, Vector3.down, rayDistance);
    }
}