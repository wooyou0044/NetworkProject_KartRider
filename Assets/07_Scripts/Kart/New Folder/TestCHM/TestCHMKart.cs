using System.Collections;
using Cinemachine;
using Photon.Pun;
using UnityEngine;

public partial class TestCHMKart : MonoBehaviour
{
    #region Serialized Fields

    [Header("카트 구성 요소")]
    [SerializeField] private GameObject wheels;   // 바퀴 오브젝트
    [SerializeField] GameObject kartBody; // 카트 바디 오브젝트

    [Header("이동 설정")]
    [SerializeField] private float maxSpeedKmh = 200f;           // 최대 속도
    [SerializeField] private float movementForce = 20f;       // 이동에 적용할 힘
    [SerializeField] private float steerAngle = 100f;         // 조향(회전) 각도    
    [SerializeField] private float minSteerMultiplier = 1f;  // 최소 조향 배수 (속도가 낮을 때)
    [SerializeField] private float maxSteerMultiplier = 2f;  // 최대 조향 배수 (속도가 최대일 때)

    [Header("드리프트 설정")]
    [SerializeField] private float minDriftAngle = 30f;       // 최소 드리프트 각도
    [SerializeField] public float maxDriftAngle = 180f;       // 최대 드리프트 각도
    [Header("드리프트 지속시간")]
    [SerializeField] private float minDriftDuration = 0.2f;     // 최소 드리프트 지속시간
    [SerializeField] private float maxDriftDuration = 2f;       // 최대판단 드리프트 지속시간
    [SerializeField] private float totalMaxDriftDuration = 2f;       // 최대판단 드리프트 지속시간
    [Header("드리프트 힘")]
    [SerializeField] private float minDriftForceMultiplier = 1f;// 최소 드리프트 힘 배수
    [SerializeField] private float maxDriftForceMultiplier = 5f;// 최대 드리프트 힘 배수
    [SerializeField] private float driftSpeedReduction = 0.7f;  // 드리프트 시 속도 감소 비율
    [SerializeField] private float driftRecoverySpeed = 2f;  // 드리프트 복구 속도 제어    
    [SerializeField] private float curveDriftSpeed = 5f;  // 드리프트 전환 속도 제어
    [SerializeField] private float boostSpeedIncrease = 1.2f;  // 드리프트시 부스터 힘

    [Header("부스트 설정")]
    [SerializeField] public float boostDuration = 1.5f;              // 기본 부스트 지속시간
    [SerializeField] public float momentboostDuration = 0.5f;       // 순간 부스트 지속시간
    [Header("부스트 게이지 설정")]
    [SerializeField] public int maxBoostGauge = 100;                // 최대 부스트 게이지
    [SerializeField] private float boostChargeRate = 5f;             // 기본 부스트 충전 속도
    [SerializeField] private float driftBoostChargeRate = 10f;        // 드리프트 중 부스트 충전 속도    
    [Header("부스트 파워 설정")]
    [SerializeField] private float maxBoostSpeed = 82.5f;            // 부스트 활성화 시 최대 속도 속력 변환 해줘야함 지금 시속 300 = m/s 82.5 가 딱임 
    [SerializeField] private float boostMultiplier = 1.1f;          // 지속적인 부스터 효과 계수

    public TestCHMCamer camerCtrl { get; set; }
    
    #endregion

    #region Private Fields

    public Animator playerCharAni { get; set; }
    public float speedKM { get; private set; }     // 현재 속력 (km/h 단위) 계기판 출력 속력
    public bool isBoostTriggered { get; set; } // 부스트 활성화 여부
    //public bool isBoostCreate { get; set; }    // 드리프트 아이템 생성 가능 여부
    public float boostGauge { get; private set; }                // 현재 부스트 게이지
    public bool isItemUsed { get; set; }
    public bool isRacingStart { get; set; } //시작 대기 변수
    public bool isGameFinished { get; set; } //결승선 변수

    public float totalBoostDuration { get; private set; }


    public bool isDrifting = false;            // 드리프트 진행 중 여부
    public float currentDriftAngle = 0f;       // 현재 누적 드리프트 각도 카메라에서 써야함 
    private CHMTestWheelController wheelCtrl;  // 바퀴 제어 스크립트
    private Rigidbody rigid;                   // 리지드바디 (물리 처리)
    private Coroutine postDriftBoostCoroutine; // 드리프트 종료 후 부스트 처리를 위한 코루틴 변수
    private float driftDuration;               // 부스터 지속 시간
    private float initialDriftSpeed;           // 드리프트 시작 시 기록된 초기 속도
    private float currentDriftThreshold;       // 속도에 따른 드리프트 입력 기준 값
    private float driftForceMultiplier;        // 동적으로 계산된 드리프트 힘 배수
    private float lockedYRotation = 0f;        // 드리프트 시 고정되는 Y 회전값
    private float currentMotorInput;
    private float currentSteerInput;
    private float maxSpeed;                    // 최대 속도 (m/s)
    private Vector3 speed;                     // 현재 속도 벡터
    private float chargeAmount;
    private bool startBoost = true;
    private bool wasAirborne = false;         //공중 상태 여부 
    private float currentSteerAngle = 0f;// 조향을 부드럽게 처리하는 변수



    /* Network Instantiate */
    private Transform _playerParent;
    private Transform _tr;
    private PhotonView _photonView;

    KartBodyController kartBodyCtrl;
    KartInventory inventory;
    AudioSource audioSource;

    [SerializeField] AudioClip driftAudioClip;
    [SerializeField] AudioClip boostAudioClip;

    bool isSparkOn;
    float inputKey;

    #endregion

    #region Unity Methods

    private void Awake()
    {
        wheelCtrl = wheels.GetComponent<CHMTestWheelController>(); // 바퀴 컨트롤러 참조
        kartBodyCtrl = kartBody.GetComponent<KartBodyController>();
        inventory = GetComponent<KartInventory>();
        camerCtrl = GetComponent<TestCHMCamer>();
        
        rigid = GetComponent<Rigidbody>();                         // 리지드바디 참조

        /* TODO : 포톤 붙일때 수정해주기 */
        _tr = gameObject.transform;
        _photonView = GetComponent<PhotonView>();
        _playerParent = GameObject.Find("Players").transform;
        transform.parent = _playerParent;

        audioSource = GetComponent<AudioSource>();

    }

    private void FixedUpdate()
    {
        if (!_photonView.IsMine) return;

        if (isRacingStart == false)
        {
            return;
        }
        // 최대 속도 계산
        maxSpeed = maxSpeedKmh / 3.6f; // 일반 최대 속도                                      
        // 입력값 읽어오기
        currentSteerInput = Input.GetAxis("Horizontal");
        currentMotorInput = Input.GetAxis("Vertical");
        // 이동 처리
        HandleKartMovement(currentMotorInput, currentSteerInput);
        StartRace();//순간 부스트 실행           


        // 속도 제한 로직 제거 또는 완화
        if (!isBoostTriggered)
        {
            Vector3 horizontalVelocity = new Vector3(rigid.velocity.x, 0f, rigid.velocity.z);
            float speed = horizontalVelocity.magnitude;

            if (speed > maxSpeed) // 일반 상태에서만 속도를 제한
            {
                rigid.velocity = horizontalVelocity.normalized * maxSpeed + Vector3.up * rigid.velocity.y;
            }
        }
        int combinedLayer = wallLayer.value | boosterLayer.value | groundLayer.value; // 충돌처리

        if (PerformCombinedBoxCast(combinedLayer))
        {
            HandleLayerCollision();
        }


        ProcessSlopeForce();//경사면 체크 
                            // 레이캐스트로 지면 체크
                            // 공중 상태일 때 추가 하강력 적용
        if (!CheckIfGrounded())
        {
            wasAirborne = true;
            ApplyAirborneLandingForce();
            rigid.angularDrag = 0f;
        }
        else
        {
            // 만약 이전 프레임에 공중 상태였다면, 착지한 순간이므로 수평 속도를 보존하도록 처리
            if (wasAirborne)
            {
                ProcessLanding();
                wasAirborne = false;
            }
        }

    }

    private void Update()
    {
        if (!_photonView.IsMine)
        {
            return;
        }

        if (isRacingStart == false)
        {
            return;
        }

        // 드리프트 상태에 따라 복구 처리
        if (!isDrifting)
        {
            RecoverDriftAngle();
        }
        // 현재 속도 갱신 (Y축 제외) 및 km/h 변환
        speed = new Vector3(rigid.velocity.x, 0f, rigid.velocity.z);
        speedKM = speed.magnitude * 3.6f;


        // 드리프트 관련 파라미터 업데이트
        UpdateDriftParameters();

        // 드리프트 입력 처리
        HandleDriftInput(currentSteerInput);

        // 부스트 입력 처리
        HandleItemInput();

        playerCharAni.SetBool("IsBoosting", isBoostTriggered);


        if (isUsingShield == false)
        {
            if (kartBodyCtrl.shield.activeSelf == true)
            {
                kartBodyCtrl.SetShieldEffectActive(false);
            }
        }
    }
    #endregion

    #region [드리프트 관련 함수] 
    private float totalDriftDuration = 0f; // 드리프트 연계시 누적된 지속시간

    // 드리프트 관련 파라미터 업데이트 (속도에 따라 민감도와 힘 배수 조정)
    private void UpdateDriftParameters()
    {
        float currentSpeed = rigid.velocity.magnitude;
        float speedFactor = Mathf.Clamp01(currentSpeed / maxSpeed * 2f);

        currentDriftThreshold = Mathf.Lerp(minDriftAngle, maxDriftAngle, speedFactor);
        driftForceMultiplier = Mathf.Lerp(minDriftForceMultiplier, maxDriftForceMultiplier, speedFactor);
    }

    // 드리프트 시작 처리 (최초 입력 시)
    private void StartDrift(float driftInputAngle)
    {
        isDrifting = true;
        lockedYRotation = transform.eulerAngles.y; // 드리프트 시작 시 회전값 고정

        // 초기 방향 설정: 입력의 50%만 반영하여 부드러운 시작
        float initialFixedAngle = driftInputAngle * 0.5f;
        currentDriftAngle = Mathf.Clamp(initialFixedAngle, -maxDriftAngle, maxDriftAngle);

        // 속도와 초기 조향 입력을 기반으로 드리프트 지속시간 계산
        float currentSpeed = rigid.velocity.magnitude;
        float speedFactor = Mathf.Clamp01(currentSpeed / maxSpeed);
        float steerFactor = Mathf.Abs(driftInputAngle / currentDriftThreshold);
        float influenceFactor = (speedFactor + steerFactor) / 2f;
        driftDuration = Mathf.Lerp(minDriftDuration, maxDriftDuration, influenceFactor);

        // 누적 지속시간 초기화 (최초 시작에서는 새 지속시간으로 초기화)
        totalDriftDuration = Mathf.Min(driftDuration, totalMaxDriftDuration); // 최대 지속 시간 제한

        // 최대 지속 시간을 초과할 경우 바로 종료 예약
        if (totalDriftDuration >= totalMaxDriftDuration)
        {
            Debug.Log("[StartDrift] 누적 지속 시간이 최대 지속 시간을 초과하여 즉시 종료 예약");
        }

        // 지정된 누적 지속시간 후 드리프트 종료 예약
        CancelInvoke(nameof(EndDrift)); // 기존 예약 제거
        Invoke(nameof(EndDrift), totalDriftDuration);

        //Debug.Log($"[StartDrift] 초기 각도={currentDriftAngle:F2}, 새 지속시간={driftDuration:F2}, 누적 지속시간={totalDriftDuration:F2}");
    }

    // 기본 드리프트 각도 업데이트
    private void UpdateDriftAngle(float steerInput)
    {
        float currentSpeed = rigid.velocity.magnitude;
        float speedFactor = Mathf.Clamp01(currentSpeed / maxSpeed);
        float adjustedInput = steerInput * speedFactor;
        currentDriftAngle += adjustedInput * driftForceMultiplier * Time.deltaTime;
        currentDriftAngle = Mathf.Clamp(currentDriftAngle, -maxDriftAngle, maxDriftAngle);

        // 차량 이동 방향 업데이트
        lockedYRotation += steerInput * (steerAngle / 2.5f) * Time.deltaTime;
        Quaternion driftRotation = Quaternion.Euler(0f, lockedYRotation, 0f);
        Vector3 driftDirection = driftRotation * Vector3.forward;
        rigid.velocity = Vector3.Lerp(rigid.velocity, driftDirection * initialDriftSpeed, Time.deltaTime * 5f);

        //Debug.Log($"[UpdateDriftAngle] 현재 각도={currentDriftAngle:F2}");
    }

    // 커브 드리프트 처리
    private void CurveDrift(float steerInput)
    {
        // 드리프트 연계 시간이 남아 있을 때만 처리
        if (!IsChainActive()) return;

        float targetDriftAngle = steerInput * maxDriftAngle; // 반대 방향 설정
        currentDriftAngle = Mathf.Lerp(currentDriftAngle, targetDriftAngle, Time.deltaTime * curveDriftSpeed);

        lockedYRotation += steerInput * (steerAngle / 2.5f) * Time.deltaTime;
        Quaternion curveRotation = Quaternion.Euler(0f, lockedYRotation, 0f);
        Vector3 driftDirection = curveRotation * Vector3.forward;
        rigid.velocity = Vector3.Lerp(rigid.velocity, driftDirection * initialDriftSpeed, Time.deltaTime * 5f);

        if (isSparkOn == true)
        {
            inputKey = steerInput;
            kartBodyCtrl.SetDriftSparkActive(true, steerInput);
            //audioSource.PlayOneShot(driftAudioClip);
            PlayDriftEffectSound();
            isSparkOn = false;
        }
        //Debug.Log($"[CurveDrift] 새 각도={currentDriftAngle:F2}, 입력={steerInput:F2}");
    }

    // 더블 드리프트 처리
    private void DoubleDrift(float steerInput)
    {
        // 연계 지속시간 내에서만 작동
        if (!IsChainActive()) return;

        float adjustedAngle = steerInput * driftForceMultiplier * Time.deltaTime;
        currentDriftAngle += adjustedAngle;
        currentDriftAngle = Mathf.Clamp(currentDriftAngle, -maxDriftAngle, maxDriftAngle);

        lockedYRotation += steerInput * (steerAngle / 2.5f) * Time.deltaTime;
        Quaternion driftRotation = Quaternion.Euler(0f, lockedYRotation, 0f);
        Vector3 driftDirection = driftRotation * Vector3.forward;
        rigid.velocity = Vector3.Lerp(rigid.velocity, driftDirection * initialDriftSpeed, Time.deltaTime * 5f);

        if (isSparkOn == true)
        {
            inputKey = steerInput;
            kartBodyCtrl.SetDriftSparkActive(true, steerInput);
            //audioSource.PlayOneShot(driftAudioClip);
            PlayDriftEffectSound();
            isSparkOn = false;
        }
        //Debug.Log($"[DoubleDrift] 현재 각도={currentDriftAngle:F2}");
    }

    // 드리프트 종료 처리 (연계 종료되면 호출됨)
    private void EndDrift()
    {
        isDrifting = false;
        currentDriftAngle = 0f;

        if (postDriftBoostCoroutine != null)
        {
            StopCoroutine(postDriftBoostCoroutine);
        }
        postDriftBoostCoroutine = StartCoroutine(PostDriftBoostCoroutine());

        initialDriftSpeed = 0f;
        RecoverDriftAngle();

        kartBodyCtrl.SetDriftSparkActive(false, inputKey);
        audioSource.Stop();
        //Debug.Log("[EndDrift] 드리프트 종료");
    }

    // 드리프트 종료 후 각도 복구 처리
    private void RecoverDriftAngle()
    {
        if (!isDrifting)
        {
            currentDriftAngle = Mathf.Lerp(currentDriftAngle, 0f, Time.deltaTime * driftRecoverySpeed);
        }
    }

    private void ChainDrift(float steerInput)
    {
        // 입력이 충분할 때만 연계 (작은 입력은 건너뜁니다)
        if (Mathf.Abs(steerInput) < 0.1f) return;

        float currentSpeed = rigid.velocity.magnitude;
        float speedFactor = Mathf.Clamp01(currentSpeed / maxSpeed);
        float steerFactor = Mathf.Abs(steerInput / currentDriftThreshold);
        float influenceFactor = (speedFactor + steerFactor) / 2f;
        float newDriftDuration = Mathf.Lerp(minDriftDuration, maxDriftDuration, influenceFactor);

        // 누적 지속시간에 추가 (최대 지속 시간을 초과하지 않도록 제한)
        totalDriftDuration = Mathf.Min(totalDriftDuration + newDriftDuration, totalMaxDriftDuration);

        // 최대 지속 시간을 초과하지 않도록 드리프트 종료 예약
        CancelInvoke(nameof(EndDrift)); // 기존 예약 제거
        Invoke(nameof(EndDrift), totalDriftDuration);

        // Debug.Log($"[ChainDrift] 추가 지속시간={newDriftDuration:F2}, 누적 지속시간={totalDriftDuration:F2}");
    }

    // 현재 연계 시간이 아직 남아있는지 확인 (항상 true를 반환해도 됨)
    // 혹은 필요하다면 총 누적 지속시간을 바탕으로 판단할 수 있음.
    private bool IsChainActive()
    {
        // 여기서는 totalDriftDuration이 0보다 크면 연계 중으로 판단
        return totalDriftDuration > 0f;
    }

    // 입력 처리 함수 : 드리프트 시작 및 진행 중 분기
    private void HandleDriftInput(float steerInput)
    {
        if (isDrifting)
        {
            // 반대 방향 입력 감지
            if (Mathf.Sign(steerInput) != Mathf.Sign(currentDriftAngle))
            {
                // 0.5초 후 드리프트 종료 예약
                Invoke(nameof(EndDrift), 0.2f);
                //Debug.Log("반대 방향 입력: 0.5초 후 드리프트 종료 예약");

                // 좌쉬프트 + 반대 입력 -> 커브 드리프트 실행
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    isSparkOn = true;
                    CurveDrift(steerInput);
                    //Debug.Log("커브 드리프트 실행: 좌쉬프트 + 반대 입력");
                }
                return;
            }

            // 같은 방향 + 좌쉬프트 -> 더블 드리프트 실행
            if (Input.GetKey(KeyCode.LeftShift))
            {
                isSparkOn = true;
                DoubleDrift(steerInput);
            }
            else
            {
                // 기본 각도 업데이트
                UpdateDriftAngle(steerInput);
            }

            // 연계 드리프트라면, 매 입력 시 추가 지속시간을 누적
            ChainDrift(steerInput);
        }
        else
        {
            // 드리프트 시작 조건: 좌쉬프트 키와 충분한 조향 입력이 있을 때
            if (Input.GetKeyDown(KeyCode.LeftShift) && Mathf.Abs(steerInput) > 0.1f)
            {
                // StartDrift에서 currentDriftThreshold를 곱해 초기 입력각도를 조정하여 줌
                StartDrift(steerInput * currentDriftThreshold);
            }
        }
    }

    #endregion

    #region [부스터 관련 함수]          
    /// <summary>
    /// 기본 부스터 힘을 점진적으로 적용합니다.
    /// </summary>
    /// <param name="elapsedTime">부스터가 활성화된 후 경과 시간</param>
    /// <param name="duration">전체 부스터 지속 시간</param>
    private void ApplyBasicBoostForce(float elapsedTime, float duration)
    {
        // 기본 힘과 최대 힘 설정
        float baseBoostForce = 100f;  // 초기 부스터 힘
        float maxBoostForce = 500f; // 최대 부스터 힘

        // 경과 시간 비례로 힘을 점진적으로 증가
        float dynamicBoostForce = Mathf.Lerp(baseBoostForce, maxBoostForce, elapsedTime / duration);

        // 부스터 힘 방향 계산
        Vector3 boostDirection = transform.forward * dynamicBoostForce;
        rigid.velocity = boostDirection;
        // 차량에 부스터 힘 적용
        //rigid.AddForce(boostDirection, ForceMode.Acceleration);
    }
    /// <summary>
    /// 현재 속력 받아서 추가로 힘줌 
    /// </summary>
    private void ApplyOriginalBoostForce()
    {
        // 기존 부스터 속도 처리
        float currentSpeed = rigid.velocity.magnitude;
        Vector3 boostForce = transform.forward * currentSpeed * boostMultiplier * Time.fixedDeltaTime * 10f;

        rigid.AddForce(boostForce, ForceMode.Acceleration);
    }

    // 드리프트 종료 후 바로 부스트 입력을 받기 위한 코루틴
    private IEnumerator PostDriftBoostCoroutine()
    {
        float timer = 0f;
        bool boosted = false;
        //Debug.Log("순간 부스트 입력 대기 중...");

        while (timer < 0.5f) // 입력 대기 시간 0.3초
        {
            if (Input.GetKeyDown(KeyCode.UpArrow)) // 순간 부스트 입력
            {
                TriggerInstantBoost();
                boosted = true;
                break;
            }
            timer += Time.deltaTime;
            yield return null;
        }
        postDriftBoostCoroutine = null;
    }

    private void TriggerInstantBoost()
    {
        if (isBoostTriggered) return; // 이미 부스트 중이면 무시

        //Debug.Log("순간 부스트 활성화!");
        isBoostTriggered = true;
        // 램프 TrilRenderer 실행
        kartBodyCtrl.SetLampTrailActive(true);
        kartBodyCtrl.SetBoostEffectActive(true);
        kartBodyCtrl.SetBoostWindEffectActive(true);

        totalBoostDuration = momentboostDuration;

        // 가속 소리 설정
        PlayBoostEffectSound();
        StartCoroutine(InstantBoostCoroutine()); // 순간 부스터 실행
    }

    private IEnumerator InstantBoostCoroutine()//순간 부스트 함수 
    {
        
        float timer = 0f;

        while (timer < momentboostDuration)
        {
            timer += Time.fixedDeltaTime;
            //// 기존 부스터 속도 처리
            //float currentSpeed = rigid.velocity.magnitude;
            //Vector3 boostForce = transform.forward * currentSpeed * boostMultiplier;
            //rigid.AddForce(boostForce, ForceMode.Acceleration);
            ApplyOriginalBoostForce();

            // 속도 제한 적용
            if (rigid.velocity.magnitude > maxBoostSpeed)
            {
                rigid.velocity = rigid.velocity.normalized * maxBoostSpeed;
            }

            yield return new WaitForFixedUpdate();
        }

        EndBoost();
    }
    private void EndBoost()
    {
        if (!isBoostTriggered) return; // 부스트가 이미 종료된 경우 무시

        //Debug.Log("부스트 종료: 속도 서서히 감소 시작");
        isBoostTriggered = false; // 부스트 상태 비활성화
        startBoost = false;//초기 부스트만 씀
        kartBodyCtrl.SetLampTrailActive(false);
        kartBodyCtrl.SetBoostEffectActive(false);
        kartBodyCtrl.SetBoostWindEffectActive(false);
        audioSource.Stop();
        audioSource.loop = false;
    }


    // 부스트 게이지 충전 처리 (드리프트 시와 일반 충전 속도 구분)
    private void ChargeBoostGauge()
    {
        chargeAmount += isDrifting
            ? driftBoostChargeRate * Time.deltaTime  // 드리프트 중 충전 속도
            : boostChargeRate * Time.deltaTime;       // 일반 충전 속도

        boostGauge = Mathf.Clamp(chargeAmount, 0, maxBoostGauge);
    }
    /// <summary>
    /// 부스트를 시작하는 함수. 현재 속도가 시속 50km 이하인 경우 기본 부스터를 사용, 아니라면 원래 부스터 로직 실행.
    /// </summary>
    /// <param name="duration">부스트 지속 시간(초)</param>
    private void StartBoost(float duration)
    {
        if (isBoostTriggered) return; // 이미 부스트 중이면 무시      

        totalBoostDuration = duration;

        // 부스트 활성화 설정
        isBoostTriggered = true;
        startBoost = false;

        // 효과 실행
        kartBodyCtrl.SetLampTrailActive(true);
        kartBodyCtrl.SetBoostEffectActive(true);
        kartBodyCtrl.SetBoostWindEffectActive(true);

        PlayBoostEffectSound();

        // 부스터 코루틴 시작
        StartCoroutine(BoostCoroutine(duration));
    }

    private IEnumerator BoostCoroutine(float duration)
    {
        float timer = 0f;

        // Booster 코루틴 내부에서 매 프레임마다
        while (timer < duration)
        {
            timer += Time.fixedDeltaTime;
            // 현재 속도를 m/s 단위로 계산
            float currentSpeed = rigid.velocity.magnitude;
            float minimumBoostSpeed = 13.89f; // 시속 50km를 m/s로 변환한 값

            // 시속 50km 이하일 경우 기본 부스터 적용
            if (currentSpeed < minimumBoostSpeed)
            {
                ApplyBasicBoostForce(timer, duration); // 기본 부스터 힘 적용
            }
            else
            {
                //float currentSpeed = rigid.velocity.magnitude;
                //Vector3 boostForce = transform.forward * currentSpeed * boostMultiplier;
                //rigid.AddForce(boostForce, ForceMode.Acceleration);
                ApplyOriginalBoostForce(); // 원래 부스터 로직 실행
            }

            // 속도 제한 적용 예시: 최대 부스트 속도가 maxBoostSpeed를 넘지 않도록 설정
            if (rigid.velocity.magnitude > maxBoostSpeed)
            {
                rigid.velocity = rigid.velocity.normalized * maxBoostSpeed;
            }

            yield return new WaitForFixedUpdate();
        }

        // 램프 TrailRenderer 끄기
        kartBodyCtrl.SetLampTrailActive(false);
        kartBodyCtrl.SetBoostEffectActive(false);
        kartBodyCtrl.SetBoostWindEffectActive(false);

        audioSource.Stop();
        audioSource.loop = false;

        // 감속이 끝났으면 부스터 종료 플래그 해제
        isBoostTriggered = false;
        //Debug.Log("부스트 종료");
    }
    #endregion

    #region [전진,후진,조향 값 계산]

    private void HandleKartMovement(float motorInput, float steerInput)
    {
        float steeringMultiplier = Mathf.Lerp(minSteerMultiplier, maxSteerMultiplier, 0.8f);
        if (steerInput != 0)
        {
            UpdateAnimator(steerInput);
        }

        // 드리프트 상태에 따라 별도 처리
        if (isDrifting)
        {
            ProcessDrift(steerInput, steeringMultiplier);
        }
        else
        {
            // 드리프트 상태가 아니면 즉각적인 가속 처리
            initialDriftSpeed = 0f;
            ProcessImmediateAcceleration(motorInput, maxSpeed);
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
    /// 드리프트 상태일 때 속도 처리 및 측면 힘 적용. 부스터 사용 시 속도 증가.
    /// </summary>
    private void ProcessDrift(float steerInput, float steeringMultiplier)
    {
        // 드리프트 시작 시 최초 속도를 기록합니다.
        if (Mathf.Approximately(initialDriftSpeed, 0f))
        {
            initialDriftSpeed = rigid.velocity.magnitude;
        }

        // 드리프트 속도 설정 (기본적으로 드리프트 시작 속도와 감소 인자 사용)
        float driftSpeed = initialDriftSpeed * driftSpeedReduction;

        // 부스터가 활성화된 경우, 부스터 속도가 우선 적용됩니다.
        if (isBoostTriggered)
        {
            driftSpeed = rigid.velocity.magnitude * boostMultiplier; // 부스터 속력 우선 적용
            // 부스터 속도가 maxBoostSpeed를 넘지 않도록 제한
            if (driftSpeed > maxBoostSpeed)
            {
                driftSpeed = maxBoostSpeed; // 최대 속도로 고정
            }

        }

        // 조향 입력에 민감도를 적용하여 누적 회전을 보정합니다.
        lockedYRotation += steerInput * ((steerAngle / 2.5f) * steeringMultiplier) * Time.fixedDeltaTime;
        Quaternion driftRotation = Quaternion.Euler(0f, lockedYRotation, 0f);
        Vector3 driftDirection = driftRotation * Vector3.forward;

        // 현재 속도를 부드럽게 드리프트 방향 또는 부스터 방향으로 전환합니다.
        rigid.velocity = Vector3.Lerp(rigid.velocity, driftDirection * driftSpeed, Time.fixedDeltaTime * 20f);

        // 측면 힘을 추가하여 드리프트 느낌을 강화합니다.
        Vector3 lateralForce = transform.right * steerInput * driftForceMultiplier * movementForce;
        rigid.AddForce(lateralForce, ForceMode.Force);
    }

    /// <summary>
    /// 드리프트가 아닐 때 즉각적으로 전진/후진 속도를 반영하는 함수.   
    /// </summary>
    private void ProcessImmediateAcceleration(float motorInput, float currentMaxSpeed)
    {
        // 목표 속도 계산: 전진/후진 입력과 최대 속도를 곱함
        Vector3 targetVelocity = transform.forward * motorInput * currentMaxSpeed;

        // 약간의 부드러운 전환을 위해 Lerp 사용 (계수가 높으면 거의 즉각적)
        Vector3 smoothedVelocity = Vector3.Lerp(new Vector3(rigid.velocity.x, 0f, rigid.velocity.z), targetVelocity, Time.fixedDeltaTime * 1.1f);

        // 수직 속도는 그대로 유지
        smoothedVelocity.y = rigid.velocity.y;

        // 최종 속도 적용
        rigid.velocity = smoothedVelocity;
    }


    /// <summary>
    /// 조향 입력과 속도 기반 민감도를 적용해 회전 처리를 담당합니다.
    /// </summary>  

    private void RotateKart(float steerInput, float steeringMultiplier)
    {
        if (speedKM > 0.1f)
        {
            // 드리프트 상태에서 다른 각도 제한 적용
            float maxSteerAngle = isDrifting ? 560f : 120f;  // 드리프트 중에는 60도, 기본은 90도

            // 목표 각도 계산
            float targetSteerAngle = steerInput * steerAngle * steeringMultiplier;

            // 부드러운 보간으로 현재 각도를 목표 각도로 점진적으로 변경
            currentSteerAngle = Mathf.Lerp(currentSteerAngle, targetSteerAngle, Time.fixedDeltaTime * 100f);

            // 각도를 -maxSteerAngle에서 +maxSteerAngle로 제한
            currentSteerAngle = Mathf.Clamp(currentSteerAngle, -maxSteerAngle, maxSteerAngle);

            // 회전 처리
            Vector3 turnDirection = Quaternion.Euler(0, currentSteerAngle * Time.fixedDeltaTime, 0) * transform.forward;
            rigid.MoveRotation(Quaternion.LookRotation(turnDirection));
        }
    }

    private void UpdateAnimator(float steerInput)
    {
        if (playerCharAni == null)
        {
            return;
        }

        if (playerCharAni != null)
        {
            // 조향 값을 기반으로 LSteer 및 RSteer 업데이트
            float LSteer = Mathf.Clamp01(-steerInput); // 왼쪽 조향
            float RSteer = Mathf.Clamp01(steerInput);  // 오른쪽 조향

            if (LSteer < 0.3f && RSteer < 0.3f)
            {
                LSteer = 0;
                RSteer = 0;
            }
            playerCharAni.SetFloat("LSteer", LSteer);
            playerCharAni.SetFloat("RSteer", RSteer);

        }

    }
    /// <summary>
    /// 게임이 시작될 때 부스트 대기 코루틴 시작
    /// </summary>
    private void StartRace()
    {
        if (startBoost == true)
        {
            StartCoroutine(WaitForInstantBoost());
            startBoost = false;
        }
    }

    /// <summary>
    /// 레이싱 시작 시 위 화살표 키 입력을 기다렸다가 부스트를 실행하는 함수.
    /// </summary>
    private IEnumerator WaitForInstantBoost()
    {
        startBoost = true; // 초기화
        float timer = 0f;

        // 0.2초 동안 키 입력을 대기
        while (timer < 0.4f)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                // 키 입력이 감지되면 TriggerInstantBoost 실행
                TriggerInstantBoost();
                startBoost = false;
                yield break; // 대기를 종료
            }

            timer += Time.deltaTime;
            yield return null;
        }

    }


    public IEnumerator DecelerateOverTime(float duration)//결승선 도착시 속력 서서히 줄어들기 
    {
        // 현재 전후진 입력 값을 저장
        float initialMotorInput = currentMotorInput;

        float timer = 0f;

        while (timer < duration)
        {
            // 전후진 입력을 서서히 감소
            speedKM = Mathf.Lerp(initialMotorInput, 0f, timer / duration);//계기판 속력 줄어들게
            currentMotorInput = Mathf.Lerp(initialMotorInput, 0f, timer / duration); // 진짜 속력 줄어들게 
            // 속도 클램핑 처리 (현재 속도 값 제한)
            rigid.velocity = Vector3.ClampMagnitude(rigid.velocity, maxSpeedKmh);

            timer += Time.deltaTime;
            yield return null;
        }
        
        //camerCtrl.ActivateFinishCamera(); //여기서하면 1초뒤에함
        // 최종적으로 전후진 입력 값을 완전히 0으로 설정
        currentMotorInput = 0f;
        currentSteerInput = 0f;
        playerCharAni.SetFloat("LSteer", 0);
        playerCharAni.SetFloat("RSteer", 0);
        playerCharAni.SetBool("IsBoosting", false);
        rigid.isKinematic = true;
    }


    #endregion

    #region [박스 캐스트 인스펙터 설정]
    [Header("공통 박스 캐스트 설정")]
    [SerializeField] private Vector3 boxCastCenter = Vector3.zero;   // 박스 캐스트 중심 오프셋

    [Header("전면/후면 박스 캐스트 설정")]
    [SerializeField] private float frontBackBoxCastDistance = 1f;        // 전면/후면 검사 거리
    [SerializeField] private Vector3 frontBackBoxCastSize = new Vector3(1, 1, 1); // 전면/후면 박스 크기 (모양)
    [SerializeField] private Color frontBackLineColor = Color.red;         // 전면/후면 선 색상
    [SerializeField] private Color frontBackBoxColor = Color.blue;         // 전면/후면 끝 박스 색상

    [Header("좌측/우측 박스 캐스트 설정")]
    [SerializeField] private float leftRightBoxCastDistance = 1f;          // 좌측/우측 검사 거리
    [SerializeField] private Vector3 leftRightBoxCastSize = new Vector3(0.5f, 1, 1.5f); // 좌측/우측 박스 크기 (모양)
    [SerializeField] private Color leftRightLineColor = Color.yellow;      // 좌측/우측 선 색상
    [SerializeField] private Color leftRightBoxColor = Color.magenta;

    [Header("지면 체크 설정")]
    [SerializeField] private float groundRayDistance = 0.8f;             // 지면 레이캐스트 거리
    [SerializeField] private float maxSlopeAngle = 10f;                  // 최소 경사각(보정 시작 기준)

    [Header("레이어 설정")]
    [SerializeField] private LayerMask wallLayer;     // 벽 레이어  
    [SerializeField] private LayerMask boosterLayer;  // 부스터 레이어
    [SerializeField] private LayerMask groundLayer;   // 지면 레이어

    // 충돌된 객체 정보를 저장할 변수들
    private RaycastHit lastHit;     // 박스 캐스트 충돌 결과
    private RaycastHit groundRayHit; // 지면 체크 Raycast 결과
    private RaycastHit slopeRayHit; // 정면(슬로프 체크) Raycast 결과    

    // 착지를 위한 추가 하강력 관련 변수들
    [SerializeField] private float landingForceMultiplier = 1.0f; // 속력에 따른 하강력 계수
    [SerializeField] private float minLandingForce = 10f;         // 최소 하강력
    [SerializeField] private float maxLandingForce = 50f;         // 최대 하강력



    #endregion
    #region [박스 캐스트 충돌 검사]

    /// <summary>
    /// 전면 및 후면 방향으로 박스 캐스트를 실행하여 충돌을 검사합니다.
    /// 충돌 시 마지막 결과를 lastHit에 저장하고 true를 반환합니다.
    /// </summary>
    private bool PerformFrontBackBoxCast(LayerMask layer)
    {
        Vector3 worldCenter = transform.position + transform.TransformDirection(boxCastCenter);
        float distance = frontBackBoxCastDistance;
        RaycastHit hit;

        // 전면 검사
        if (Physics.BoxCast(worldCenter, frontBackBoxCastSize * 0.5f, transform.forward, out hit, Quaternion.identity, distance, layer.value))
        {
            lastHit = hit;
            return true;
        }
        // 후면 검사
        if (Physics.BoxCast(worldCenter, frontBackBoxCastSize * 0.5f, -transform.forward, out hit, Quaternion.identity, distance, layer.value))
        {
            lastHit = hit;
            return true;
        }
        return false;
    }

    /// <summary>
    /// 좌측 및 우측 방향으로 박스 캐스트를 실행하여 충돌을 검사합니다.
    /// 충돌 시 마지막 결과를 lastHit에 저장하고 true를 반환합니다.
    /// </summary>
    private bool PerformLeftRightBoxCast(LayerMask layer)
    {
        Vector3 worldCenter = transform.position + transform.TransformDirection(boxCastCenter);
        float distance = leftRightBoxCastDistance;
        RaycastHit hit;

        // 우측 검사
        if (Physics.BoxCast(worldCenter, leftRightBoxCastSize * 0.5f, transform.right, out hit, Quaternion.identity, distance, layer.value))
        {
            lastHit = hit;
            return true;
        }
        // 좌측 검사
        if (Physics.BoxCast(worldCenter, leftRightBoxCastSize * 0.5f, -transform.right, out hit, Quaternion.identity, distance, layer.value))
        {
            lastHit = hit;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Inspector 설정에 따라 전면/후면과 좌측/우측 박스 캐스트 결과를 결합하여 충돌 여부를 검사합니다.
    /// 하나라도 충돌이 발생하면 true를 반환합니다.
    /// </summary>
    private bool PerformCombinedBoxCast(LayerMask layer)
    {
        return PerformFrontBackBoxCast(layer) || PerformLeftRightBoxCast(layer);
    }



    /// <summary>
    /// 박스 캐스트 결과에 따라 충돌된 레이어에 따른 처리를 호출합니다.
    /// </summary>
    private void HandleLayerCollision()
    {
        if (lastHit.collider != null)
        {
            int hitLayer = lastHit.collider.gameObject.layer;

            if (((1 << hitLayer) & wallLayer.value) != 0)
            {
                ProcessWallCollision();
            }
            else if (((1 << hitLayer) & boosterLayer.value) != 0)
            {
                ProcessBoosterCollision();
            }
            else if (((1 << hitLayer) & groundLayer.value) != 0)
            {
                ProcessGroundCollision();
            }
            else
            {
                // 기타 레이어 처리
            }
        }
    }

    /// <summary>
    /// 벽과 충돌 시 반사 효과를 적용하며, 드리프트 중이면 드리프트를 종료하고,
    /// 충돌 후 차량이 벽 안으로 박히는 현상을 막기 위해 약간의 밀쳐내기(Separation) 처리를 합니다.
    /// </summary>
    private void ProcessWallCollision()
    {
        if (lastHit.collider != null)
        {
            // 드리프트 중이면 드리프트 종료 처리
            if (isDrifting)
            {
                EndDrift();
            }

            // 예: 아이템 박스 처리
            if (lastHit.collider.CompareTag("ItemBox"))
            {
                lastHit.collider.gameObject.GetComponent<BarricadeController>().OffBarricade();
            }

            // 충돌 전 손실된 속도를 반사 처리: 현재 속도를 반사 및 감쇠
            Vector3 incomingVelocity = rigid.velocity;
            Vector3 reflectedVelocity = Vector3.Reflect(incomingVelocity, lastHit.normal);
            float bounceFactor = 0.2f; // 감쇠 계수
            Vector3 newVelocity = reflectedVelocity * bounceFactor;

            // 벽에 너무 박히지 않도록, 충돌면의 법선 방향으로 살짝 밀어냅니다.
            // 이 거리 값은 실험을 통해 적절한 수치(예: 0.1f ~ 0.2f 등)로 조정하면 됩니다.
            float separationDistance = 0.1f;
            transform.position += lastHit.normal * separationDistance;

            rigid.velocity = newVelocity;
            Debug.Log($"벽 충돌 후 처리: 새 속도 = {rigid.velocity}");

            kartBodyCtrl.SetCollisonSparkActive(true);
        }
    }


    /// <summary>
    /// 부스터 레이어 충돌 시 부스트를 실행합니다.
    /// </summary>
    private void ProcessBoosterCollision()
    {
        StartBoost(boostDuration); // boostDuration 예시
    }

    /// <summary>
    /// 지면과 충돌 시 수평 속도를 유지하고, 경사면일 경우 보정 처리를 실행합니다.
    /// (이 함수는 박스 캐스트로부터 충돌 정보를 사용)
    /// </summary>
    private void ProcessGroundCollision()
    {
        // 우선 수평 속도 유지
        Vector3 horizontalVelocity = new Vector3(rigid.velocity.x, 0f, rigid.velocity.z);
        rigid.velocity = new Vector3(horizontalVelocity.x, rigid.velocity.y, horizontalVelocity.z);
    }

    #endregion

    #region [정면(슬로프 체크) 캐스트 및 경사 보정 힘 적용]

    /// <summary>
    /// 정면으로 Raycast를 쏘아 지면의 경사를 체크하고, 경사각에 따라 추가 힘을 적용합니다.
    /// </summary>
    private void ProcessSlopeForce()
    {
        // 정면으로 슬로프 체크를 위한 오리진. 예: 플레이어 전방 약간 앞으로 이동한 위치
        Vector3 origin = transform.position + transform.forward * 1.0f; // 1단위 만큼 앞으로
        // 정면 + 약간 아래로 쏘기 (지면 체크에 적합하도록)
        Vector3 direction = (transform.forward + Vector3.down * 0.5f).normalized;
        float rayDistance = 2.0f; // 슬로프 체크용 거리

        if (Physics.Raycast(origin, direction, out slopeRayHit, rayDistance, groundLayer))
        {
            float slopeAngle = Vector3.Angle(slopeRayHit.normal, Vector3.up);
            // 예: 경사각이 클수록 (혹은 특정 임계치 이상) 추가 힘을 부여
            float forceMultiplier = Mathf.Lerp(1.0f, 2.0f, Mathf.InverseLerp(0f, 45f, slopeAngle));
            //Debug.Log($"슬로프 체크: 경사각 {slopeAngle:F2}°, 힘 배율 {forceMultiplier:F2}");

            // 예: 경사면에서 속도를 보완하기 위해 추가 전진 힘을 가한다. (단, 조건에 맞게 조정)
            rigid.AddForce(transform.forward * forceMultiplier * 1.2f, ForceMode.Acceleration);
        }
    }
    #endregion

    #region [지면 체크]
    /// <summary>
    /// 아래 방향으로 Raycast를 실행해 지면 충돌 여부를 확인합니다.
    /// 결과는 groundRayHit에 저장됩니다.
    /// </summary>
    private bool CheckIfGrounded()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out groundRayHit, groundRayDistance, groundLayer))
        {
            rigid.angularDrag = 50f;
            return true;
        }
        return false;
    }
    /// <summary>
    /// 공중 상태일 때(currently not grounded) 현재 수평 속력에 비례하여 추가 하강력을 적용합니다.
    /// 이를 통해 높은 속력에서 공중에 뜬 차량이 더 빠르게 착지하도록 합니다.
    /// </summary>
    private void ApplyAirborneLandingForce()
    {
        // 공중 상태인지 검사
        if (!CheckIfGrounded())
        {
            // 수평 속도 계산 (Y축 제외)
            Vector3 horizontalVelocity = new Vector3(rigid.velocity.x, 0f, rigid.velocity.z);
            float currentSpeed = horizontalVelocity.magnitude;

            // 현재 속력에 비례한 추가 하강력을 계산
            float extraDownForce = currentSpeed * landingForceMultiplier;
            // 최소, 최대 한계를 적용
            extraDownForce = Mathf.Clamp(extraDownForce, minLandingForce, maxLandingForce);

            // 추가 하강력을 아래쪽으로 적용 (ForceMode.Acceleration은 질량 무관한 가속도 적용)
            rigid.AddForce(Vector3.down * extraDownForce, ForceMode.Acceleration);
        }
    }
    /// <summary>
    /// 착지 직후에 수평 속도를 보존하면서 수직 속도를 정리합니다.
    /// </summary>
    private void ProcessLanding()
    {
        // 현재 속도에서 수평 성분을 별도로 분리 (Y축은 제외)
        Vector3 horizontalVelocity = new Vector3(rigid.velocity.x, 0f, rigid.velocity.z);

        // 착지 시, 수직 속도는 0으로 설정하여 운동 에너지 손실 없이 수평 속도를 유지
        rigid.velocity = new Vector3(horizontalVelocity.x, 0f, horizontalVelocity.z);
    }
    #endregion
    //기즈모 그리는거
        #region [Gizmos 시각화: 고정된 방향(월드 기준)으로 표시]
        private void OnDrawGizmos()
        {
            // Gizmo에서 기본 시작점은 차량의 현재 위치에 boxCastCenter 오프셋을 적용한 위치입니다.
            Vector3 worldCenter = transform.position + boxCastCenter;

            // 기본 중심 박스 (녹색) - 선택 사항: 두 그룹의 박스 크기가 다르므로 기준을 표시
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(worldCenter, frontBackBoxCastSize);

            // 전면/후면 그룹
            Gizmos.color = frontBackLineColor;
            Vector3 frontEnd = worldCenter + Vector3.forward * frontBackBoxCastDistance;
            Gizmos.DrawLine(worldCenter, frontEnd);
            Gizmos.color = frontBackBoxColor;
            Gizmos.DrawWireCube(frontEnd, frontBackBoxCastSize);

            Gizmos.color = frontBackLineColor;
            Vector3 backEnd = worldCenter + Vector3.back * frontBackBoxCastDistance;
            Gizmos.DrawLine(worldCenter, backEnd);
            Gizmos.color = frontBackBoxColor;
            Gizmos.DrawWireCube(backEnd, frontBackBoxCastSize);

            // 좌측/우측 그룹
            Gizmos.color = leftRightLineColor;
            Vector3 rightEnd = worldCenter + Vector3.right * leftRightBoxCastDistance;
            Gizmos.DrawLine(worldCenter, rightEnd);
            Gizmos.color = leftRightBoxColor;
            Gizmos.DrawWireCube(rightEnd, leftRightBoxCastSize);

            Gizmos.color = leftRightLineColor;
            Vector3 leftEnd = worldCenter + Vector3.left * leftRightBoxCastDistance;
            Gizmos.DrawLine(worldCenter, leftEnd);
            Gizmos.color = leftRightBoxColor;
            Gizmos.DrawWireCube(leftEnd, leftRightBoxCastSize);
        }
        #endregion

    //*/

}