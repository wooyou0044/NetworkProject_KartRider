using System.Collections;
using UnityEngine;

public class WheelController : MonoBehaviour
{
    public float maxTorque = 10f; // 최대 토크
    public float maxSteerAngle = 30f; // 최대 조향 각도
    public float maxSpeed = 50f; // 최대 속도
    public float antiRollPow;
    public float AnglePow;
    public float driftFactor = 0.5f; // 드리프트 시 마찰력 감소 비율
    public Transform[] wheels; // 바퀴의 트랜스폼 배열 (0: 앞왼쪽, 1: 뒤왼쪽, 2: 앞오른쪽, 3: 뒤오른쪽)
    private Rigidbody rb;
    private float motorInput; // 모터 입력 값
    private float steerInput; // 조향 입력 값
    private bool isDrifting; // 드리프트 중인지 여부
    private float originalDrag = 0.1f; // 기본 마찰력 값
    private float driftDrag = 0.5f; // 드리프트 중 마찰력 값
    private float driftTime = 1f; // 드리프트 지속 시간

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = new Vector3(0, -0.5f, 0);
    }

    void Update()
    {
        steerInput = Input.GetAxis("Horizontal");
        motorInput = Input.GetAxis("Vertical");

        if (Input.GetKey(KeyCode.LeftShift))
        {
            StartDrift();
        }
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            EndDrift();
        }

        SteerAndRotateWheels();
        Move();

        // 현재 속도를 구하고 시속으로 변환하여 출력
        float speedInMetersPerSecond = rb.velocity.magnitude;
        float speedInKilometersPerHour = speedInMetersPerSecond * 3.6f;
        Debug.Log("Speed: " + speedInKilometersPerHour + " km/h");
    }

    void StartDrift()
    {
        isDrifting = true;
        foreach (Transform wheel in wheels)
        {
            Collider wheelCollider = wheel.GetComponent<Collider>();
            if (wheelCollider != null)
            {
                Rigidbody wheelRb = wheelCollider.GetComponent<Rigidbody>();
                if (wheelRb != null)
                {
                    wheelRb.drag = driftDrag;
                }
            }
        }
    }

    void EndDrift()
    {
        isDrifting = false;
        StartCoroutine(GraduallyRestoreDrag());
    }

    IEnumerator GraduallyRestoreDrag()
    {
        float elapsedTime = 0f;
        while (elapsedTime < driftTime)
        {
            elapsedTime += Time.deltaTime;
            float currentDrag = Mathf.Lerp(driftDrag, originalDrag, elapsedTime / driftTime);
            foreach (Transform wheel in wheels)
            {
                Collider wheelCollider = wheel.GetComponent<Collider>();
                if (wheelCollider != null)
                {
                    Rigidbody wheelRb = wheelCollider.GetComponent<Rigidbody>();
                    if (wheelRb != null)
                    {
                        wheelRb.drag = currentDrag;
                    }
                }
            }
            yield return null;
        }
    }
    void Move()
    {
        float maxSpeedMetersPerSecond = maxSpeed / 3.6f; // 시속을 초속으로 변환
        if (rb.velocity.magnitude < maxSpeedMetersPerSecond)
        {
            Vector3 forwardForce = transform.forward * motorInput * maxTorque;
            rb.AddForce(forwardForce);
        }
        else
        {
            rb.velocity = rb.velocity.normalized * maxSpeedMetersPerSecond;
        }

        // 회전 토크 적용
        float speedFactor = rb.velocity.magnitude / maxSpeedMetersPerSecond; // 현재 속도 비율 계산
        float adjustedAnglePow = AnglePow * (1 - speedFactor); // 속도 비율에 따른 회전 조정
        Vector3 turnTorque = transform.up * steerInput * maxTorque * adjustedAnglePow;
        rb.AddTorque(turnTorque);
    }


    void SteerAndRotateWheels()
    {
        float steerAngle = maxSteerAngle * steerInput;
        float rotationAngle = motorInput * maxTorque * Time.deltaTime * 50f; // 회전 각도 계산

        // 앞왼쪽 바퀴 (wheels[0]) 로컬 로테이션 포지션 Y = 180
        float clampLY = Mathf.Clamp(180 + steerAngle, 150, 210);
        Vector3 wheelEulerAngles0 = wheels[0].localEulerAngles;
        wheels[0].localEulerAngles = new Vector3(0, clampLY, wheelEulerAngles0.z);

        // 앞오른쪽 바퀴 (wheels[2]) 로컬 로테이션 포지션 Y = 0
        float clampRY = Mathf.Clamp(steerAngle, -maxSteerAngle, maxSteerAngle);
        Vector3 wheelEulerAngles2 = wheels[2].localEulerAngles;
        wheels[2].localEulerAngles = new Vector3(0, clampRY, wheelEulerAngles2.z);

        // 바퀴의 시각적 요소 회전
        Vector3 leftRotation = Vector3.forward * rotationAngle;
        Vector3 rightRotation = Vector3.back * rotationAngle;
        // 앞바퀴 회전
        wheels[0].Rotate(rightRotation);
        wheels[1].Rotate(rightRotation);

        // 뒷바퀴 회전
        wheels[2].Rotate(leftRotation);
        wheels[3].Rotate(leftRotation);
    }

    void FixedUpdate()
    {
        ApplyAntiRollBar();
    }

    void ApplyAntiRollBar()
    {
        for (int i = 0; i < wheels.Length / 2; i++)
        {
            Transform wheelL = wheels[i];
            Transform wheelR = wheels[i + wheels.Length / 2];

            RaycastHit hitL;
            RaycastHit hitR;
            bool groundedL = Physics.Raycast(wheelL.position, -transform.up, out hitL, 1f);
            bool groundedR = Physics.Raycast(wheelR.position, -transform.up, out hitR, 1f);

            float travelL = groundedL ? 1 - hitL.distance : 1;
            float travelR = groundedR ? 1 - hitR.distance : 1;

            float antiRollForce = (travelL - travelR) * antiRollPow;

            if (groundedL)
                rb.AddForceAtPosition(transform.up * -antiRollForce, wheelL.position);
            else
                rb.AddForceAtPosition(transform.up * antiRollForce / 2, wheelL.position); // 지면에 다시 붙도록 힘 조절

            if (groundedR)
                rb.AddForceAtPosition(transform.up * antiRollForce, wheelR.position);
            else
                rb.AddForceAtPosition(transform.up * -antiRollForce / 2, wheelR.position); // 지면에 다시 붙도록 힘 조절
        }
    }


    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        for (int i = 0; i < wheels.Length; i++)
        {
            Gizmos.DrawRay(wheels[i].position, -transform.up * 0.7f);
        }
    }
}
