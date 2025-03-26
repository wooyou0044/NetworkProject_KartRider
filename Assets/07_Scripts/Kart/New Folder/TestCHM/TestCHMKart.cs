using System.Collections;
using Photon.Pun;
using Unity.VisualScripting;
using UnityEngine;

public class TestCHMKart : MonoBehaviour
{
    #region Serialized Fields

    [Header("카트 구성 요소")]
    [SerializeField] private GameObject wheels;   // 바퀴 오브젝트
    [SerializeField] GameObject kartBody; // 카트 바디 오브젝트

    [Header("이동 설정")]
    [SerializeField] private float maxSpeedKmh = 200f;           // 최대 속도
    [SerializeField] private float movementForce = 20f;       // 이동에 적용할 힘
    [SerializeField] private float steerAngle = 100f;         // 조향(회전) 각도
    [SerializeField] private float decelerationRate = 2f;  // 키 없음 감속 보정 계수
    [SerializeField] private float minSteerMultiplier = 1f;  // 최소 조향 배수 (속도가 낮을 때)
    [SerializeField] private float maxSteerMultiplier = 2f;  // 최대 조향 배수 (속도가 최대일 때)

    [Header("드리프트 설정")]
    [SerializeField] private float minDriftAngle = 30f;       // 최소 드리프트 각도
    [SerializeField] public float maxDriftAngle = 180f;       // 최대 드리프트 각도
    [SerializeField] private float minDriftDuration = 0.2f;     // 최소 드리프트 지속시간
    [SerializeField] private float maxDriftDuration = 2f;       // 최대 드리프트 지속시간
    [SerializeField] private float minDriftForceMultiplier = 1f;// 최소 드리프트 힘 배수
    [SerializeField] private float maxDriftForceMultiplier = 5f;// 최대 드리프트 힘 배수
    [SerializeField] private float driftSpeedReduction = 0.7f;  // 드리프트 시 속도 감소 비율

    [Header("부스트 설정")]
    [SerializeField] public float boostDuration = 2f;         // 기본 부스트 지속시간
    [SerializeField] public float momentboostDuration = 1.2f;         // 기본 부스트 지속시간
    [SerializeField] public int maxBoostGauge = 100;           // 최대 부스트 게이지
    [SerializeField] private float boostChargeRate = 5f;        // 기본 부스트 충전 속도
    [SerializeField] private float driftBoostChargeRate = 10f;   // 드리프트 중 부스트 충전 속도
    [SerializeField] private float boostMaxSpeedKmh = 280f;        // 부스트 상태의 최대 속도
    private float boostSpeed;         // 부스트 활성화 시 속도
    [Header("언덕 주행 보조 설정")]
    [SerializeField] private float uphillAngleThreshold = 10f;     // 언덕 판별 각도 (예: 10도 이상이면 언덕으로 간주)
    [SerializeField] private float uphillForceMultiplier = 1.5f;     // 언덕에서 추가 적용할 힘의 배수  


    #endregion

    #region Private Fields

    public float speedKM { get; private set; }     // 현재 속력 (km/h 단위)
    public bool isBoostTriggered { get; private set; } // 부스트 활성화 여부
    public bool isBoostCreate { get; set; }    // 드리프트 아이템 생성 가능 여부
    public float boostGauge { get; private set; }                // 현재 부스트 게이지
    public bool isBoostUsed { get; set; }
    public bool isRacingStart { get; set; }

    private float driftDuration = 4f;  // 부스터 지속 시간 (예: 총 4초, 2초 가속, 2초 감속)
    private CHMTestWheelController wheelCtrl;  // 바퀴 제어 스크립트
    private Rigidbody rigid;                   // 리지드바디 (물리 처리)
    private Coroutine postDriftBoostCoroutine; // 드리프트 종료 후 부스트 처리를 위한 코루틴 변수
    private float initialDriftSpeed;           // 드리프트 시작 시 기록된 초기 속도
    public bool isDrifting = false;            // 드리프트 진행 중 여부
    public float currentDriftAngle = 0f;       // 현재 누적 드리프트 각도
    private float currentDriftThreshold;       // 속도에 따른 드리프트 입력 기준 값
    private float driftForceMultiplier;        // 동적으로 계산된 드리프트 힘 배수
    private float lockedYRotation = 0f;        // 드리프트 시 고정되는 Y 회전값
    private float currentMotorInput;
    private float currentSteerInput;
    public int boostCount { get; private set; }
    // 내부적으로 사용할 m/s 단위 변수
    private float maxSpeed;      // 최대 속도 (m/s)
    private float boostMaxSpeed; // 부스트 최대 속도 (m/s)
    private Vector3 speed;       // 현재 속도 벡터
    private float chargeAmount;
    private float currentMaxSpeed;//부스트,기본 최대속도 판단해서 담김
    public AnimationCurve boostCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);  // 0초에 0, 1초에 1 값을 가진 부드러운 커브

    /* Network Instantiate */
    private Transform _playerParent;
    private Transform _tr;
    private PhotonView _photonView;

    KartBodyController kartBodyCtrl;

    #endregion

    #region Unity Methods

    private void Awake()
    {
        wheelCtrl = wheels.GetComponent<CHMTestWheelController>(); // 바퀴 컨트롤러 참조
        kartBodyCtrl = kartBody.GetComponent<KartBodyController>();
        rigid = GetComponent<Rigidbody>();                         // 리지드바디 참조

        /* TODO : 포톤 붙일때 수정해주기 */
        _tr = gameObject.transform;
        _photonView = GetComponent<PhotonView>();
        _playerParent = GameObject.Find("Players").transform;
        transform.parent = _playerParent;
    }
    private void FixedUpdate()
    {
        if (!_photonView.IsMine)
        {
            return;
        }

        maxSpeed = maxSpeedKmh / 3.6f;
        boostMaxSpeed = boostMaxSpeedKmh / 3.6f;
        currentMaxSpeed = isBoostTriggered ? boostMaxSpeed : maxSpeed;
        HandleKartMovement(currentMotorInput, currentSteerInput); // 입력은 Update()에서 저장 후 사용
                                                                  // 그리고 마지막에 항상 속도 클램핑을 적용합니다.
        ApplyEnhancedGravity();
    }


    private void Update()
    {
        if (!_photonView.IsMine)
        {
            return;
        }

        //if(isRacingStart == false)
        //{
        //    return;
        //}

        // 입력값 읽어오기
        currentSteerInput = Input.GetAxis("Horizontal");
        currentMotorInput = Input.GetAxis("Vertical");

        // 현재 속도 갱신 (Y축 제외) 및 km/h 변환
        speed = new Vector3(rigid.velocity.x, 0f, rigid.velocity.z);
        speedKM = speed.magnitude * 3.6f;
       

        // 드리프트 관련 파라미터 업데이트
        UpdateDriftParameters();

        // 드리프트 입력 처리
        HandleDriftInput(currentSteerInput);

        // 부스트 입력 처리
        HandleBoostInput();

        if (PerformBoxCast(groundLayer | wallLayer | jumpLayer | boosterLayer))

        {
            HandleLayerCollision(); // 충돌된 레이어 처리
        }

        // 레이캐스트로 지면 체크
        if (CheckIfGrounded())
        {
            Debug.Log("현재 지면 위에 있습니다.");
        }
        else
        {
            Debug.Log("현재 공중 상태입니다.");
        }
    }
    #endregion

    #region [키입력 함수 ]

    private void HandleDriftInput(float steerInput)
    {
        // LeftShift 키와 조향 입력이 있을 때 드리프트 시작
        if (Input.GetKeyDown(KeyCode.LeftShift) && Mathf.Abs(steerInput) > 0)
        {
            StartDrift(steerInput * currentDriftThreshold);
        }

        // 드리프트 중 추가 입력으로 드리프트 각도 업데이트
        if (isDrifting && Input.GetKeyDown(KeyCode.LeftShift))
        {
            UpdateDriftAngle();
        }
    }

    private void HandleBoostInput()
    {
        // LeftControl 키와 부스트 게이지 최대치 시 부스터 기본 발동
        if (Input.GetKeyDown(KeyCode.LeftControl) && boostCount > 0)
        {
            // 램프 TrilRenderer 실행
            kartBodyCtrl.SetLampTrailActive(true);

            StartBoost(boostDuration);
            boostCount--;
            isBoostUsed = true;
        }
        // 부스트 게이지 충전
        if (currentMotorInput != 0 || isDrifting)
        {
            ChargeBoostGauge();
        }

        if (boostGauge >= maxBoostGauge)
        {
            isBoostCreate = true;
            boostGauge = 0;
            chargeAmount = 0;
            if (boostCount < 2)
            {
                boostCount++;
            }
        }
    }

    #endregion

    #region [드리프트 관련 함수]

    // 드리프트 관련 파라미터 업데이트
    private void UpdateDriftParameters()
    {
        float currentSpeed = rigid.velocity.magnitude;
        float speedFactor = Mathf.Clamp01(currentSpeed / maxSpeed * 2f);

        // 속도에 따라 드리프트 입력 민감도와 힘 배수 업데이트
        currentDriftThreshold = Mathf.Lerp(minDriftAngle, maxDriftAngle, speedFactor);
        driftForceMultiplier = Mathf.Lerp(minDriftForceMultiplier, maxDriftForceMultiplier, speedFactor);
    }

    // 드리프트 시작 처리
    private void StartDrift(float driftInputAngle)
    {
        isDrifting = true;
        currentDriftAngle = driftInputAngle;
        lockedYRotation = transform.eulerAngles.y;  // 드리프트 시작 시 현재 회전값 고정

        // 속도와 조향 입력에 따른 드리프트 지속시간 결정
        float currentSpeed = rigid.velocity.magnitude;
        float speedFactor = Mathf.Clamp01(currentSpeed / maxSpeed);
        float steerFactor = Mathf.Abs(driftInputAngle / currentDriftThreshold);
        float influenceFactor = (speedFactor + steerFactor) / 2f;
        driftDuration = Mathf.Lerp(minDriftDuration, maxDriftDuration, influenceFactor);

        Debug.Log($"드리프트 시작: 입력각={driftInputAngle}, 속도={currentSpeed}, 조향비={steerFactor}, 지속시간={driftDuration:F2}초");

        // 지정된 시간 후 드리프트 종료 예약
        Invoke(nameof(EndDrift), driftDuration);
    }

    // 드리프트 중 입력에 따른 각도 업데이트
    private void UpdateDriftAngle()
    {
        currentDriftAngle += Time.deltaTime * 10f;
        currentDriftAngle = Mathf.Clamp(currentDriftAngle, -maxDriftAngle, maxDriftAngle);
    }

    // 드리프트 종료 처리 및 즉시 부스트 입력 대기
    private void EndDrift()
    {
        isDrifting = false;
        currentDriftAngle = 0f;
        Debug.Log("드리프트 종료");

        // 드리프트 종료 후 0.5초 동안 즉시 부스트 입력 대기
        if (postDriftBoostCoroutine != null)
        {
            StopCoroutine(postDriftBoostCoroutine);
        }
        postDriftBoostCoroutine = StartCoroutine(PostDriftBoostCoroutine());

        initialDriftSpeed = 0f;
    }
    #endregion

    #region [부스터 관련 함수]

    // 드리프트 종료 후 바로 부스트 입력을 받기 위한 코루틴
    private IEnumerator PostDriftBoostCoroutine()
    {
        float timer = 0f;
        bool boosted = false;
        Debug.Log("순간 부스트 입력 대기 중...");

        while (timer < 0.3f)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                PerformInstantBoost(momentboostDuration);
                boosted = true;
                break;
            }
            timer += Time.deltaTime;
            yield return null;
        }

        if (!boosted)
        {
            Debug.Log("즉시 부스트 입력 시간 초과");
        }

        postDriftBoostCoroutine = null;
    }

    // 드리프트 후 즉시 부스트 (현재 속력의 1.2배 증폭)
    private void PerformInstantBoost(float momentboostDuration)
    {
        Debug.Log("즉시 부스트 활성화!");
        rigid.velocity *= 1.2f;
        postDriftBoostCoroutine = null;
    }
    // 부스트 종료 처리
    private void EndBoost()
    {
        isBoostTriggered = false;
        Debug.Log("부스트 종료");
    }
    // 부스트 게이지 충전 처리 (드리프트 시와 일반 충전 속도 구분)
    private void ChargeBoostGauge()
    {
        chargeAmount += isDrifting
            ? driftBoostChargeRate * Time.fixedDeltaTime  // 드리프트 중 충전 속도
            : boostChargeRate * Time.fixedDeltaTime;       // 일반 충전 속도

        boostGauge = Mathf.Clamp(chargeAmount, 0, maxBoostGauge);
        if (boostGauge >= maxBoostGauge)
        {
            Debug.Log("부스트 게이지 최대치 도달!");
        }
    }
    /// <summary>
    /// 부스트를 시작하는 함수. 코루틴을 호출해 현재 속도에서 최대 부스트 속도까지 점진적 가속을 구현합니다.
    /// </summary>
    /// <param name="duration">부스트 지속 시간(초)</param>
    private void StartBoost(float duration)
    {
        if (isBoostTriggered) return;  // 이미 부스트 중이면 무시

        // 코루틴 실행: BoostCoroutine이 매 프레임 속도를 업데이트합니다.
        StartCoroutine(BoostCoroutine(duration));
    }

    /// <summary>
    /// BoostCoroutine 코루틴은 boostDuration 동안 linearly (선형 보간) 현재 속도에서 boostMaxSpeed에 도달하도록 가속을 조절합니다.
    /// </summary>
    /// <param name="duration">부스트 지속 시간(초)</param>
    /// <returns></returns>
    // Inspector에서 설정 가능한 AnimationCurve들
    public AnimationCurve boostAccelerationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    public AnimationCurve boostDecelerationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private IEnumerator BoostCoroutine(float duration)
    {
        isBoostTriggered = true;

        // 전체 부스터 시간을 가속/감속 기간으로 분할
        float accelDuration = duration * 0.5f;
        float decelDuration = duration * 0.5f;

        // 부스터 시작 시점 속도 (실제 m/s 단위)
        float startSpeed = rigid.velocity.magnitude;

        float timer = 0f;

        // [Phase 1] Acceleration: startSpeed -> boostMaxSpeed
        while (timer < accelDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / accelDuration);
            // acceleration curve를 사용해 비선형 보간 적용
            float curveValue = boostAccelerationCurve.Evaluate(t);
            float newSpeed = Mathf.Lerp(startSpeed, boostMaxSpeed, curveValue);

            // forward 방향으로 새 속도 적용
            rigid.velocity = transform.forward * newSpeed;
            yield return null;
        }

        // 가속 종료 후 확실히 boostMaxSpeed를 적용
        rigid.velocity = transform.forward * boostMaxSpeed;

        // [Phase 2] Deceleration: boostMaxSpeed -> maxSpeed (혹은 다른 기본 속도)
        timer = 0f;
        while (timer < decelDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / decelDuration);
            // deceleration curve를 사용해 감속 곡선 적용
            float curveValue = boostDecelerationCurve.Evaluate(t);
            float newSpeed = Mathf.Lerp(boostMaxSpeed, maxSpeed, curveValue);

            rigid.velocity = transform.forward * newSpeed;
            yield return null;
        }

        // 램프 TrailRenderer 끄기
        kartBodyCtrl.SetLampTrailActive(false);

        // 감속이 끝났으면 부스터 종료 플래그 해제
        isBoostTriggered = false;
        Debug.Log("부스트 종료");
    }

    #endregion

    #region [전진,후진,조향 값 계산]

    private void HandleKartMovement(float motorInput, float steerInput)
    {
        float steeringMultiplier = Mathf.Lerp(minSteerMultiplier, maxSteerMultiplier, 0.8f);

        // 드리프트 중인지 아닌지에 따라 별도 처리
        if (isDrifting)
        {
            ProcessDrift(steerInput, steeringMultiplier);
        }
        else
        {
            initialDriftSpeed = 0f;
            ProcessAcceleration(motorInput, currentMaxSpeed);
        }

        // 조향 민감도를 적용해 회전 처리
        RotateKart(steerInput, steeringMultiplier);

        // 바퀴 업데이트 (필요 시)
        if (wheelCtrl != null)
        {
            wheelCtrl.UpdateAndRotateWheels(steerInput, motorInput, rigid.velocity.magnitude, isDrifting);
        }
    }

    /// <summary>
    /// 드리프트 상태일 때 속도 처리 및 측면 힘 적용
    /// </summary>
    private void ProcessDrift(float steerInput, float steeringMultiplier)
    {
        // 드리프트 시작 시 최초 속도를 기록합니다.
        if (Mathf.Approximately(initialDriftSpeed, 0f))
        {
            initialDriftSpeed = rigid.velocity.magnitude;
        }
        float driftSpeed = initialDriftSpeed * driftSpeedReduction;

        // 조향 입력에 민감도를 적용하여 누적 회전을 보정합니다.
        lockedYRotation += steerInput * ((steerAngle / 2.5f) * steeringMultiplier) * Time.fixedDeltaTime;
        Quaternion driftRotation = Quaternion.Euler(0f, lockedYRotation, 0f);
        Vector3 driftDirection = driftRotation * Vector3.forward;

        // 현재 속도를 부드럽게 드리프트 방향으로 전환합니다.
        rigid.velocity = Vector3.Lerp(rigid.velocity, driftDirection * driftSpeed, Time.fixedDeltaTime * 5f);

        // 측면 힘을 추가하여 드리프트 느낌을 강화합니다.
        Vector3 lateralForce = transform.right * steerInput * driftForceMultiplier * movementForce;
        rigid.AddForce(lateralForce, ForceMode.Force);
    }

    /// <summary>
    /// 전진/후진 가속 처리 (드리프트가 아닐 때, 후진 부드럽게 전환)
    /// </summary>
    private void ProcessAcceleration(float motorInput, float currentMaxSpeed)
    {
        // 전진/후진 입력에 따른 가속도 계산
        Vector3 acceleration = transform.forward * movementForce * motorInput * Time.fixedDeltaTime;

        //// 추가: 언덕 보조 힘 적용 (지면에 붙어 있을 때만)
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit))
        {
            // 지면의 기울기를 계산 (노멀과 위쪽 벡터 사이의 각도)
            float groundAngle = Vector3.Angle(hit.normal, Vector3.up);
            if (groundAngle > uphillAngleThreshold && motorInput > 0)
            {
                // 언덕에서 전진할 때 보조 힘 적용
                acceleration *= uphillForceMultiplier;
            }
        }

        // 전진/후진 방향에 따라 속도 전환 처리
        Vector3 currentVelocity = rigid.velocity;
        if (Mathf.Abs(motorInput) > 0.1f)
        {
            // motorInput 값에 따라 속도를 해당 방향으로 설정
            Vector3 desiredDirection = motorInput > 0 ? transform.forward : -transform.forward;
            currentVelocity = Vector3.Lerp(currentVelocity, desiredDirection * currentMaxSpeed, Time.fixedDeltaTime * 2f);
        }

        // Y축 속도는 기존대로 유지
        rigid.velocity = new Vector3(currentVelocity.x, rigid.velocity.y, currentVelocity.z);

        // lockedYRotation 갱신
        lockedYRotation = transform.eulerAngles.y;
    }

    /// <summary>
    /// 조향 입력과 속도 기반 민감도를 적용해 회전 처리를 담당합니다.
    /// </summary>
    private void RotateKart(float steerInput, float steeringMultiplier)
    {
        Vector3 turnDirection = Quaternion.Euler(0, steerInput * steerAngle * steeringMultiplier * Time.fixedDeltaTime, 0) * transform.forward;
        rigid.MoveRotation(Quaternion.LookRotation(turnDirection));
    }

    #endregion
    #region [박스 캐스트 인스펙터 설정]

    [Header("박스 캐스트 설정")]
    [SerializeField] private Vector3 boxCastCenter = Vector3.zero;     // 박스 캐스트 중심 오프셋
    [SerializeField] private Vector3 boxCastSize = new Vector3(1, 1, 1); // 박스 크기
    [SerializeField] private float boxCastDistance = 1f;               // 박스 캐스트 거리
    [SerializeField] private float groundRayDistance = 0.8f;               // 박스 캐스트 거리

    [Header("레이어 설정")]
    [SerializeField] private LayerMask wallLayer;     // 벽 레이어
    [SerializeField] private LayerMask jumpLayer;     // 점프 레이어
    [SerializeField] private LayerMask boosterLayer;  // 부스터 레이어
    [SerializeField] private LayerMask groundLayer;   // 지면 레이어


    // 충돌된 객체 정보를 저장할 변수
    private RaycastHit lastHit;

    /// <summary>
    /// 박스 캐스트로 충돌 여부를 확인합니다.
    /// </summary>
    private bool PerformBoxCast(LayerMask layer)
    {
        Vector3 worldCenter = transform.position + transform.TransformDirection(boxCastCenter);
        return Physics.BoxCast(worldCenter, boxCastSize * 0.5f, transform.forward, out lastHit, Quaternion.identity, boxCastDistance, layer.value);
    }

    /// <summary>
    /// 충돌된 레이어를 판별합니다.
    /// </summary>
    private void HandleLayerCollision()
    {
        if (lastHit.collider != null)
        {
            int hitLayer = lastHit.collider.gameObject.layer;

            if (((1 << hitLayer) & wallLayer.value) != 0)
            {
                Debug.Log("충돌: 벽 레이어");
                ProcessWallCollision();
            }
            else if (((1 << hitLayer) & jumpLayer.value) != 0)
            {
                Debug.Log("충돌: 점프 레이어");
                ProcessJumpCollision();
            }
            else if (((1 << hitLayer) & boosterLayer.value) != 0)
            {
                Debug.Log("충돌: 부스터 레이어");
                ProcessBoosterCollision();
            }
            else if (((1 << hitLayer) & groundLayer.value) != 0)
            {
                Debug.Log("충돌: 지면 레이어");
                ProcessGroundCollision();
            }
            else
            {
                Debug.Log("충돌: 정의되지 않은 레이어");
            }
        }
    }

    /// <summary>
    /// 벽과 충돌 처리
    /// </summary>
    private void ProcessWallCollision()
    {
        // 벽 충돌 처리 로직 추가
        Debug.Log("벽 충돌 처리 실행됨!");
    }

    /// <summary>
    /// 점프 레이어와 충돌 처리
    /// </summary>
    private void ProcessJumpCollision()
    {
        // 점프 레이어 충돌 처리 로직 추가
        Debug.Log("점프 충돌 처리 실행됨!");
    }

    /// <summary>
    /// 부스터 레이어와 충돌 처리
    /// </summary>
    private void ProcessBoosterCollision()
    {
        // 부스터 충돌 처리 로직 추가
        Debug.Log("부스터 충돌 처리 실행됨!");
    }

    /// <summary>
    /// 지면과 충돌 처리
    /// </summary>
    private void ProcessGroundCollision()
    {
        // 지면 충돌 처리 로직 추가
        Debug.Log("지면 충돌 처리 실행됨!");
    }

    /// <summary>
    /// 공중 상태에서 착지 속도를 현재 속도에 비례하여 강화합니다.
    /// </summary>
    private void ApplyEnhancedGravity()
    {
        // 공중 상태 확인
        if (!CheckIfGrounded())
        {
            // 현재 수평 속도 계산
            float currentHorizontalSpeed = new Vector3(rigid.velocity.x, 0f, rigid.velocity.z).magnitude;

            // 속도에 비례한 중력 계수 계산
            float gravityMultiplier = Mathf.Lerp(1f, 3f, currentHorizontalSpeed / currentMaxSpeed);

            // 추가적인 중력 효과 적용
            Vector3 enhancedGravity = Physics.gravity * gravityMultiplier;
            rigid.AddForce(enhancedGravity, ForceMode.Acceleration);

            Debug.Log("강화된 중력 적용됨: " + enhancedGravity + " | 현재 속도: " + currentHorizontalSpeed);
        }
    }

    #endregion

    #region [지면 체크]

    // 지면과의 접촉 확인
    private bool CheckIfGrounded()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, groundRayDistance, groundLayer))
        {
            Debug.Log("지면 감지됨: " + hit.collider.name);
            return true;
        }
        Debug.Log("지면 감지되지 않음");
        return false;
    }

    #endregion
   
}