using UnityEngine;

public class CHMTestWheelController : MonoBehaviour
{
    [Header("Steering Settings")]
    [Tooltip("최소 조향 각도")]
    public float steerAngleFrontMin = -45f;
    [Tooltip("최대 조향 각도")]
    public float steerAngleFrontMax = 45f;

    [Tooltip("스키드 마크 효과")]
    public GameObject[] skidMarks;

    public float maxTorque = 30f; // 최대 토크
    public float maxSteerAngle = 30f; // 최대 조향 각도
    public Transform[] wheels; // 바퀴 트랜스폼 배열 (0: 왼쪽 앞바퀴, 1: 오른쪽 앞바퀴, 2: 왼쪽 뒷바퀴, 3: 오른쪽 뒷바퀴)
    public GameObject skidMark;
    public Transform[] backWheels; // 뒷 바퀴 트랜스폼
    public LayerMask groundLayer;


    // 카트 컨트롤러 참조
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
        // 카트 컨트롤러 참조 가져오기
        kartController = GetComponentInParent<TestCHMKart>();

        // 스키드 마크 초기 비활성화
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
                left.SetActive(true);
                curLeftSkid = left.GetComponent<SkidMark>();

                right = skidMarkPool.GetSkidMark();
                right.transform.position += new Vector3(0, 0.06f, 0);
                right.SetActive(true);
                curRightSkid = right.GetComponent<SkidMark>();

                skidMarkPool.ReturnSkidMark(left);
                skidMarkPool.ReturnSkidMark(right);

                if (curSkidMarkCount < 8)
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

    /// <summary>
    /// 입력값을 기반으로 바퀴의 회전 및 조향, 회전(spin) 처리하고 기본 안티롤 기능을 적용합니다.
    /// </summary>
    public void UpdateAndRotateWheels(float steerInput, float motorInput, float speed, bool isDrifting)
    {
        float steeringSensitivity = 1.0f; // 필요에 따라 조정

        // 전면 바퀴(왼쪽 앞, 오른쪽 앞) 조향 처리
        if (wheels.Length >= 2)
        {
            // 왼쪽 앞바퀴
            float leftSteerAngle = Mathf.Lerp(steerAngleFrontMin, steerAngleFrontMax, (steerInput + 1f) / 2f) * steeringSensitivity;
            wheels[0].localRotation = Quaternion.Euler(0, leftSteerAngle - 90, wheels[0].localRotation.eulerAngles.z);

            // 오른쪽 앞바퀴
            float rightSteerAngle = Mathf.Lerp(steerAngleFrontMin, steerAngleFrontMax, (steerInput + 1f) / 2f) * steeringSensitivity;
            wheels[1].localRotation = Quaternion.Euler(0, rightSteerAngle - 90, wheels[1].localRotation.eulerAngles.z);
        }

        // 모든 바퀴에 대해 회전(spin) 적용
        float spinAngle = Mathf.Abs(speed) * Time.deltaTime * maxTorque;
        foreach (Transform wheel in wheels)
        {
            // 전진이면 앞으로, 후진이면 반대로 회전하도록 조건 추가 가능
            wheel.Rotate(Vector3.forward, spinAngle, Space.Self);
        }
        // 드리프트 상태라면 스키드 마크 효과 활성화
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
