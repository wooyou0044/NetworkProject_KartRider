using UnityEngine;

public class CarController : MonoBehaviour
{
    public float maxTorque = 10f; // ������ �ִ� ��ũ
    public float maxSteerAngle = 30f; // �ִ� ���� ����
    public float maxSpeed = 50f; // �ִ� �ӵ�
    public float antiRollPow;
    public float AnglePow;
    public Transform[] wheels; // ������ Ʈ������ �迭 (0: ���� �չ���, 1: ���� �޹���, 2: ������ �չ���, 3: ������ �޹���)

    private Rigidbody rb;
    private float motorInput; // ���� �Է� ��
    private float steerInput; // ���� �Է� ��

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        steerInput = Input.GetAxis("Horizontal");
        motorInput = Input.GetAxis("Vertical");
        SteerAndRotateWheels();
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
    /* Test
    void TestSteerAndRotateWheels()
    {
        float steerAngle = maxSteerAngle * steerInput;
        //�չ��� ���� ���� ���� 
        float rotationAngle = motorInput * maxTorque * Time.deltaTime * 50f;//���� �߷°� ���߸´� ��
        Vector3 leftRotation = Vector3.left * rotationAngle;
        Vector3 rightRotation = Vector3.right * rotationAngle;

        //���� �չ��� ���÷����̼� ������Y = 0
        Vector3 wheelEulerAngles0 = wheels[0].localEulerAngles;
        float clampLY = Mathf.Clamp(steerAngle, -maxSteerAngle, maxSteerAngle);
        wheels[0].localEulerAngles = new Vector3(wheels[0].localEulerAngles.x + wheelEulerAngles0.x, clampLY, 0);

        //������ �չ��� ���÷����̼� ������Y = 180
        Vector3 wheelEulerAngles1 = wheels[2].localEulerAngles;
        float clampRY = Mathf.Clamp(180 + steerAngle, 150, 210);
        wheels[2].localEulerAngles = new Vector3(wheels[2].localEulerAngles.x + wheelEulerAngles1.x, clampRY, 0);  

        //���� ���� +
        wheels[0].Rotate(rightRotation);
        wheels[1].Rotate(rightRotation);
        //���� ���� -
        wheels[2].Rotate(leftRotation);
        wheels[3].Rotate(leftRotation);

        //wheels[0].localEulerAngles = new Vector3(wheels[0].localEulerAngles.x + rightRotation.x, clampLY, 0);
        //wheels[2].localEulerAngles = new Vector3(wheels[2].localEulerAngles.x + rightRotation.x, clampRY, 0);

    }
    */
    void SteerAndRotateWheels()
    {
        float steerAngle = maxSteerAngle * steerInput;
        float rotationAngle = motorInput * maxTorque * Time.deltaTime * 50f; // ȸ�� ���� ���

        // ���� �չ��� (wheels[0]) ���� �����̼� ������ Y = 0
        float clampLY = Mathf.Clamp(steerAngle, -maxSteerAngle, maxSteerAngle);
        Vector3 wheelEulerAngles0 = wheels[0].localEulerAngles;
        wheels[0].localEulerAngles = new Vector3(wheelEulerAngles0.x, clampLY, 0);

        // ������ �չ��� (wheels[2]) ���� �����̼� ������ Y = 180
        float clampRY = Mathf.Clamp(180 + steerAngle, 150, 210);
        Vector3 wheelEulerAngles2 = wheels[2].localEulerAngles;
        wheels[2].localEulerAngles = new Vector3(wheelEulerAngles2.x, clampRY, 0);

        // ������ �ð��� ��� ȸ��
        Vector3 leftRotation = Vector3.left * rotationAngle;
        Vector3 rightRotation = Vector3.right * rotationAngle;

        wheels[0].Rotate(rightRotation);
        wheels[1].Rotate(rightRotation);
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
