using System.Collections;
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
    [SerializeField] private float decelerationRate = 2f;  // 키 없음 감속 보정 계수
    [SerializeField] private float minSteerMultiplier = 1f;  // 최소 조향 배수 (속도가 낮을 때)
    [SerializeField] private float maxSteerMultiplier = 2f;  // 최대 조향 배수 (속도가 최대일 때)

    [Header("드리프트 설정")]
    [SerializeField] private float minDriftAngle = 30f;       // 최소 드리프트 각도
    [SerializeField] public float maxDriftAngle = 180f;       // 최대 드리프트 각도
    [SerializeField] private float minDriftDuration = 0.2f;     // 최소 드리프트 지속시간
    [SerializeField] private float maxDriftDuration = 2f;       // 최대판단 드리프트 지속시간
    [SerializeField] private float totalMaxDriftDuration = 2f;       // 최대판단 드리프트 지속시간
    [SerializeField] private float minDriftForceMultiplier = 1f;// 최소 드리프트 힘 배수
    [SerializeField] private float maxDriftForceMultiplier = 5f;// 최대 드리프트 힘 배수
    [SerializeField] private float driftSpeedReduction = 0.7f;  // 드리프트 시 속도 감소 비율
    [SerializeField] private float driftRecoverySpeed = 2f;  // 드리프트 복구 속도 제어    
    [SerializeField] private float curveDriftSpeed = 5f;  // 드리프트 전환 속도 제어

    [Header("부스트 설정")]
    [SerializeField] public float boostDuration = 2f;         // 기본 부스트 지속시간
    [SerializeField] public float momentboostDuration = 0.2f;  // 순간 부스트 지속시간
    [SerializeField] public int maxBoostGauge = 100;           // 최대 부스트 게이지
    [SerializeField] private float boostChargeRate = 5f;        // 기본 부스트 충전 속도
    [SerializeField] private float driftBoostChargeRate = 10f;   // 드리프트 중 부스트 충전 속도
    [SerializeField] private float boostMaxSpeedKmh = 280f;        // 부스트 상태의 최대 속도


    private float boostSpeed;         // 부스트 활성화 시 속도 

    #endregion

    #region Private Fields

    public Animator playerCharAni { get; set; }       
    public float speedKM { get; private set; }     // 현재 속력 (km/h 단위)
    public bool isBoostTriggered { get; set; } // 부스트 활성화 여부
    //public bool isBoostCreate { get; set; }    // 드리프트 아이템 생성 가능 여부
    public float boostGauge { get; private set; }                // 현재 부스트 게이지
    public bool isItemUsed { get; set; }
    public bool isRacingStart { get; set; }

    private float driftDuration;  // 부스터 지속 시간
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
    //public int boostCount { get; private set; }
    // 내부적으로 사용할 m/s 단위 변수
    private float maxSpeed;      // 최대 속도 (m/s)
    private Vector3 speed;       // 현재 속도 벡터
    private float chargeAmount;
    

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

        rigid = GetComponent<Rigidbody>();                         // 리지드바디 참조
        ///charAni = GetComponent<Animator>();

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

        // 최대 속도 계산
        maxSpeed = maxSpeedKmh / 3.6f; // 일반 최대 속도
                                       //boostMaxSpeed = boostMaxSpeedKmh / 3.6f; // 부스트 최대 속도
                                       //currentMaxSpeed = isBoostTriggered ? boostMaxSpeed : maxSpeed;

        

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

        // 중력 강화 처리
        ApplyEnhancedGravity();
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

        // 입력값 읽어오기
        currentSteerInput = Input.GetAxis("Horizontal");
        currentMotorInput = Input.GetAxis("Vertical");
        // 이동 처리
        HandleKartMovement(currentMotorInput, currentSteerInput);
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
        if (PerformBoxCast(groundLayer | wallLayer | jumpLayer | boosterLayer))

        {
            HandleLayerCollision(); // 충돌된 레이어 처리
        }

        // 레이캐스트로 지면 체크
        if (CheckIfGrounded())
        {
            //Debug.Log("현재 지면 위에 있습니다.");
        }
        else
        {
            //Debug.Log("현재 공중 상태입니다.");
        }

        if(isUsingShield == false)
        {
            if(kartBodyCtrl.shield.activeSelf == true)
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

        if(isSparkOn == true)
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

        if(isSparkOn == true)
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

        if (!boosted)
        {
            //Debug.Log("즉시 부스트 입력 시간 초과");
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

        // 가속 소리 설정
        PlayBoostEffectSound();
        StartCoroutine(InstantBoostCoroutine()); // 순간 부스터 실행
    }

    private IEnumerator InstantBoostCoroutine()
    {
        float boostDuration = momentboostDuration; // 부스터 지속 시간
        float boostMultiplier = 1.2f; // 현재 속력의 1.2배로 밀기
        float timer = 0f;

        // [Phase 1] 부스터 가속 단계
        while (timer < boostDuration)
        {
            timer += Time.fixedDeltaTime; // FixedUpdate 주기에 맞춰 타이머 업데이트

            // 현재 속도에 비례한 추가 힘 계산
            float currentSpeed = rigid.velocity.magnitude;
            Vector3 boostForce = transform.forward * currentSpeed * boostMultiplier;

            // 추가 힘을 차량의 전방 방향으로 적용
            rigid.AddForce(boostForce, ForceMode.Acceleration);

            yield return new WaitForFixedUpdate(); // FixedUpdate와 동기화
        }

        EndBoost(); // 부스터 종료 처리
                    // Debug.Log("순간 부스터 종료!");
    }
    private void EndBoost()
    {
        if (!isBoostTriggered) return; // 부스트가 이미 종료된 경우 무시

        //Debug.Log("부스트 종료: 속도 서서히 감소 시작");
        isBoostTriggered = false; // 부스트 상태 비활성화
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
        if (boostGauge >= maxBoostGauge)
        {
            //Debug.Log("부스트 게이지 최대치 도달!");
        }
    }
    /// <summary>
    /// 부스트를 시작하는 함수. 코루틴을 호출해 현재 속도에서 최대 부스트 속도까지 점진적 가속을 구현합니다.
    /// </summary>
    /// <param name="duration">부스트 지속 시간(초)</param>
    private void StartBoost(float duration)
    {
        if (isBoostTriggered) return; // 이미 부스트 중이면 무시

        //Debug.Log("기본 부스트 활성화!");
        isBoostTriggered = true;
        // 램프 TrilRenderer 실행
        kartBodyCtrl.SetLampTrailActive(true);
        kartBodyCtrl.SetBoostEffectActive(true);
        kartBodyCtrl.SetBoostWindEffectActive(true);

        PlayBoostEffectSound();

        StartCoroutine(BoostCoroutine(duration)); // 기본 부스터 실행
    }

    private IEnumerator BoostCoroutine(float duration)
    {
        float boostMultiplier = 1.2f; // 현재 속력의 1.2배로 밀기
        float timer = 0f;

        // [Phase 1] 부스터 가속 단계
        while (timer < duration)
        {
            timer += Time.fixedDeltaTime;  // FixedDeltaTime 사용

            // 현재 속도에 비례한 추가 힘 계산
            float currentSpeed = rigid.velocity.magnitude;
            Vector3 boostForce = transform.forward * currentSpeed * boostMultiplier;

            // 추가 힘을 차량의 전방 방향으로 적용
            rigid.AddForce(boostForce, ForceMode.Acceleration);

            yield return new WaitForFixedUpdate();  // FixedUpdate와 동기화
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
        // 드리프트 중인지 아닌지에 따라 별도 처리
        if (isDrifting)
        {
            ProcessDrift(steerInput, steeringMultiplier);
        }
        else
        {
            initialDriftSpeed = 0f;
            ProcessAcceleration(motorInput, maxSpeed);
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
        lockedYRotation += steerInput * ((steerAngle / 2.5f) * steeringMultiplier) * Time.deltaTime;
        Quaternion driftRotation = Quaternion.Euler(0f, lockedYRotation, 0f);
        Vector3 driftDirection = driftRotation * Vector3.forward;

        // 현재 속도를 부드럽게 드리프트 방향으로 전환합니다.
        rigid.velocity = Vector3.Lerp(rigid.velocity, driftDirection * driftSpeed, Time.deltaTime * 10f);

        // 측면 힘을 추가하여 드리프트 느낌을 강화합니다.
        Vector3 lateralForce = transform.right * steerInput * driftForceMultiplier * movementForce;
        rigid.AddForce(lateralForce, ForceMode.Force);
    }

    [SerializeField] private AnimationCurve accelerationCurve; // 에디터에서 가속 곡선 설정
    private float accelerationTime = 0f; // 누적 가속 시간을 추적
    private float currentAcceleration = 0f; // 현재 가속도 (매 프레임 누적)

    // 최대 가속도 제한
    [SerializeField] private float maxAcceleration; // 최대 가속도 (수정 가능)

    /// <summary>
    /// 전진/후진 가속 처리 (누적 가속도 기반으로 점진적 증가)
    /// </summary>
    private void ProcessAcceleration(float motorInput, float currentMaxSpeed)
    {
        // 가속 시간 누적: 입력이 있는 경우만 증가
        if (Mathf.Abs(motorInput) > 0.1f)
        {
            accelerationTime += Time.fixedDeltaTime; // 누적 시간 증가
                                                     // AnimationCurve로 가속 배율 계산
            float accelerationMultiplier = accelerationCurve.Evaluate(accelerationTime);

            // 가속도가 최대 가속도를 초과하지 않도록 제한
            currentAcceleration = Mathf.Clamp(accelerationMultiplier * maxAcceleration, 0f, maxAcceleration);
        }
        else
        {
            // 입력이 없을 경우 가속도와 시간 초기화
            accelerationTime = 0f;
            currentAcceleration = 0f;
        }

        // 목표 속도 계산
        Vector3 targetVelocity = transform.forward * motorInput * currentMaxSpeed;

        // 점진적으로 가속도 기반으로 속도 증가
        Vector3 smoothedVelocity = Vector3.Lerp(new Vector3(rigid.velocity.x, 0f, rigid.velocity.z), targetVelocity * currentAcceleration, Time.fixedDeltaTime * 0.25f);

        // Y축 속도는 유지
        smoothedVelocity.y = rigid.velocity.y;

        // 최종적으로 Rigidbody에 속도 적용
        rigid.velocity = smoothedVelocity;
    }

    /// <summary>
    /// 조향 입력과 속도 기반 민감도를 적용해 회전 처리를 담당합니다.
    /// </summary>
    // 조향을 부드럽게 처리하는 변수
    private float currentSteerAngle = 0f;

    private void RotateKart(float steerInput, float steeringMultiplier)
    {
        if (speedKM > 0.1f)
        {
            // 드리프트 상태에서 다른 각도 제한 적용
            float maxSteerAngle = isDrifting ? 360f : 90f;  // 드리프트 중에는 60도, 기본은 90도

            // 목표 각도 계산
            float targetSteerAngle = steerInput * steerAngle * steeringMultiplier;

            // 부드러운 보간으로 현재 각도를 목표 각도로 점진적으로 변경
            currentSteerAngle = Mathf.Lerp(currentSteerAngle, targetSteerAngle, Time.deltaTime * 30f);

            // 각도를 -maxSteerAngle에서 +maxSteerAngle로 제한
            currentSteerAngle = Mathf.Clamp(currentSteerAngle, -maxSteerAngle, maxSteerAngle);

            // 회전 처리
            Vector3 turnDirection = Quaternion.Euler(0, currentSteerAngle * Time.deltaTime, 0) * transform.forward;
            rigid.MoveRotation(Quaternion.LookRotation(turnDirection));
        }
    }
    private void UpdateAnimator(float steerInput)
    {
        if (playerCharAni == null)
        {
            Debug.LogError("charAni가 null입니다! Animator가 제대로 연결되지 않았는지 확인하세요.");
            return; // 더 이상 진행하지 않음
        }

        if (playerCharAni != null)
        {
            // 조향 값을 기반으로 LSteer 및 RSteer 업데이트
            float LSteer = Mathf.Clamp01(-steerInput); // 왼쪽 조향
            float RSteer = Mathf.Clamp01(steerInput);  // 오른쪽 조향
            
            if (LSteer <0.3f && RSteer <0.3f)
            {
                LSteer = 0;
                RSteer = 0;
            }
            playerCharAni.SetFloat("LSteer", LSteer);
            playerCharAni.SetFloat("RSteer", RSteer);           
                   
        }      

    }

    #endregion

    #region [박스 캐스트 인스펙터 설정]

    [Header("박스 캐스트 설정")]
    [SerializeField] private Vector3 boxCastCenter = Vector3.zero;     // 박스 캐스트 중심 오프셋
    [SerializeField] private Vector3 boxCastSize = new Vector3(1, 1, 1); // 박스 크기
    [SerializeField] private float boxCastDistance = 1f;                 // 박스 캐스트 거리
    [SerializeField] private float groundRayDistance = 0.8f;             // 지면 레이캐스트 거리

    [Header("레이어 설정")]
    [SerializeField] private LayerMask wallLayer;     // 벽 레이어
    [SerializeField] private LayerMask jumpLayer;     // 점프 레이어
    [SerializeField] private LayerMask boosterLayer;  // 부스터 레이어
    [SerializeField] private LayerMask groundLayer;   // 지면 레이어

    // 충돌된 객체 정보를 저장할 변수들
    private RaycastHit lastHit;     // 박스 캐스트 충돌 결과
    private RaycastHit groundRayHit; // 아래로 쏘는 레이캐스트 결과

    /// <summary>
    /// 박스 캐스트로 충돌 여부를 확인합니다.
    /// </summary>
    private bool PerformBoxCast(LayerMask layer)
    {
        Vector3 worldCenter = transform.position + transform.TransformDirection(boxCastCenter);
        return Physics.BoxCast(worldCenter, boxCastSize * 0.5f, transform.forward, out lastHit, Quaternion.identity, boxCastDistance, layer.value);
    }

    /// <summary>
    /// 충돌된 레이어를 판별하고 각 상황에 맞는 처리를 호출합니다.
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
            else if (((1 << hitLayer) & jumpLayer.value) != 0)
            {
                ProcessJumpCollision();
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
                // 정의되지 않은 레이어 처리 (필요 시 추가)
            }
        }
    }

    /// <summary>
    /// 벽과 충돌 시 반사 효과를 적용합니다.
    /// </summary>
    private void ProcessWallCollision()
    {
        if (lastHit.collider != null)
        {
            if(lastHit.collider.CompareTag("ItemBox"))
            {
                lastHit.collider.gameObject.GetComponent<BarricadeController>().OffBarricade();
            }
            // 현재 속도를 가져온 후, lastHit.normal을 기준으로 반사
            Vector3 incomingVelocity = rigid.velocity;
            Vector3 reflectedVelocity = Vector3.Reflect(incomingVelocity, lastHit.normal);
            float bounceFactor = 0.5f; // 감쇠 계수

            rigid.velocity = reflectedVelocity * bounceFactor;
            Debug.Log($"벽 충돌 처리: 반사된 속도 = {rigid.velocity}");

            kartBodyCtrl.SetCollisonSparkActive(true);
        }
    }

    /// <summary>
    /// 점프 레이어와 충돌 시, 경사면 기울기가 10도 이상이면 현재 수평 속력을 1.2배 보정합니다.
    /// </summary>
    private void ProcessJumpCollision()
    {
        if (lastHit.collider == null) return;

        float slopeAngle = Vector3.Angle(lastHit.normal, Vector3.up);
        if (slopeAngle >= 10f)
        {
            Vector3 horizontalVelocity = new Vector3(rigid.velocity.x, 0f, rigid.velocity.z) * 1.2f;
            rigid.velocity = new Vector3(horizontalVelocity.x, rigid.velocity.y, horizontalVelocity.z);
            //Debug.Log($"ProcessJumpCollision: 경사각 {slopeAngle:F2}° 보정됨. (1.2배)");
        }
        else
        {
            //Debug.Log($"ProcessJumpCollision: 경사각 {slopeAngle:F2}° (보정 없음)");
        }
    }

    /// <summary>
    /// 부스터 레이어와 충돌 시 기본 부스터 기능을 호출합니다.
    /// </summary>
    private void ProcessBoosterCollision()
    {
        StartBoost(boostDuration);
        // Debug.Log("부스터 충돌 처리 실행됨!");
    }

    /// <summary>
    /// 지면과 충돌 시 수평 속도를 그대로 유지합니다.
    /// </summary>
    private void ProcessGroundCollision()
    {
        Vector3 horizontalVelocity = new Vector3(rigid.velocity.x, 0f, rigid.velocity.z);
        rigid.velocity = horizontalVelocity;
        // Debug.Log("ProcessGroundCollision: 지면 충돌 시 수평 속력 유지");

        float slopeAngle = Vector3.Angle(lastHit.normal, Vector3.up);
        if (slopeAngle >= 10f)
        {
            Vector3 horizontalVelocityGround = new Vector3(rigid.velocity.x, 0f, rigid.velocity.z) * 1.2f;
            rigid.velocity = new Vector3(horizontalVelocityGround.x, rigid.velocity.y, horizontalVelocityGround.z);
            //Debug.Log($"ProcessJumpCollision: 경사각 {slopeAngle:F2}° 보정됨. (1.2배)");
        }
    }

    /// <summary>
    /// 공중 상태에서 추가 중력을 적용합니다 (지면에 닿기 전 강화).
    /// </summary>
    private void ApplyEnhancedGravity()
    {
        if (!CheckIfGrounded())
        {
            float currentHorizontalSpeed = new Vector3(rigid.velocity.x, 0f, rigid.velocity.z).magnitude;
            float speedFactor = Mathf.Clamp01(currentHorizontalSpeed / 50f); // 조정 가능
            float gravityMultiplier = Mathf.Lerp(1f, 5f, speedFactor);
            Vector3 enhancedGravity = Physics.gravity * gravityMultiplier;

            rigid.AddForce(enhancedGravity, ForceMode.Acceleration);
            // Debug.Log($"강화된 중력 적용됨: {enhancedGravity} (속도: {currentHorizontalSpeed})");
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
            return true;
        }
        return false;
    }

    #endregion

    #region [Gizmos: 박스 캐스트 시각화]

    private void OnDrawGizmos()
    {
        // 박스 캐스트 중심을 월드 좌표로 변환
        Vector3 worldCenter = transform.position + transform.TransformDirection(boxCastCenter);

        // 시작 위치의 박스 (녹색)
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(worldCenter, boxCastSize);

        // 캐스트 방향과 끝 위치 표시 (빨간 선 및 파란 박스)
        Gizmos.color = Color.red;
        Vector3 endCenter = worldCenter + transform.forward * boxCastDistance;
        Gizmos.DrawLine(worldCenter, endCenter);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(endCenter, boxCastSize);
    }

    #endregion
}