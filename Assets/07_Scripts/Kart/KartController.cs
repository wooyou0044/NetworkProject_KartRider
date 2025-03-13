using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KartController : MonoBehaviour
{
    [SerializeField] GameObject wheels;

    WheelController wheelCtrl;
    Rigidbody rigid;

    private float motorInput; // 모터 입력 값
    private float steerInput; // 조향 입력 값

    private void Awake()
    {
        wheelCtrl = wheels.GetComponent<WheelController>();
        rigid = GetComponent<Rigidbody>();
    }

    void Start()
    {
        
    }

    void Update()
    {
        steerInput = Input.GetAxis("Horizontal");
        motorInput = Input.GetAxis("Vertical");
        if(steerInput != 0 || motorInput != 0)
        {
            wheelCtrl.SteerAndRotateWheels(steerInput, motorInput);
            wheelCtrl.Move(steerInput, motorInput);
        }
    }

    private void FixedUpdate()
    {
        if (steerInput != 0 || motorInput != 0)
        {
            wheelCtrl.ApplyAntiRollBar();
        }
    }

    void FollowCarbodyFromWheel()
    {
        Vector3 centerPos = wheelCtrl.wheels[0].position + wheelCtrl.wheels[1].position + wheelCtrl.wheels[2].position + wheelCtrl.wheels[3].position / 4;
        transform.position = centerPos;
    }
}
