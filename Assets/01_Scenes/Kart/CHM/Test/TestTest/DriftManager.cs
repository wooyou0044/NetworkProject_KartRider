using UnityEngine;

public class DriftManager : MonoBehaviour
{
    [Header("Drift Settings")]
    [SerializeField] public float driftForceMultiplier = 2f;
    [SerializeField] public float driftSpeedReduction = 0.7f;

    private Rigidbody rigid;
    private bool isDrifting = false;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
    }

    public void StartDrift(float steerInput)
    {
        isDrifting = true;

        // 측면 힘 추가
        Vector3 lateralForce = transform.right * steerInput * driftForceMultiplier;
        rigid.AddForce(lateralForce, ForceMode.Force);

        // 드리프트 시 속도 감소
        rigid.velocity *= driftSpeedReduction;

        Debug.Log("드리프트 시작");
    }

    public void EndDrift()
    {
        isDrifting = false;
        Debug.Log("드리프트 종료");
    }

    public bool IsDrifting()
    {
        return isDrifting;
    }
}