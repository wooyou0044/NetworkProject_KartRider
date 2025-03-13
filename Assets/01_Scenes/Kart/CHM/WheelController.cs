using System.Collections;
using UnityEngine;

public class WheelController : MonoBehaviour
{
    public float maxTorque = 10f; // �ִ� ��ũ
    public float maxSteerAngle = 30f; // �ִ� ���� ����
    public float maxSpeed = 50f; // �ִ� �ӵ�
    public float antiRollPow;
    public float AnglePow;
    public float driftFactor = 0.5f; // �帮��Ʈ �� ������ ���� ����
    public Transform[] wheels; // ������ Ʈ������ �迭 (0: �տ���, 1: �ڿ���, 2: �տ�����, 3: �ڿ�����)
    private Rigidbody rb;
    private float motorInput; // ���� �Է� ��
    private float steerInput; // ���� �Է� ��
    private bool isDrifting; // �帮��Ʈ ������ ����
    private float originalDrag = 0.1f; // �⺻ ������ ��
    private float driftDrag = 0.5f; // �帮��Ʈ �� ������ ��
    private float driftTime = 1f; // �帮��Ʈ ���� �ð�

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

        // ���� �ӵ��� ���ϰ� �ü����� ��ȯ�Ͽ� ���
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
        float maxSpeedMetersPerSecond = maxSpeed / 3.6f; // �ü��� �ʼ����� ��ȯ
        if (rb.velocity.magnitude < maxSpeedMetersPerSecond)
        {
            Vector3 forwardForce = transform.forward * motorInput * maxTorque;
            rb.AddForce(forwardForce);
        }
        else
        {
            rb.velocity = rb.velocity.normalized * maxSpeedMetersPerSecond;
        }

        // ȸ�� ��ũ ����
        float speedFactor = rb.velocity.magnitude / maxSpeedMetersPerSecond; // ���� �ӵ� ���� ���
        float adjustedAnglePow = AnglePow * (1 - speedFactor); // �ӵ� ������ ���� ȸ�� ����
        Vector3 turnTorque = transform.up * steerInput * maxTorque * adjustedAnglePow;
        rb.AddTorque(turnTorque);
    }


    void SteerAndRotateWheels()
    {
        float steerAngle = maxSteerAngle * steerInput;
        float rotationAngle = motorInput * maxTorque * Time.deltaTime * 50f; // ȸ�� ���� ���

        // �տ��� ���� (wheels[0]) ���� �����̼� ������ Y = 180
        float clampLY = Mathf.Clamp(180 + steerAngle, 150, 210);
        Vector3 wheelEulerAngles0 = wheels[0].localEulerAngles;
        wheels[0].localEulerAngles = new Vector3(0, clampLY, wheelEulerAngles0.z);

        // �տ����� ���� (wheels[2]) ���� �����̼� ������ Y = 0
        float clampRY = Mathf.Clamp(steerAngle, -maxSteerAngle, maxSteerAngle);
        Vector3 wheelEulerAngles2 = wheels[2].localEulerAngles;
        wheels[2].localEulerAngles = new Vector3(0, clampRY, wheelEulerAngles2.z);

        // ������ �ð��� ��� ȸ��
        Vector3 leftRotation = Vector3.forward * rotationAngle;
        Vector3 rightRotation = Vector3.back * rotationAngle;
        // �չ��� ȸ��
        wheels[0].Rotate(rightRotation);
        wheels[1].Rotate(rightRotation);

        // �޹��� ȸ��
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
                rb.AddForceAtPosition(transform.up * antiRollForce / 2, wheelL.position); // ���鿡 �ٽ� �ٵ��� �� ����

            if (groundedR)
                rb.AddForceAtPosition(transform.up * antiRollForce, wheelR.position);
            else
                rb.AddForceAtPosition(transform.up * -antiRollForce / 2, wheelR.position); // ���鿡 �ٽ� �ٵ��� �� ����
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
