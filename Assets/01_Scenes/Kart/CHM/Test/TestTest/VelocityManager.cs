using UnityEngine;

public class VelocityManager : MonoBehaviour
{
    [Header("Speed Settings")]
    public float currentSpeed = 0f;
    public float maxSpeed = 200f;
    public float acceleration = 10f;
    public float deceleration = 5f;
    public float friction = 0.1f;

    public void UpdateSpeed(float input, float deltaTime, bool isDrifting)
    {
        // 가속 처리
        if (input != 0)
        {
            currentSpeed += acceleration * input * deltaTime;
        }
        else
        {
            // 감속 및 마찰 적용
            if (currentSpeed > 0) // 전진 상태
            {
                currentSpeed -= deceleration * deltaTime + friction * deltaTime;
                currentSpeed = Mathf.Max(currentSpeed, 0f); // 0 이하로 떨어지지 않게 제한
            }
            else if (currentSpeed < 0) // 후진 상태
            {
                currentSpeed += deceleration * deltaTime + friction * deltaTime;
                currentSpeed = Mathf.Min(currentSpeed, 0f); // 0 이상으로 올라가지 않게 제한
            }
        }

        // 드리프트 시 속도 감소
        if (isDrifting)
        {
            currentSpeed *= 0.8f;
        }

        // 전진/후진의 속도 제한
        currentSpeed = Mathf.Clamp(currentSpeed, -maxSpeed / 2, maxSpeed);
    }

    public float GetCurrentSpeed()
    {
        return currentSpeed;
    }
}