using System.Collections;
using UnityEngine;

public class CHMTestKartController : MonoBehaviour
{
    [Header("Speed Settings")]
    [SerializeField] private float maxSpeed = 200f; // km/h 기준 최대 속도
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float deceleration = 15f;

    [Header("Steering Settings")]
    [SerializeField] private float steeringForce = 200f;

    [Header("Drift Settings")]
    [SerializeField] private float driftSpeedReduction = 0.8f; // 드리프트 시작시 속도 감소 비율
    [SerializeField] private float driftForce = 50f;           // 드리프트 중 횡력 (AddForce 사용)
    [SerializeField] private float driftSteerStrength = 0.5f;    // 드리프트 중 약간의 조향 효과

    [Header("Motor Force")]
    [SerializeField] private float motorForce = 500f;

    // 스키드 마크 등의 효과를 위한 참조(프로젝트에 맞춰 연결)
    public CHMTestWheelController CHM;

    private Rigidbody rigid;
    private float currentSpeed = 0f; // km/h 단위
    private float inputVertical;
    private float inputHorizontal;
    private bool isDrifting = false;

    // 드리프트 관련 변수
    private float driftStartTime;
    private Vector3 driftDirection;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        // 입력 값 받아오기
        inputVertical = Input.GetAxis("Vertical");
        inputHorizontal = Input.GetAxis("Horizontal");

        // 드리프트 시작 조건:
        // - LeftShift 누름, 좌우 입력이 있고, 현재 속도가 일정 이상일 때
        if (Input.GetKeyDown(KeyCode.LeftShift) && Mathf.Abs(inputHorizontal) > 0.1f && Mathf.Abs(currentSpeed) > maxSpeed * 0.2f)
        {
            StartDrift();
        }

        // 드리프트 종료 조건: LeftShift 키 릴리즈 시
        if (Input.GetKeyUp(KeyCode.LeftShift) && isDrifting)
        {
            StopDrift();
        }
    }

    private void FixedUpdate()
    {
        // 드리프트 중이 아닐때만 일반 이동과 조향 처리
        if (!isDrifting)
        {
            HandleAcceleration();
            HandleSteering();
        }
    }

    // 전진/후진은 rigidbody.velocity 값을 직접 제어 (km/h -> m/s 변환)
    private void HandleAcceleration()
    {
        if (inputVertical != 0)
        {
            currentSpeed += inputVertical * acceleration * Time.fixedDeltaTime;
            currentSpeed = Mathf.Clamp(currentSpeed, -maxSpeed, maxSpeed);
        }
        else
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0, deceleration * Time.fixedDeltaTime);
        }

        Vector3 newVelocity = transform.forward * (currentSpeed / 3.6f);
        rigid.velocity = new Vector3(newVelocity.x, rigid.velocity.y, newVelocity.z);
    }

    // 일반 이동 시 조향 처리 (속도에 따른 보정 포함)
    private void HandleSteering()
    {
        float steerAngle = inputHorizontal * GetSpeedProportionalValue() * steeringForce;
        Quaternion turnRotation = Quaternion.Euler(0, steerAngle * Time.fixedDeltaTime, 0);
        rigid.MoveRotation(rigid.rotation * turnRotation);
    }

    // 속도에 따른 조향 감도 보정: 속도가 빠를수록 값이 낮게
    private float GetSpeedProportionalValue()
    {
        return Mathf.Clamp(1 - (Mathf.Abs(currentSpeed) / maxSpeed), 0.3f, 1f);
    }

    // 드리프트 시작
    private void StartDrift()
    {
        isDrifting = true;
        driftStartTime = Time.time;
        driftDirection = transform.forward; // 드리프트 시작 시 전진 방향 저장

        // 드리프트 시작되면 현재 속도에 비례하여 속도 감소 적용
        currentSpeed *= driftSpeedReduction;
        rigid.velocity = transform.forward * (currentSpeed / 3.6f);

        if (CHM != null)
            CHM.SetSkidMarkActive(isDrifting);

        // 드리프트 시 AddForce를 이용해 횡 방향 힘 적용
        StartCoroutine(DriftRoutine());
    }

    // 드리프트 중 동작: 좌우 입력에 따라 횡력 추가 및 약간의 조향 적용
    private IEnumerator DriftRoutine()
    {
        while (isDrifting)
        {
            float lateralForce = inputHorizontal * driftForce;
            rigid.AddForce(transform.right * lateralForce, ForceMode.Acceleration);

            float driftSteer = inputHorizontal * driftSteerStrength;
            transform.Rotate(0, driftSteer, 0);

            yield return null;
        }
    }

    // 드리프트 종료: 드리프트 효과 해제 후 속도를 보정하고 차량 정렬 보정 시작
    private void StopDrift()
    {
        isDrifting = false;

        if (CHM != null)
            CHM.SetSkidMarkActive(isDrifting);

        // 드리프트 종료 후 부드러운 속도 감소 처리
        StartCoroutine(SmoothSpeedRecovery());
        // 드리프트 종료 후 차량이 현재 이동 방향(정면)을 바라보도록 회전 보정
        StartCoroutine(AlignForwardCoroutine());
    }

    // 드리프트 종료 후 부드럽게 속도를 감소(예, 20% 추가 감소)
    private IEnumerator SmoothSpeedRecovery()
    {
        float recoveryTime = 0.5f;
        float targetSpeed = currentSpeed * 0.8f;
        float elapsedTime = 0;

        while (elapsedTime < recoveryTime)
        {
            currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, elapsedTime / recoveryTime);
            elapsedTime += Time.deltaTime;

            Vector3 newVelocity = transform.forward * (currentSpeed / 3.6f);
            rigid.velocity = new Vector3(newVelocity.x, rigid.velocity.y, newVelocity.z);

            yield return null;
        }

        currentSpeed = targetSpeed;
    }

    // 드리프트 종료 후 현재 이동 방향(velocity 기준)에 차가 서서히 정면 정렬하도록 하는 코루틴
    private IEnumerator AlignForwardCoroutine()
    {
        float alignmentDuration = 1.0f;
        float elapsed = 0f;
        Quaternion initialRotation = transform.rotation;

        // 차량이 이동 중이면 그 방향을 목표 회전값으로, 정지 상태면 현재 forward 유지
        Vector3 velocity = rigid.velocity;
        if (velocity.sqrMagnitude < 0.1f)
        {
            velocity = transform.forward;
        }
        velocity.y = 0;
        velocity.Normalize();
        Quaternion targetRotation = Quaternion.LookRotation(velocity, Vector3.up);

        while (elapsed < alignmentDuration)
        {
            transform.rotation = Quaternion.Slerp(initialRotation, targetRotation, elapsed / alignmentDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.rotation = targetRotation;
    }
}
