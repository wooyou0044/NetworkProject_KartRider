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
        SteerAndRotateWheels();
        TestSteerAndRotateWheels();
        Move();

    }

    void Move()
    {
        if (rb.velocity.magnitude < maxSpeed)
        {
            Vector3 forwardForce = transform.forward * motorInput * maxTorque;
            rb.AddForce(forwardForce);
        }
        else
        {
            rb.velocity = rb.velocity.normalized * maxSpeed;
        }

        Vector3 turnTorque = transform.up * steerInput * maxTorque * AnglePow;
        rb.AddTorque(turnTorque);
    }
    
    void TestSteerAndRotateWheels()
    {
        float steerAngle = maxSteerAngle * steerInput;
        //앞바퀴 바퀴 각도 제한 
        float rotationAngle = motorInput * maxTorque * Time.deltaTime * 50f;//지금 중력과 얼추맞는 힘
    
        Vector3 leftRotation = Vector3.left * rotationAngle;
        Vector3 rightRotation = Vector3.right * rotationAngle;
    
        //왼쪽 앞바퀴 로컬로테이션 포지션Y = 0
        Vector3 wheelEulerAngles0 = wheels[0].localEulerAngles;
        float clampLY = Mathf.Clamp(steerAngle, -30, 30);
        wheels[0].localEulerAngles = new Vector3(wheels[0].localEulerAngles.x + wheelEulerAngles0.x, clampLY, 0);
    
        //오른쪽 앞바퀴 로컬로테이션 포지션Y = 180
        Vector3 wheelEulerAngles1 = wheels[2].localEulerAngles;
        float clampRY = Mathf.Clamp(180 + steerAngle, 150, 210);
        wheels[2].localEulerAngles = new Vector3(wheels[2].localEulerAngles.x + wheelEulerAngles1.x, clampRY, 0);
    
        //좌측 바퀴 +
        wheels[0].Rotate(rightRotation);
        wheels[1].Rotate(rightRotation);
        //우측 바퀴 -
        wheels[2].Rotate(leftRotation);
        wheels[3].Rotate(leftRotation);
    
        //wheels[0].localEulerAngles = new Vector3(wheels[0].localEulerAngles.x + rightRotation.x, clampLY, 0);
        //wheels[2].localEulerAngles = new Vector3(wheels[2].localEulerAngles.x + rightRotation.x, clampRY, 0);
    
    }

    void SteerAndRotateWheels()
    {
        float steerAngle = maxSteerAngle * steerInput;
        float rotationAngle = motorInput * maxTorque * Time.deltaTime * 50f; // 회전 각도 계산
    
        // 왼쪽 앞바퀴 (wheels[0]) 로컬 로테이션 포지션 Y = 0
        float clampLY = Mathf.Clamp(steerAngle, -maxSteerAngle, maxSteerAngle);
        Vector3 wheelEulerAngles0 = wheels[0].localEulerAngles;
        wheels[0].localEulerAngles = new Vector3(wheelEulerAngles0.x, clampLY, 0);
    
        // 오른쪽 앞바퀴 (wheels[2]) 로컬 로테이션 포지션 Y = 180
        float clampRY = Mathf.Clamp(180 + steerAngle, 150, 210);
        Vector3 wheelEulerAngles2 = wheels[2].localEulerAngles;
        wheels[2].localEulerAngles = new Vector3(wheelEulerAngles2.x, clampRY, 0);
    
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
