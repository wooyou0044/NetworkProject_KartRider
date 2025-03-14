using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KartController : MonoBehaviour
{
    [SerializeField] GameObject wheels;
    [SerializeField] GameObject carBody;

    WheelController wheelCtrl;
    Rigidbody rigid;
    Rigidbody carBodyRb;

    private float motorInput; // 모터 입력 값
    private float steerInput; // 조향 입력 값

    private void Awake()
    {
        wheelCtrl = wheels.GetComponent<WheelController>();
        rigid = GetComponent<Rigidbody>();
        carBodyRb = carBody.GetComponent<Rigidbody>();
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
            //FollowCarbodyFromWheel();
        }
        //FollowCarbodyFromWheel();
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
        Vector3 centerPos = (wheelCtrl.wheels[0].position + wheelCtrl.wheels[1].position + wheelCtrl.wheels[2].position + wheelCtrl.wheels[3].position) / 4;
        Quaternion wheelRot = Quaternion.Lerp(wheelCtrl.wheels[0].rotation, wheelCtrl.wheels[2].rotation, 0.5f);
        carBody.transform.position = Vector3.Lerp(carBody.transform.position, centerPos, Time.deltaTime* 10f);
        //carBody.transform.rotation = Quaternion.Lerp(carBody.transform.rotation, wheelRot, Time.deltaTime * 10f);
    }
}
