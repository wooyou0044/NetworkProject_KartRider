using UnityEngine;

public class CarController : MonoBehaviour
{
    public float maxTorque = 10f;
    public float maxSpeed = 50f;
    public float AnglePow;
    public Transform[] wheels;
    private Rigidbody rb;
    private WheelController wheelController;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        // 자식 객체에서 WheelController 컴포넌트를 찾습니다.
        wheelController = GetComponentInChildren<WheelController>();
    }

    void Update()
    {
        float motorInput = Input.GetAxis("Vertical");
        float steerInput = Input.GetAxis("Horizontal");
        Move(motorInput, steerInput);
        CalculateSpeed();
        
    }

    void Move(float motorInput, float steerInput)
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

        // WheelController의 SteerAndRotateWheels 함수를 호출하여 바퀴를 회전시키고 조향합니다.
        //wheelController.SteerAndRotateWheels(steerInput, motorInput);
    }

    void CalculateSpeed()
    {
        // 현재 속도를 구하고 시속으로 변환하여 출력
        float speedInMetersPerSecond = rb.velocity.magnitude;
        float speedInKilometersPerHour = speedInMetersPerSecond * 3.6f;
        Debug.Log("Speed: " + speedInKilometersPerHour + " km/h");
    }
    
}
