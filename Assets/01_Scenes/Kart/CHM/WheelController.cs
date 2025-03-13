using UnityEngine;

public class WheelController : MonoBehaviour
{
    public float maxTorque = 10f; // 모터의 최대 토크
    public float maxSteerAngle = 30f; // 최대 조향 각도
    public float maxSpeed = 50f; // 최대 속도
    public float antiRollPow;
    public float AnglePow;
    public Transform[] wheels; // 바퀴의 트랜스폼 배열 (0: 왼쪽 앞바퀴, 1: 왼쪽 뒷바퀴, 2: 오른쪽 앞바퀴, 3: 오른쪽 뒷바퀴)

    private Rigidbody rb;
    private float motorInput; // 모터 입력 값
    private float steerInput; // 조향 입력 값

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        steerInput = Input.GetAxis("Horizontal");
        motorInput = Input.GetAxis("Vertical");
        SteerAndRotateWheels(steerInput, motorInput);
        Move(steerInput, motorInput);

    }

    public void Move(float steerInput, float motorInput)
    //public void Move()
    {
        if (rb.velocity.magnitude < maxSpeed)
        {
            Vector3 forwardDirection = rb.transform.forward;
            Vector3 forwardForce = forwardDirection * motorInput *maxTorque;
            rb.AddForce(forwardForce);
        }
        else
        {
            rb.velocity = rb.velocity.normalized * maxSpeed;
        }

        Vector3 turnTorque = transform.up * steerInput * maxTorque * AnglePow;
        rb.AddTorque(turnTorque);
    }

    public void SteerAndRotateWheels(float steerInput, float motorInput)
    {
        float steerAngle = maxSteerAngle * steerInput;
        float rotationAngle = motorInput * maxTorque * Time.deltaTime * 50f; // 회전 각도 계산

        // 왼쪽 앞바퀴 (wheels[0]) 로컬 로테이션 포지션 Y = 0
        float clamp = Mathf.Clamp(steerAngle, -maxSteerAngle, maxSteerAngle) + 90;
        Vector3 wheelEulerAngles0 = wheels[0].localEulerAngles;
        wheels[0].localRotation = Quaternion.Euler(0, clamp, wheelEulerAngles0.z);

        // 오른쪽 앞바퀴 (wheels[2]) 로컬 로테이션 포지션 Y = 180
        Vector3 wheelEulerAngles2 = wheels[2].localEulerAngles;
        wheels[2].localRotation = Quaternion.Euler(0, clamp, wheelEulerAngles2.z);

        // 바퀴의 시각적 요소 회전
        Vector3 leftRotation = Vector3.forward * rotationAngle;
        Vector3 rightRotation = Vector3.back * rotationAngle;
        //좌측
        wheels[0].Rotate(rightRotation);
        wheels[1].Rotate(rightRotation);

        //우측
        wheels[2].Rotate(leftRotation);
        wheels[3].Rotate(leftRotation);
    }

    //void FixedUpdate()
    //{
    //    ApplyAntiRollBar();
    //}

    public void ApplyAntiRollBar()
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
            if (groundedR)
                rb.AddForceAtPosition(transform.up * antiRollForce, wheelR.position);
        }
    }
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        for (int i = 0; i < wheels.Length; i++)
        {
            Gizmos.DrawRay(wheels[i].position, -transform.up * 1f);
        }
    }
}
