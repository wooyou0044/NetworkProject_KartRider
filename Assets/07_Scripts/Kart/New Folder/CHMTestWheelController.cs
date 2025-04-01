using UnityEngine;

public class CHMTestWheelController : MonoBehaviour
{
    [Header("Steering Settings")]
    [Tooltip("최소 조향 각도")]
    public float steerAngleFrontMin = -45f;
    [Tooltip("최�? 조향 각도")]
    public float steerAngleFrontMax = 45f;

    [Tooltip("?�키??마크 ?�과")]
    public GameObject[] skidMarks;

    public float maxTorque = 30f;
    public float maxSteerAngle = 30f; 
    public Transform[] wheels; 
    public GameObject skidMark;
    public Transform[] backWheels;
    public LayerMask groundLayer;


    private TestCHMKart kartController;
    bool isGround;

    SkidMark curLeftSkid;
    SkidMark curRightSkid;

    GameObject left;
    GameObject right;
    GameObject skidMarkManager;

    [SerializeField] int poolSize = 50;
    public SkidMarkPool skidMarkPool;

    int curSkidMarkCount;

    void Start()
    {
        kartController = GetComponentInParent<TestCHMKart>();

        SetSkidMarkActive(false);

        skidMarkManager = GameObject.FindGameObjectWithTag("SkidMark");

        skidMarkPool = new SkidMarkPool(skidMark, poolSize, skidMarkManager.transform);
    }

    void Update()
    {
        if(kartController.isDrifting == true && CheckGround())
        {
            if (curLeftSkid == null && curRightSkid == null)
            {
                //left = Instantiate(skidMark);
                //left.transform.position += new Vector3(0, 0.04f, 0);
                //curLeftSkid = left.GetComponent<SkidMark>();
                //right = Instantiate(skidMark);
                //right.transform.position += new Vector3(0, 0.04f, 0);
                //curRightSkid = right.GetComponent<SkidMark>();

                left = skidMarkPool.GetSkidMark();
                left.transform.position += new Vector3(0, 0.06f, 0);
                //left.SetActive(true);
                curLeftSkid = left.GetComponent<SkidMark>();

                right = skidMarkPool.GetSkidMark();
                right.transform.position += new Vector3(0, 0.06f, 0);
                //right.SetActive(true);
                curRightSkid = right.GetComponent<SkidMark>();

                skidMarkPool.ReturnSkidMark(left);
                skidMarkPool.ReturnSkidMark(right);

                if (curSkidMarkCount <= poolSize - 2)
                {
                    curSkidMarkCount += 2;
                }
                else
                {
                    Debug.Log("����");
                    curLeftSkid.ResetSkidMarks();
                    curRightSkid.ResetSkidMarks();
                }
            }

            curLeftSkid.AddSkidMark(backWheels[0].position);
            curRightSkid.AddSkidMark(backWheels[1].position);
        }
        else
        {
            curLeftSkid = null;
            curRightSkid = null;
        }
    }

    private void FixedUpdate()
    {
        
    }

    public void SetSkidMarkActive(bool isActive)
    {
        foreach (GameObject skidMark in skidMarks)
        {
            skidMark.GetComponent<TrailRenderer>().emitting = isActive;
        }
    }

    public void UpdateAndRotateWheels(float steerInput, float motorInput, float speed, bool isDrifting)
    {
        float steeringSensitivity = 1.0f;

        if (wheels.Length >= 2)
        {
            float leftSteerAngle = Mathf.Lerp(steerAngleFrontMin, steerAngleFrontMax, (steerInput + 1f) / 2f) * steeringSensitivity;
            wheels[0].localRotation = Quaternion.Euler(0, leftSteerAngle - 90, wheels[0].localRotation.eulerAngles.z);

            float rightSteerAngle = Mathf.Lerp(steerAngleFrontMin, steerAngleFrontMax, (steerInput + 1f) / 2f) * steeringSensitivity;
            wheels[1].localRotation = Quaternion.Euler(0, rightSteerAngle + 90, wheels[1].localRotation.eulerAngles.z);
        }

        float spinAngle = Mathf.Abs(speed) * Time.deltaTime * maxTorque;

        Vector3 directionLeft = (motorInput >= 0) ? Vector3.back : Vector3.forward;
        Vector3 directionRight = (motorInput >= 0) ? Vector3.forward : Vector3.back;

        foreach (Transform wheel in wheels)
        {
            if(wheel == wheels[0] || wheel == wheels[2])
            {
                wheel.Rotate(directionLeft, spinAngle, Space.Self);
            }
            else
            {
                wheel.Rotate(directionRight, spinAngle, Space.Self);
            }
        }
        //SetSkidMarkActive(isDrifting);
    }

    bool CheckGround()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 0.5f, groundLayer))
        {
            isGround = true;
        }
        else
        {
            isGround = false;
        }
        return isGround;
    }
}
