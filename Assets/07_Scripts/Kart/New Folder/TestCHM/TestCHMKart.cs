using System.Collections;
using Photon.Pun;
using UnityEngine;

public class TestCHMKart : MonoBehaviour
{
    #region Serialized Fields

    [Header("카트 구성 요소")]
    [SerializeField] private GameObject wheels;   // 바퀴 오브젝트

    [Header("이동 설정")]
    [SerializeField] private float maxSpeedKmh = 200f;           // 최대 속도
    [SerializeField] private float movementForce = 20f;       // 이동에 적용할 힘
    [SerializeField] private float steerAngle = 200f;         // 조향(회전) 각도
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
    [SerializeField] public float boostDuration = 1.2f;         // 부스트 지속시간
    [SerializeField] public int maxBoostGauge = 100;           // 최대 부스트 게이지
    [SerializeField] private float boostChargeRate = 5f;        // 기본 부스트 충전 속도
    [SerializeField] private float driftBoostChargeRate = 10f;   // 드리프트 중 부스트 충전 속도
    [SerializeField] private float boostMaxSpeedKmh = 280f;        // 부스트 상태의 최대 속도
                     private float boostSpeed;         // 부스트 활성화 시 속도

    [Header("기타 설정")]
    [SerializeField] private float rotationCorrectionSpeed = 5f;// 회전 보정 속도
    [SerializeField] private float downwardForce = 50f;         // 하강(중력 보조) 힘
    [SerializeField] private float wallBounceFactor = 2f;  // 벽 충돌 시 적용할 튕김 힘(조정 가능)

    #endregion

    #region Private Fields

    private CHMTestWheelController wheelCtrl;  // 바퀴 제어 스크립트
    private Rigidbody rigid;                     // 리지드바디 (물리 처리)
    public float speedKM { get; private set; }     // 현재 속력 (km/h 단위)
    public bool isBoostTriggered { get; private set; } // 부스트 활성화 여부
    private float driftDuration;  // 드리프트 진행 누적시간
    public bool isBoostCreate { get; set; }            // 드리프트 아이템 생성 가능 여부

    private Coroutine postDriftBoostCoroutine; // 드리프트 종료 후 부스트 처리를 위한 코루틴 변수
    private float initialDriftSpeed;           // 드리프트 시작 시 기록된 초기 속도
    public bool isDrifting = false;            // 드리프트 진행 중 여부
    public float currentDriftAngle = 0f;       // 현재 누적 드리프트 각도
    private float currentDriftThreshold;       // 속도에 따른 드리프트 입력 기준 값
    private float driftForceMultiplier;        // 동적으로 계산된 드리프트 힘 배수
    public float boostGauge { get; private set; }                // 현재 부스트 게이지
    private float lockedYRotation = 0f;        // 드리프트 시 고정되는 Y 회전값
    private float currentMotorInput;
    private float currentSteerInput;
    // 내부적으로 사용할 m/s 단위 변수
    private float maxSpeed;      // 최대 속도 (m/s)
    private float boostMaxSpeed; // 부스트 최대 속도 (m/s)



    private Vector3 speed;                     // 현재 속도 벡터
    private float chargeAmount;

    #endregion

    #region
    /* Network Instantiate */
    private Transform _playerParent;
    private Transform _tr;
    private PhotonView _photonView;    
    #endregion
    
    #region Unity Methods

    private void Awake()
    {
        wheelCtrl = wheels.GetComponent<CHMTestWheelController>(); // 바퀴 컨트롤러 참조
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
        float currentMaxSpeed = isBoostTriggered ? boostMaxSpeed : maxSpeed;
        HandleKartMovement(currentMotorInput, currentSteerInput); // 입력은 Update()에서 저장 후 사용
                                                                  // 그리고 마지막에 항상 속도 클램핑을 적용합니다.
        ClampHorizontalSpeed(currentMaxSpeed);
        // 기존 이동 처리 후 공중 회전 보정
        CorrectAirborneRotation();
    }
    

    private void Update()
    {
        if (!_photonView.IsMine)
        {
            return;
        }        
        
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

        // 드리프트 중 지속시간 누적 및 조건 체크
        //if (isDrifting)
        //{
        //    driftDuration += Time.deltaTime;
        //    if (driftDuration >= 1f)
        //    {
        //        isBoostCreate = true;
        //        driftDuration = 0f;
        //    }
        //}

        // 부스트 게이지 충전
        if(currentMotorInput != 0 || isDrifting)
        {
            ChargeBoostGauge();
        }

        if(boostGauge >= maxBoostGauge)
        {
            isBoostCreate = true;
            boostGauge = 0;
            chargeAmount = 0;
        }

        // 카트 이동 처리 (이동/회전)
        //HandleKartMovement(currentMotorInput, currentSteerInput);
    }
    #endregion

    #region Input Handling

    private void HandleDriftInput(float steerInput)
    {
        // LeftShift 키와 조향 입력이 있을 때 드리프트 시작
        if (Input.GetKeyDown(KeyCode.LeftShift) && Mathf.Abs(steerInput) > 0)
        {
            StartDrift(steerInput * currentDriftThreshold);
        }

        // 드리프트 중 추가 입력으로 드리프트 각도 업데이트
        if (isDrifting && Input.GetKey(KeyCode.LeftShift))
        {
            UpdateDriftAngle();
        }
    }

    private void HandleBoostInput()
    {
        // LeftControl 키와 부스트 게이지 최대치 시 기본 부스트 활성화
        if (Input.GetKeyDown(KeyCode.LeftControl) && boostGauge >= maxBoostGauge)
        {
            StartBoost(boostDuration);
        }
    }

    #endregion

    #region Drift and Boost Methods

    private void UpdateDriftParameters()
    {
        float currentSpeed = rigid.velocity.magnitude;
        float speedFactor = Mathf.Clamp01(currentSpeed / maxSpeed * 2f);

        // 속도에 따라 드리프트 입력 민감도와 힘 배수 업데이트
        currentDriftThreshold = Mathf.Lerp(minDriftAngle, maxDriftAngle, speedFactor);
        driftForceMultiplier = Mathf.Lerp(minDriftForceMultiplier, maxDriftForceMultiplier, speedFactor);
    }

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

    private void UpdateDriftAngle()
    {
        currentDriftAngle += Time.deltaTime * 10f;
        currentDriftAngle = Mathf.Clamp(currentDriftAngle, -maxDriftAngle, maxDriftAngle);
    }

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

    private IEnumerator PostDriftBoostCoroutine()
    {
        float timer = 0f;
        bool boosted = false;
        Debug.Log("즉시 부스트 입력 대기 중...");

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
        {
            Debug.Log("즉시 부스트 입력 시간 초과");
        }

        postDriftBoostCoroutine = null;
    }

    private void PerformInstantBoost()
    {
        Debug.Log("즉시 부스트 활성화!");
        rigid.velocity *= 1.2f;  // 현재 속력의 1.2배 증폭
        postDriftBoostCoroutine = null;
    }

    private void StartBoost(float duration)
    {
        boostSpeed = speedKM;
        isBoostTriggered = true;
        boostGauge = 0;
        rigid.velocity = transform.forward * boostSpeed;
        Debug.Log("부스트 활성화");
        Invoke(nameof(EndBoost), duration);
    }

    private void EndBoost()
    {
        isBoostTriggered = false;
        Debug.Log("부스트 종료");
    }

    private void ChargeBoostGauge()
    {
        chargeAmount += isDrifting
            ? driftBoostChargeRate * Time.fixedDeltaTime  // 드리프트 중 충전 속도
            : boostChargeRate * Time.fixedDeltaTime;       // 일반 충전 속도

        boostGauge = Mathf.Clamp(chargeAmount, 0, maxBoostGauge);
        Debug.Log("boostGage : " + boostGauge);

        if (boostGauge >= maxBoostGauge)
        {
            Debug.Log("부스트 게이지 최대치 도달!");
        }
    }

    #endregion
    #region Movement Handling

    private void HandleKartMovement(float motorInput, float steerInput)
    {
        // 부스터 적용 여부에 따라 최대 속도(m/s)를 설정합니다.
        float currentMaxSpeed = isBoostTriggered ? boostMaxSpeed : maxSpeed;

        // 현재 수평 속도를 측정하고, 최대속도 대비 비율(0~1)을 계산합니다.
        Vector3 currentHorizontalVelocity = new Vector3(rigid.velocity.x, 0f, rigid.velocity.z);
        float currentHorizontalSpeed = currentHorizontalVelocity.magnitude;
        float speedFactor = Mathf.Clamp01(currentHorizontalSpeed / currentMaxSpeed);

        // 속도에 따라 조향 민감도를 선형 보간합니다.
        float steeringMultiplier = Mathf.Lerp(minSteerMultiplier, maxSteerMultiplier, speedFactor);

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

        // 최종적으로 수평 속도를 currentMaxSpeed 이하로 제한합니다.
        ClampHorizontalSpeed(currentMaxSpeed);

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
    /// 전진/후진 가속 처리 (드리프트가 아닐 때)
    /// </summary>
    private void ProcessAcceleration(float motorInput, float currentMaxSpeed)
    {
        Vector3 acceleration = transform.forward * movementForce * motorInput * Time.fixedDeltaTime;
        Vector3 currentHorizVelocity = new Vector3(rigid.velocity.x, 0f, rigid.velocity.z);
        Vector3 newHorizVelocity = currentHorizVelocity + acceleration;

        // 누적된 속도를 최대 속도로 클램프합니다.
        newHorizVelocity = Vector3.ClampMagnitude(newHorizVelocity, currentMaxSpeed);

        // 입력이 있는 경우, 속도 벡터를 즉시 transform.forward 방향으로 정렬합니다.
        if (Mathf.Abs(motorInput) > 0.1f)
        {
            newHorizVelocity = transform.forward * newHorizVelocity.magnitude;
        }

        // 수직 속도(Y)는 그대로 두고, XZ 성분만 적용합니다.
        rigid.velocity = new Vector3(newHorizVelocity.x, rigid.velocity.y, newHorizVelocity.z);

        // 보정된 회전값 저장 (필요 시)
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

    /// <summary>
    /// XZ 평면상의 수평 속도를 최대값 이하로 제한합니다.
    /// </summary>
    private void ClampHorizontalSpeed(float maxHorizontalSpeed)
    {
        Vector3 horizontalVelocity = new Vector3(rigid.velocity.x, 0f, rigid.velocity.z);
        if (horizontalVelocity.magnitude > maxHorizontalSpeed)
        {
            horizontalVelocity = horizontalVelocity.normalized * maxHorizontalSpeed;
            rigid.velocity = new Vector3(horizontalVelocity.x, rigid.velocity.y, horizontalVelocity.z);
        }
    }

    #endregion


    #region 안티롤 & 충돌 처리
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

    public bool IsGrounded()
    {
        // 간단한 Raycast를 사용하여 kart의 하단이 지면에 닿았는지 확인합니다.
        float rayDistance = 0.8f; // 필요에 따라 조정 가능
        return Physics.Raycast(transform.position, Vector3.down, rayDistance);
    }
    private void OnCollisionEnter(Collision collision)
    {
        // 충돌한 오브젝트가 "Wall" 태그인 경우
        if (collision.gameObject.CompareTag("Wall"))
        {
            // 드리프트 중이면 드리프트 취소 (예: EndDrift() 호출)
            if (isDrifting)
            {
                EndDrift();
            }

            // 현재 속도를 가져옴
            Vector3 currentVelocity = rigid.velocity;

            // 충돌 접점의 첫 번째 법선 벡터를 가져옴
            Vector3 normal = collision.contacts[0].normal;

            // Vector3.Reflect()를 사용하여 벽에 닿은 방향에 대해 반사된 속도를 계산
            Vector3 reflectedVelocity = Vector3.Reflect(currentVelocity, normal);

            // 튕긴 후 감속 효과를 적용하기 위해 계수를 곱함
            rigid.velocity = reflectedVelocity * wallBounceFactor;

            Debug.Log("벽 충돌: 반사된 벨로시티 적용됨");
        }
    }
    #endregion
}