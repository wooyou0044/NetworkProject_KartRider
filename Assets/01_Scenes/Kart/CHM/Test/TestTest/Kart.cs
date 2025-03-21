using UnityEngine;

public class Kart : MonoBehaviour
{
    [Header("Kart Components")]
    [SerializeField] private Rigidbody rigid;
    [SerializeField] private WheelManager wheelManager;
    [SerializeField] private VelocityManager velocityManager;
    [SerializeField] private DriftManager driftManager;
    [SerializeField] private BoostManager boostManager;

    [Header("Movement Settings")]
    [SerializeField] private float movementForce = 200f;
    [SerializeField] private float steerAngle = 800f;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        wheelManager = GetComponent<WheelManager>();
        velocityManager = GetComponent<VelocityManager>();
        driftManager = GetComponent<DriftManager>();
        boostManager = GetComponent<BoostManager>();
    }

    private void Update()
    {
        float motorInput = Input.GetAxis("Vertical"); // 전진/후진 입력
        float steerInput = Input.GetAxis("Horizontal"); // 조향 입력

        // 드리프트 시작
        if (Input.GetKeyDown(KeyCode.LeftShift) && Mathf.Abs(steerInput) > 0)
        {
            driftManager.StartDrift(steerInput);
        }

        // 부스트 활성화
        if (Input.GetKeyDown(KeyCode.LeftControl) && boostManager.CanBoost())
        {
            boostManager.StartBoost();
        }

        // 부스트 게이지 충전
        boostManager.ChargeBoostGauge(driftManager.IsDrifting());

        // 이동 처리
        HandleKartMovement(motorInput, steerInput);
    }

    private void HandleKartMovement(float motorInput, float steerInput)
    {
        // 후진 조건 처리
        bool isReversing = motorInput < 0;

        // 속도 업데이트 (VelocityManager를 통해 처리)
        velocityManager.UpdateSpeed(motorInput, Time.deltaTime, driftManager.IsDrifting());

        // 현재 속도 가져오기
        float currentSpeed = velocityManager.GetCurrentSpeed();

        // 이동 처리
        if (currentSpeed != 0) // 속도가 0이 아닐 때만 이동 처리
        {
            Vector3 movementDirection = transform.forward * currentSpeed * Time.deltaTime;
            rigid.AddForce(movementDirection, ForceMode.Force);
        }

        // 조향 처리 (motorInput과 무관하게 실행)
        float turnAmount = steerInput * steerAngle * Time.deltaTime * (isReversing ? -1 : 1);
        transform.Rotate(Vector3.up, turnAmount);

        // 휠 매니저 업데이트
        wheelManager.UpdateAndRotateWheels(steerInput, motorInput, currentSpeed, driftManager.IsDrifting());
    }
}