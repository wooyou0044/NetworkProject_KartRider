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
        // �ڽ� ��ü���� WheelController ������Ʈ�� ã���ϴ�.
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

        // WheelController�� SteerAndRotateWheels �Լ��� ȣ���Ͽ� ������ ȸ����Ű�� �����մϴ�.
        //wheelController.SteerAndRotateWheels(steerInput, motorInput);
    }

    void CalculateSpeed()
    {
        // ���� �ӵ��� ���ϰ� �ü����� ��ȯ�Ͽ� ���
        float speedInMetersPerSecond = rb.velocity.magnitude;
        float speedInKilometersPerHour = speedInMetersPerSecond * 3.6f;
        Debug.Log("Speed: " + speedInKilometersPerHour + " km/h");
    }
    
}
