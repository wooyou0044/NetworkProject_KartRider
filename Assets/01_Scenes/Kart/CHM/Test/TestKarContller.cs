using System.Collections;
using UnityEngine;

public class TestKartController : MonoBehaviour
{
    [Header("Kart Components")]
    [SerializeField] public GameObject wheels; // 휠 컴포넌트
    [SerializeField] public GameObject carBody; // 카트 바디

    [Header("Movement Settings")]
    [SerializeField] public float maxSpeed = 200f; // 최대 속도 (m/s)
    [SerializeField] public float movementForce = 200; // 기본 이동 힘
    [SerializeField] public float steerAngle = 800; // 조향 각도

    [Header("Drift Settings")]
    [SerializeField] public float minDriftAngle = 30f; // 최대 드리프트 각도
    [SerializeField] public float maxDriftAngle = 180f; // 최대 드리프트 각도
    [SerializeField] public float minDriftDuration = 0.2f; // 최소 드리프트 시간
    [SerializeField] public float maxDriftDuration = 2f; // 최대 드리프트 시간
    [SerializeField] public float mindriftForceMultiplier = 1f; // 드리프트 시 최소 힘 배수
    [SerializeField] public float maxdriftForceMultiplier = 5f; // 드리프트 시 최대 힘 배수
    [SerializeField] public float driftForceMultiplier = 0f; // 드리프트 시 힘 배수
    [SerializeField] public float driftSpeedReduction = 0.7f; // 드리프트 중 속도 감소 배율

    [Header("Boost Settings")]
    [SerializeField] public float boostSpeed = 280f; // 부스트 속도
    [SerializeField] public float boostDuration = 1.2f; // 부스트 지속 시간
    [SerializeField] public int maxBoostGauge = 100; // 최대 부스트 게이지
    [SerializeField] public float boostChargeRate = 1f; // 기본 부스트 충전 속도
    [SerializeField] public float driftBoostChargeRate = 5f; // 드리프트 시 부스트 충전 속도
    [SerializeField] public float boostMaxSpeed = 280f; // 부스터 시 최대 속도

    private WheelController wheelCtrl;
    private Rigidbody rigid;


    private Coroutine postDriftBoostCoroutine;// 부스팅 판단
    private float initialDriftSpeed; // 드리프트 시작 시 초기 속도 저장
    public bool isDrifting = false;
    private bool isBoosting = false;
    public float currentDriftAngle = 0f;
    private float driftDuration;
    private float driftAngle;
    private int boostGauge = 0;
    private float lockedYRotation = 0f; // 드리프트 중 고정된 Y 회전 값

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
        // 드리프트 시작
        if (Input.GetKeyDown(KeyCode.LeftShift) && Mathf.Abs(steerInput) > 0)
        {
            StartDrift(steerInput * driftAngle);
        }      

        // 드리프트 중 추가 조작 처리
        if (isDrifting && Input.GetKey(KeyCode.LeftShift))
        {
            currentDriftAngle += Time.deltaTime * 10f; // 드리프트 각도 조정
            currentDriftAngle = Mathf.Clamp(currentDriftAngle, -maxDriftAngle, maxDriftAngle);
        }

        // 부스트 활성화
        if (Input.GetKeyDown(KeyCode.LeftControl) && boostGauge >= maxBoostGauge)
        {
            StartBoost(boostDuration);
        }

        // 부스트 게이지 충전
        ChargeBoostGauge();

        // 이동 처리
        HandleKartMovement(motorInput, steerInput);
    }
    private void AdjustDriftParameters()
    {
        // 현재 속도를 기준으로 비율 계산
        float currentSpeed = rigid.velocity.magnitude;
        float speedFactor = currentSpeed / maxSpeed *2f;

        // maxDriftAngle 조정 (30 ~ 180도까지)
        driftAngle = Mathf.Lerp(minDriftAngle, maxDriftAngle, speedFactor);

        // driftForceMultiplier 조정 (1~ 5까지)
        driftForceMultiplier = Mathf.Lerp(mindriftForceMultiplier, maxdriftForceMultiplier, speedFactor);
    }

    private void HandleKartMovement(float motorInput, float steerInput)
    {
        // 부스터 상태에 따른 최대 속도 설정    
        float currentMaxSpeed = isBoosting ? boostMaxSpeed : maxSpeed;

        // 최대 속도 제한
        if (rigid.velocity.magnitude > currentMaxSpeed)
        {
            rigid.velocity = rigid.velocity.normalized * currentMaxSpeed;
        }

        // 기본 이동력 계산
        Vector3 forwardForce = transform.forward * motorInput * movementForce;

        if (isDrifting)
        {          

            // 드리프트 초기 속도 저장
            if (initialDriftSpeed == 0f)
            {
                initialDriftSpeed = rigid.velocity.magnitude; // 초기 속도 저장
            }

            // 드리프트 속도 감소
            float driftSpeed = initialDriftSpeed * driftSpeedReduction;
            rigid.velocity = rigid.velocity.normalized * driftSpeed;
            // 드리프트 동안 steerInput 값 기반으로 즉각 회전
            lockedYRotation = transform.eulerAngles.y + steerInput * steerAngle/2.5f * Time.deltaTime;
            transform.rotation = Quaternion.Euler(0, lockedYRotation, 0);
            // 측면 힘 추가
            Vector3 lateralForce = transform.right * steerInput * driftForceMultiplier * movementForce;
            rigid.AddForce(lateralForce, ForceMode.Force);
        }
        else
        {
            // 드리프트가 아닐 때 초기화
            initialDriftSpeed = 0f;
        }

        // 기본 전진 힘 추가
        rigid.AddForce(forwardForce, ForceMode.Force);

        // 회전 처리
        Vector3 turnDirection = Quaternion.Euler(0, steerInput * steerAngle * Time.deltaTime, 0) * transform.forward;
        rigid.MoveRotation(Quaternion.LookRotation(turnDirection));

        // 바퀴 컨트롤러 처리
        if (wheelCtrl != null)
        {
            //wheelCtrl.UpdateAndRotateWheels(steerInput, motorInput, rigid.velocity.magnitude, isDrifting);
        }
    }

    private void StartDrift(float driftAngle)
    {
        isDrifting = true;
        currentDriftAngle = driftAngle;

        // Y축 회전 값을 고정
        lockedYRotation = transform.eulerAngles.y;

        // 속도와 입력값 기반으로 드리프트 지속 시간 계산
        float currentSpeed = rigid.velocity.magnitude; // 현재 속도
        float speedFactor = currentSpeed / maxSpeed;   // 속도 비율 (0 ~ 1)
        float steerInputAbs = Mathf.Abs(driftAngle / this.driftAngle); // 조향 입력 절댓값 (0 ~ 1)
        float influenceFactor = (speedFactor + steerInputAbs) / 2f; // 속도와 입력값의 평균
        driftDuration = Mathf.Lerp(minDriftDuration, maxDriftDuration, influenceFactor); // 드리프트 시간

        Debug.Log($"드리프트 시작: 각도 = {driftAngle}, 속도 = {currentSpeed}, 입력 = {steerInputAbs}, 시간 = {driftDuration}초");

        // 드리프트 종료 예약
        Invoke(nameof(EndDrift), driftDuration);
    }



    // 드리프트 종료 시 코루틴으로 부스터 관리
    private void EndDrift()
    {
        isDrifting = false;
        currentDriftAngle = 0f;
        Debug.Log("드리프트 종료.");

        // 기존 코루틴이 실행 중이면 정지
        if (postDriftBoostCoroutine != null)
        {
            StopCoroutine(postDriftBoostCoroutine);
        }

        // 부스터 입력 대기 코루틴 실행
        postDriftBoostCoroutine = StartCoroutine(PostDriftBoostCoroutine());
    }

    // 순간 부스터 입력 대기 코루틴
    private IEnumerator PostDriftBoostCoroutine()
    {
        float timer = 0f;
        bool boosted = false;

        Debug.Log("순간 부스터 대기 시작...");

        while (timer < 0.5f)
        {
            // 부스터 입력 감지
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
            Debug.Log("순간 부스터 입력 시간 초과.");
        }

        // 코루틴 종료
        postDriftBoostCoroutine = null;
    }
    // 순간 부스터 기능 추가
    private void PerformInstantBoost()
    {
        Debug.Log("순간 부스터 활성화!");

        // 현재 속력의 1.2배로 부스팅
        rigid.velocity *= 1.2f;

        // 순간 부스터 가능 상태 종료
        postDriftBoostCoroutine = null;
    }

    private void StartBoost(float duration)
    {
        isBoosting = true;
        boostGauge = 0;

        // 부스트 속도 적용
        rigid.velocity = transform.forward * boostSpeed;

        // 부스트 종료 예약
        Invoke(nameof(EndBoost), duration);
        Debug.Log("부스트 활성화.");
    }

    private void EndBoost()
    {
        isBoosting = false;
        Debug.Log("부스트 종료.");
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
            Debug.Log("부스트 준비 완료!");
        }
    }
}
