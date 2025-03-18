using UnityEngine;

public class TestKartController : MonoBehaviour
{
    [Header("Kart Components")]
    [SerializeField] public GameObject wheels; // 휠 컨트롤러 오브젝트
    [SerializeField] public GameObject carBody; // 차량 본체

    [Header("Movement Settings")]
    [SerializeField] public float maxSpeed = 200f; // 최대 속도
    [SerializeField] public float movementForce = 1000f; // 이동 힘
    [SerializeField] public float steerAngle = 200f; // 조향 각도

    [Header("Drift Settings")]
    [SerializeField] public float maxDriftAngle = 45f; // 최대 드리프트 각도
    [SerializeField] public float minDriftDuration = 1.0f; // 최소 드리프트 지속 시간
    [SerializeField] public float maxDriftDuration = 2.0f; // 최대 드리프트 지속 시간
    [SerializeField] public float driftForceMultiplier = 0.4f; // 드리프트 힘 배율
    [SerializeField] public float driftSpeedReduction = 0.8f; // 드리프트 시 속도 감소 비율

    [Header("Boost Settings")]
    [SerializeField] public float boostSpeed = 280f; // 부스터 속도
    [SerializeField] public float boostDuration = 1.2f; // 부스터 지속 시간
    [SerializeField] public int maxBoostGauge = 100; // 최대 부스터 게이지
    [SerializeField] public float boostChargeRate = 1f; // 주행 중 부스터 충전 속도
    [SerializeField] public float driftBoostChargeRate = 5f; // 드리프트 시 부스터 충전 속도

    private TestWheelController wheelCtrl;
    private Rigidbody rigid;

    private bool isDrifting = false;
    private bool isBoosting = false;
    private float currentDriftAngle = 0f;
    private float driftDuration;
    private int boostGauge = 0;

    private void Awake()
    {
        wheelCtrl = wheels.GetComponent<TestWheelController>();
        rigid = GetComponent<Rigidbody>();
    }

    void Update()
    {
        float steerInput = Input.GetAxis("Horizontal");
        float motorInput = Input.GetAxis("Vertical");

        // 드리프트 시작
        if (Input.GetKeyDown(KeyCode.LeftShift) && Mathf.Abs(steerInput) > 0)
        {
            StartDrift(steerInput * maxDriftAngle);
        }

        // 드리프트 중 각도 증가
        if (isDrifting && Input.GetKey(KeyCode.LeftShift))
        {
            currentDriftAngle += Time.deltaTime * 10f; // 각도 증가
            currentDriftAngle = Mathf.Clamp(currentDriftAngle, -maxDriftAngle, maxDriftAngle);
        }

        // 부스터 시작
        if (Input.GetKeyDown(KeyCode.LeftControl) && boostGauge >= maxBoostGauge)
        {
            StartBoost(boostDuration);
        }

        // 부스터 게이지 충전
        ChargeBoostGauge();

        // 이동 처리
        HandleKartMovement(motorInput, steerInput);
    }

    private void HandleKartMovement(float motorInput, float steerInput)
    {
        // 기본 이동 처리
        Vector3 forwardForce = transform.forward * motorInput * movementForce;

       // if (isDrifting)
       // {
       //     // 드리프트 시 속도 감소
       //     forwardForce *= driftSpeedReduction;
       //
       //     // 드리프트 방향으로 힘 추가
       //     Vector3 driftForce = transform.right * currentDriftAngle * motorInput * movementForce * driftForceMultiplier;
       //     rigid.AddForce(driftForce, ForceMode.Force);
       // }

        // 기본 힘 추가
        rigid.AddForce(forwardForce, ForceMode.Force);

        // 조향 처리
        Vector3 turnDirection = Quaternion.Euler(0, steerInput * steerAngle * Time.deltaTime, 0) * transform.forward;
        rigid.MoveRotation(Quaternion.LookRotation(turnDirection));

        // 휠 업데이트
        if (wheelCtrl != null)
        {
            wheelCtrl.UpdateAndRotateWheels(steerInput, motorInput, rigid.velocity.magnitude, isDrifting);
        }
    }

    private void StartDrift(float driftAngle)
    {
        isDrifting = true;
        currentDriftAngle = driftAngle;

        // 드리프트 지속 시간 계산
        float steerInputAbs = Mathf.Abs(driftAngle / maxDriftAngle);
        if (steerInputAbs <= 0.3f)
        {
            driftDuration = minDriftDuration; // 짧은 드리프트
        }
        else if (steerInputAbs > 0.3f && steerInputAbs <= 0.7f)
        {
            driftDuration = (minDriftDuration + maxDriftDuration) / 2; // 중간 드리프트
        }
        else
        {
            driftDuration = maxDriftDuration; // 최대 드리프트
        }

        Debug.Log($"Drift started with angle: {driftAngle}, duration: {driftDuration}s");

        // 드리프트 유지 시간 후 종료
        Invoke(nameof(EndDrift), driftDuration);
    }

    private void EndDrift()
    {
        isDrifting = false;
        currentDriftAngle = 0f;
        Debug.Log("Drift ended.");
    }

    private void StartBoost(float duration)
    {
        isBoosting = true;
        boostGauge = 0;

        // 부스터 속도 증가
        float boostForce = rigid.velocity.magnitude * 1.2f;
        rigid.AddForce(transform.forward * boostForce, ForceMode.VelocityChange);

        // 부스터 종료
        Invoke(nameof(EndBoost), duration);
        Debug.Log("Boost started.");
    }

    private void EndBoost()
    {
        isBoosting = false;
        Debug.Log("Boost ended.");
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
            Debug.Log("Boost ready!");
        }
    }
}
