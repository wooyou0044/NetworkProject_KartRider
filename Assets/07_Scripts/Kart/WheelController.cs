using UnityEngine;

public class WheelController : MonoBehaviour
{
    public float maxTorque = 10f; // ������ �ִ� ��ũ
    public float maxSteerAngle = 30f; // �ִ� ���� ����
    public Transform[] wheels; // ������ Ʈ������ �迭 (0: ���� �չ���, 1: ���� �޹���, 2: ������ �չ���, 3: ������ �޹���)

    [Header("Steering Settings")]
    [Tooltip("�ּ� ���� ����")]
    public float steerAngleFrontMin = -45f;
    [Tooltip("�ִ� ���� ����")]
    public float steerAngleFrontMax = 45f;

    [Tooltip("��Ű�� ��ũ ȿ��")]
    public GameObject[] skidMarks;
    public GameObject skidMark;

    public Transform[] backWheelPos;

    KartController kartCtrl;
    SkidMark skidMarkCtrl;

    GameObject skidBack1;
    GameObject skidBack2;

    void Awake()
    {
        kartCtrl = transform.GetComponentInParent<KartController>();
        skidMarkCtrl = skidMark.GetComponent<SkidMark>();
    }

    void Start()
    {
        // ��Ű�帶ũ �ʱ� ��Ȱ��ȭ
        SetSkidMarkActive(false);
    }

    void Update()
    {
        if(kartCtrl == null)
        {
            kartCtrl = transform.GetComponentInParent<KartController>();
        }
        else
        {
            if(kartCtrl.isDrifting == true)
            {
                skidBack1 = Instantiate(skidMark, backWheelPos[0].position, backWheelPos[0].rotation);
                skidBack2 = Instantiate(skidMark, backWheelPos[1].position, backWheelPos[1].rotation);
                if(skidBack1 != null && skidBack2 != null)
                {
                    skidMarkCtrl.AddSkidMark(skidBack1.transform.position);
                    skidMarkCtrl.AddSkidMark(skidBack2.transform.position);
                }
            }
            else
            {
                skidBack1 = null;
                skidBack2 = null;
            }
        }
    }

    public void SetSkidMarkActive(bool isActive)
    {
        foreach (GameObject skidMark in skidMarks)
        {
            skidMark.GetComponent<TrailRenderer>().emitting = isActive;
        }
    }

    public void RotateWheel(float steerInput, float steeringSensitivity)
    {
        // ���� ���� ���
        float steerAngleFrontLeft = Mathf.Lerp(steerAngleFrontMin, steerAngleFrontMax, (steerInput + 1) / 2) * steeringSensitivity;
        float steerAngleFrontRight = Mathf.Lerp(steerAngleFrontMin, steerAngleFrontMax, (steerInput + 1) / 2) * steeringSensitivity;

        // �� ���� ȸ��
        wheels[0].localRotation = Quaternion.Euler(0, steerAngleFrontLeft - 90, wheels[0].localRotation.eulerAngles.z);
        wheels[2].localRotation = Quaternion.Euler(0, steerAngleFrontRight - 90, wheels[1].localRotation.eulerAngles.z);
    }

    public void UpdateWheelRotation(float motorInput, float speed)
    {
        foreach (Transform wheel in wheels)
        {
            wheel.Rotate(Vector3.back * motorInput * speed);
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
