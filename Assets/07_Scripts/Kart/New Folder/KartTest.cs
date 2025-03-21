using UnityEngine;
using System.Collections;

public class KartTest : MonoBehaviour
{
    [Header("카트 속도 설정")]
    public float maxSpeed = 20f;
    public float acceleration = 10f;
    public float deceleration = 5f;
    public float reverseSpeed = 10f;
    public float turnSpeed = 5f;
    public float driftSteerStrength = 1.2f;
    public float driftSpeedReduction = 0.2f;
    public float driftFriction = 0.98f;

    [Header("드리프트 설정")]
    public float minDriftTime = 0.1f;
    public float maxDriftTime = 1.0f;
    public float driftExtendTime = 0.2f;
    public float shortDriftThreshold = 0.3f;
    public float optimizedDriftAngleThreshold = 30f;

    [Header("안티롤 설정")]
    public float antiRollForce = 500f;

    private Rigidbody rb;
    private CHMTestWheelController wheelController;
    public bool isDrifting = false;
    private float driftStartTime;
    private float currentSpeed;
    private float steeringInput;
    public int steeringDirection;
    private bool hasCuttingDriftCondition = false;
    private float lastDriftEndTime;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        wheelController = GetComponentInChildren<CHMTestWheelController>();
    }

    private void Update()
    {
        HandleInput();
    }

    private void FixedUpdate()
    {
        MoveKart();
        
        if (isDrifting)
        {
            Vector3 driftForce = transform.right * (-steeringDirection) * 2f;
            rb.AddForce(driftForce, ForceMode.Acceleration);
        }
    }

    void HandleInput()
    {
        float moveInput = Input.GetAxis("Vertical");
        float turnInput = Input.GetAxis("Horizontal");

        if (moveInput > 0)
        {
            currentSpeed = Mathf.Clamp(currentSpeed + moveInput * acceleration * Time.deltaTime, -reverseSpeed, maxSpeed);
        }
        else if (moveInput < 0)
        {
            currentSpeed = Mathf.Clamp(currentSpeed + moveInput * deceleration * Time.deltaTime, -reverseSpeed, maxSpeed);
        }
        else
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0, deceleration * Time.deltaTime);
        }

        steeringInput = turnInput;
        steeringDirection = (turnInput > 0) ? 1 : (turnInput < 0) ? -1 : 0;

        if (!isDrifting && (Mathf.Abs(currentSpeed) > 0.1f || Mathf.Abs(steeringInput) > 0.1f))
        {
            StartDrift();
        }

        if (isDrifting && Input.GetKeyDown(KeyCode.LeftShift))
        {
            ExtendDrift();
        }

        if (Input.GetKeyUp(KeyCode.LeftShift) || IsOppositeInput())
        {
            EndDrift();
        }
    }

    void MoveKart()
    {
        rb.velocity = transform.forward * currentSpeed;

        float rotationAmount = steeringInput * turnSpeed * Time.deltaTime;
        rb.MoveRotation(rb.rotation * Quaternion.Euler(0, rotationAmount, 0));
    }

    void StartDrift()
    {
        if (isDrifting) return;
        isDrifting = true;
        driftStartTime = Time.time;
        wheelController.SetSkidMarkActive(true);
    }

    void ExtendDrift()
    {
        if (isDrifting)
        {
            driftStartTime += driftExtendTime;
        }
    }

    void EndDrift()
    {
        if (!isDrifting) return;
        isDrifting = false;
        lastDriftEndTime = Time.time;
        hasCuttingDriftCondition = false;
        wheelController.SetSkidMarkActive(false);
        StartCoroutine(SmoothSpeedRecovery());
    }

    IEnumerator SmoothSpeedRecovery()
    {
        float elapsedTime = 0;
        float startSpeed = currentSpeed;
        float targetSpeed = startSpeed * (1 - driftSpeedReduction);
        while (elapsedTime < 0.3f)
        {
            elapsedTime += Time.deltaTime;
            currentSpeed = Mathf.Lerp(startSpeed, targetSpeed, elapsedTime / 0.3f);
            yield return null;
        }
    }

    bool IsOppositeInput()
    {
        return (steeringDirection > 0 && Input.GetKey(KeyCode.LeftArrow)) ||
               (steeringDirection < 0 && Input.GetKey(KeyCode.RightArrow));
    }
}
