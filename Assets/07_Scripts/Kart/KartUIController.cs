using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KartUIController : MonoBehaviour
{
    [SerializeField] GameObject kart;

    public Text speedTxt;

    KartController kartCtrl;

    float kartSpeed;

    void Start()
    {
        kartCtrl = kart.GetComponent<KartController>();
    }

    void Update()
    {
        if(kartSpeed > 300.0f)
        {
            kartSpeed = 300;
        }
        kartSpeed = kartCtrl.speedKM * 2;
        speedTxt.text = kartSpeed.ToString("f0");
    }
}
