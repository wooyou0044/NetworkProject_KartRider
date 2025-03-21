using System.Collections;
using UnityEngine;

public class CHMTestKartController : MonoBehaviour
{
    [Header("Speed Settings")]
    [Tooltip("최대 속도 (km/h)")]
    [SerializeField] private float maxSpeed = 200f; // 최대 속도
    [Tooltip("가속도 (m/s^2)")]
    [SerializeField] private float acceleration = 10f; // 가속도
    [Tooltip("감속도 (m/s^2)")]
    [SerializeField] private float deceleration = 15f; // 감속도

    [Header("Steering Settings")]
    [Tooltip("조향 민감도 (클수록 조향 민감)")]
    [SerializeField] private float steeringForce = 200f; // 조향 민감도

    [Header("Drift Settings")]
    [Tooltip("드리프트 중 속도 감소 비율")]
    [SerializeField] private float driftSpeedReduction = 0.8f; // 드리프트 속도 감소 비율
    [Tooltip("드리프트 시 측면으로 가해지는 힘")]
    [SerializeField] private float driftForce = 50f; // 드리프트 중 측면으로 가해지는 힘

    [Header("Motor Force")]
    [Tooltip("차량의 모터 힘")]
    [SerializeField] private float motorForce = 500f; // 모터 힘 (전진 힘)

    public CHMTestWheelController CHM;
    private Rigidbody rigid;
    private float currentSpeed = 0f; // 현재 속도
    private float inputVertical; // 전진/후진 입력
    private float inputHorizontal; // 조향 입력
    private bool isDrifting = false; // 드리프트 여부

    private float savedSpeed = 0f; // 드리프트 시작 시 속도 저장
    private float savedSteeringAngle = 0f; // 드리프트 시작 시 조향각 저장

    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // 사용자 입력
        inputVertical = Input.GetAxis("Vertical"); // W/S 또는 위/아래 방향키
        inputHorizontal = Input.GetAxis("Horizontal"); // A/D 또는 좌/우 방향키

        // 드리프트 시작
        if (Input.GetKeyDown(KeyCode.LeftShift) && !isDrifting)
        {
            StartDrift();
        }

        // 드리프트 종료
        if (Input.GetKeyUp(KeyCode.LeftShift) && isDrifting)
        {
            StopDrift();
        }
    }

    private void FixedUpdate()
    {
        if (!isDrifting)
        {

            HandleAcceleration(); // 전진/후진 속도 처리
            HandleSteering();     // 조향 처리
        }
    }

    private void HandleAcceleration()
    {
        if (inputVertical != 0)
        {
            // 가속/감속 처리
            currentSpeed += inputVertical * acceleration * Time.fixedDeltaTime;
            currentSpeed = Mathf.Clamp(currentSpeed, -maxSpeed, maxSpeed);

            // Rigidbody 속도 적용 (m/s로 변환)
            rigid.velocity = transform.forward * (currentSpeed / 3.6f);
            rigid.velocity = new Vector3(rigid.velocity.x, rigid.velocity.y, rigid.velocity.z);
        }
        else
        {
            // 입력이 없을 경우 감속 처리
            if (currentSpeed > 0)
                currentSpeed -= deceleration * Time.fixedDeltaTime;
            else if (currentSpeed < 0)
                currentSpeed += deceleration * Time.fixedDeltaTime;

            if (Mathf.Abs(currentSpeed) < 0.1f)
                currentSpeed = 0;

            // Rigidbody 속도 업데이트
            rigid.velocity = transform.forward * (currentSpeed / 3.6f);
            rigid.velocity = new Vector3(rigid.velocity.x, rigid.velocity.y, rigid.velocity.z);
        }
    }

    private void HandleSteering()
    {
        // 속도 비례 조향 각도 계산
        float steerAngle = inputHorizontal * GetSpeedProportionalValue() * steeringForce; // 조향 민감도 반영
        Quaternion turnRotation = Quaternion.Euler(0, steerAngle * Time.fixedDeltaTime, 0);
        rigid.MoveRotation(rigid.rotation * turnRotation);
    }

    // 속도에 비례한 값 반환 (0 ~ 1)
    private float GetSpeedProportionalValue()
    {
        return Mathf.Clamp(1 - (Mathf.Abs(currentSpeed) / maxSpeed), 0.3f, 1f); // 최소 0.3 ~ 최대 1
    }

    private void StartDrift()
    {
        isDrifting = true;

        // 드리프트 시작 시 현재 속도와 조향각 저장
        savedSpeed = currentSpeed;
        savedSteeringAngle = inputHorizontal;

        // 드리프트 시 속도 감소
        currentSpeed *= driftSpeedReduction;

        CHM.SetSkidMarkActive(isDrifting);
        // 드리프트 코루틴 시작
        StartCoroutine(DriftRoutine());

    }

    private void StopDrift()
    {
        isDrifting = false;
        CHM.SetSkidMarkActive(isDrifting);
        StopCoroutine(DriftRoutine());
    }

    private IEnumerator DriftRoutine()
    {
        while (isDrifting)
        {
            // 드리프트 시 저장된 속도를 유지하며 이동
            rigid.velocity = transform.forward * (savedSpeed / 3.6f);

            // 측면 방향으로 힘 가하기
            Vector3 driftForceVector = transform.right * savedSteeringAngle * driftForce;
            rigid.AddForce(driftForceVector, ForceMode.Acceleration);

            // 조향값 반영
            float steerAngle = inputHorizontal * GetSpeedProportionalValue() * steeringForce;
            Quaternion turnRotation = Quaternion.Euler(0, steerAngle * Time.fixedDeltaTime, 0);
            rigid.MoveRotation(rigid.rotation * turnRotation);
            Debug.Log("드리프트");

            yield return null;
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            rigid.velocity = new Vector3(0,rigid.velocity.y,0);
            currentSpeed = -1;

        }
    }
}
