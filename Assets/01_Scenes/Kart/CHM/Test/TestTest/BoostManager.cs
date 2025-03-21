using UnityEngine;

public class BoostManager : MonoBehaviour
{
    [Header("Boost Settings")]
    [SerializeField] public float boostSpeed = 280f;
    [SerializeField] public float boostDuration = 1.2f;
    [SerializeField] public int maxBoostGauge = 100;
    [SerializeField] public float boostChargeRate = 1f;
    [SerializeField] public float driftBoostChargeRate = 5f;

    private Rigidbody rigid;
    private int boostGauge = 0;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
    }

    public bool CanBoost()
    {
        return boostGauge >= maxBoostGauge;
    }

    public void StartBoost()
    {
        boostGauge = 0;
        rigid.velocity = transform.forward * boostSpeed;

        Debug.Log("부스트 시작!");
        Invoke(nameof(EndBoost), boostDuration);
    }

    private void EndBoost()
    {
        Debug.Log("부스트 종료.");
    }

    public void ChargeBoostGauge(bool isDrifting)
    {
        int chargeRate = isDrifting ? Mathf.RoundToInt(driftBoostChargeRate * Time.deltaTime) : Mathf.RoundToInt(boostChargeRate * Time.deltaTime);
        boostGauge = Mathf.Clamp(boostGauge + chargeRate, 0, maxBoostGauge);

        if (boostGauge >= maxBoostGauge)
        {
            Debug.Log("부스트 준비 완료!");
        }
    }
}