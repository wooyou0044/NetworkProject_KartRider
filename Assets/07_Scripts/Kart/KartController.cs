using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KartController : MonoBehaviour
{
    [SerializeField] GameObject wheels;
    [SerializeField] GameObject carBody;

    public float maxSpeed = 50f; // 최대 속도
    public float antiRollPow;
    public float AnglePow;

    WheelController wheelCtrl;
    Rigidbody rigid;
    Transform[] wheelTrans;

    private float motorInput; // 모터 입력 값
    private float steerInput; // 조향 입력 값
    private float driftTime = 0f;

    private bool isDrifting = false;
    private bool isBoostTriggered = false;
    private bool isUpArrowKeyPressed = false; // 현재 키가 눌려있는 상태
    private bool wasUpArrowKeyReleased = true; // 이전에 키가 떼어진 상태

    [Header("Steering Settings")]
    [Tooltip("조향 민감도")]
    public float steeringForce = 200f;

    [Header("Motor Settings")]
    [Tooltip("가속도 힘")]
    public float motorForce = 1000f;
    [Tooltip("최대 속도 (km/h)")]
    public float maxSpeedKPH = 280f;

    [Header("Physics Settings")]
    [Tooltip("안티 롤 강도")]
    public float antiRollForce = 5000f;
    [Tooltip("지면 레이어")]
    public LayerMask groundLayer;

    [Header("Drift Settings")]
    [Tooltip("드리프트 키 설정")]
    public KeyCode driftKey = KeyCode.LeftShift;
    [Tooltip("드리프트 감속 비율")]
    public float driftFactor = 0.5f;
    [Tooltip("드리프트 측면 힘")]
    public float driftForceSide = 200f;
    [Tooltip("최대 드리프트 지속 시간")]
    public float maxDriftDuration = 2f;
    [Tooltip("드리프트 중 현재 측면 힘")]
    public float currentDriftForce = 20f;

    [Header("Drag Settings")]
    [Tooltip("기본 드래그 값")]
    public float normalDrag = 0.5f;
    [Tooltip("드리프트 중 드래그 값")]
    public float driftDrag = 0.01f;
    [Tooltip("기본 Angular Drag 값")]
    public float normalAngularDrag = 0.05f;
    [Tooltip("드리프트 중 Angular Drag 값")]
    public float driftAngularDrag = 0.01f;

    [Header("Boost Settings")]
    [Tooltip("부스트 힘")]
    public float boostForce = 1.1f;
    [Tooltip("부스트 지속 시간")]
    public float boostDuration = 1.5f;

    private void Awake()
    {
        wheelCtrl = wheels.GetComponent<WheelController>();
        rigid = GetComponent<Rigidbody>();
    }

    void Start()
    {
        wheelTrans = new Transform[wheels.transform.childCount];
        for (int i = 0; i < wheelTrans.Length; i++)
        {
            wheelTrans[i] = wheels.transform.GetChild(i).transform;
        }

        // 리지드바디 드래그 초기화
        rigid.drag = normalDrag;
        // 기본 Angular Drag 값 초기화
        rigid.angularDrag = normalAngularDrag;
    }

    void Update()
    {
        steerInput = Input.GetAxis("Horizontal");
        motorInput = Input.GetAxis("Vertical");

        // 키 입력 상태 추적
        if (Input.GetKey(KeyCode.UpArrow))
        {
            if (wasUpArrowKeyReleased)
            {
                isUpArrowKeyPressed = true; // 새롭게 키 입력
                wasUpArrowKeyReleased = false; // 입력 상태로 전환
            }
            else
            {
                isUpArrowKeyPressed = false; // 지속 입력은 무시
            }
        }
        else
        {
            wasUpArrowKeyReleased = true; // 키가 떼어진 상태로 변경
            isUpArrowKeyPressed = false; // 입력 해제
        }

        if (steerInput != 0 || motorInput != 0)
        {
            //wheelCtrl.SteerAndRotateWheels(steerInput, motorInput);
            //Move(steerInput, motorInput);

            float speed = rigid.velocity.magnitude;
            wheelCtrl.UpdateWheelRotation(motorInput, speed);
            //DisplaySpeed();
        }
    }

    private void FixedUpdate()
    {
        if (steerInput != 0 || motorInput != 0)
        {
            HandleSteering(steerInput);  // 조향 처리
            HandleMovement(motorInput); // 가속/감속 처리
            //ApplyAntiRoll();            // 안티롤 처리
            LimitSpeed();               // 최대 속도 제한

            // 드리프트 시작 및 종료 관리
            if (Input.GetKey(driftKey))
            {
                StartDrift(); // 드리프트 시작
            }
            else
            {
                EndDrift(); // 드리프트 종료
            }
        }
    }

    private void LimitSpeed()
    {
        float currentSpeed = rigid.velocity.magnitude * 3.6f;
        if (currentSpeed > maxSpeedKPH)
        {
            float speedLimit = maxSpeedKPH / 3.6f;
            rigid.velocity = rigid.velocity.normalized * speedLimit;
        }
    }

    public void StartDrift()
    {
        if (!isDrifting)
        {
            isDrifting = true;
            driftTime = 0f;

            // 즉각적인 감속 효과
            rigid.velocity *= 0.8f;
            // 드리프트 중 드래그 값 적용
            rigid.drag = driftDrag;
            // Angular Drag 조정
            rigid.angularDrag = driftAngularDrag;

            // 드리프트 힘 조정
            float speedFactor = Mathf.Clamp01(rigid.velocity.magnitude / (maxSpeedKPH / 3.6f));
            float adjustedDriftForceSide = driftForceSide * (1 - speedFactor) * 5f;

            if (Input.GetAxis("Horizontal") < 0)
            {
                currentDriftForce = Mathf.Lerp(currentDriftForce, adjustedDriftForceSide, Time.deltaTime * 10f);
                Vector3 sideForce = transform.right * -currentDriftForce;
                rigid.AddForce(sideForce, ForceMode.Acceleration);
            }
            else if (Input.GetAxis("Horizontal") > 0)
            {
                currentDriftForce = Mathf.Lerp(currentDriftForce, adjustedDriftForceSide, Time.deltaTime * 10f);
                Vector3 sideForce = transform.right * currentDriftForce;
                rigid.AddForce(sideForce, ForceMode.Acceleration);
            }

            // 스키드 마크 활성화
            //wheelCtrl.SetSkidMarkActive(true);
            Debug.Log("드리프트 시작!");
        }
    }

    public void EndDrift()
    {
        if (isDrifting)
        {
            isDrifting = false;
            driftTime = 0f;
            currentDriftForce = 0f;
            // 기본 드래그 값 복원
            rigid.drag = normalDrag;
            // Angular Drag 복원
            rigid.angularDrag = normalAngularDrag;

            // 스키드마크 비활성화
            //wheelCtrl.SetSkidMarkActive(false);

            Debug.Log("드리프트 종료!");
            StartCoroutine(BoostCheckCoroutine());
        }
    }
    private IEnumerator BoostCheckCoroutine()
    {
        float timer = 0f;
        while (timer < 0.5f)
        {
            if (isUpArrowKeyPressed) // 재입력 감지
            {
                TriggerBoost();
                break;
            }
            timer += Time.deltaTime;
            yield return null;
        }
    }

    private void TriggerBoost()
    {
        if (!isBoostTriggered)
        {
            isBoostTriggered = true;
            Debug.Log("부스터 발동!");
            StartCoroutine(ApplyBoostCoroutine());
        }
    }

    private IEnumerator ApplyBoostCoroutine()
    {
        float timer = 0f;
        while (timer < boostDuration)
        {
            float currentSpeed = rigid.velocity.magnitude * 3.6f; // m/s를 km/h로 변환
            if (currentSpeed < maxSpeedKPH) // 최대 속도 초과 여부 확인
            {
                rigid.AddForce(transform.forward * boostForce, ForceMode.Acceleration);
            }
            timer += Time.deltaTime;
            yield return null;
        }
        isBoostTriggered = false;
    }

    private void HandleSteering(float steerInput)
    {
        float currentSpeed = rigid.velocity.magnitude;
        float steeringSensitivity = Mathf.Clamp(1 - (currentSpeed / (maxSpeedKPH / 3.6f)), 0.5f, 2.0f); // 속도에 따라 조향 민감도 증가

        // 이동 방향과 조화
        if (isDrifting) // 드리프트 중에는 강화된 유턴 효과 추가
        {
            rigid.velocity = Quaternion.Euler(0, steerInput * 90f * Time.deltaTime, 0) * rigid.velocity;
        }

        wheelCtrl.RotateWheel(steerInput, steeringSensitivity);

        // 차량 자체 회전 처리
        Vector3 turnDirection = Quaternion.Euler(0, steerInput * steeringForce * steeringSensitivity * Time.deltaTime, 0) * transform.forward;
        rigid.MoveRotation(Quaternion.LookRotation(turnDirection));
    }

    private void HandleMovement(float motorInput)
    {
        float adjustedMotorForce = isDrifting ? motorForce * 0.3f : motorForce;
        Vector3 forwardForce = transform.forward * motorInput * adjustedMotorForce;

        rigid.AddForce(forwardForce, ForceMode.Force);
    }

    private void DisplaySpeed()
    {
        float speed = rigid.velocity.magnitude * 3.6f; // m/s를 km/h로 변환
        //if (speedText != null)
        //{
        //    speedText.text = "Speed: " + speed.ToString("F2") + " km/h";
        //}
    }

    void ApplyAntiRoll()
    {
        for (int i = 0; i < wheelTrans.Length / 2; i++)
        {
            float leftSuspension = GetWheelSuspensionForce(wheelTrans[i]);
            float rightSuspension = GetWheelSuspensionForce(wheelTrans[i + 1]);

            float antiRoll = (leftSuspension - rightSuspension) * antiRollForce;

            if (leftSuspension > 0)
                rigid.AddForceAtPosition(wheelTrans[i].up * antiRoll, wheelTrans[i].position, ForceMode.Force);

            if (rightSuspension > 0)
                rigid.AddForceAtPosition(wheelTrans[i + 1].up * -antiRoll, wheelTrans[i + 1].position, ForceMode.Force);
        }
    }

    private float GetWheelSuspensionForce(Transform wheel)
    {
        Ray ray = new Ray(wheel.position, -wheel.up);
        if (Physics.Raycast(ray, out RaycastHit hit, 1f, groundLayer))
        {
            return 1 - hit.distance;
        }
        return 0;
    }
}
