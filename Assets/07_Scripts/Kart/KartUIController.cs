using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KartUIController : MonoBehaviour
{
    [SerializeField] GameObject kart;
    [SerializeField] GameObject needle;
    [SerializeField] GameObject circleBack;
    [SerializeField] GameObject[] boostChangeUI;
    [SerializeField] Color boostColor;
    [SerializeField] Color originColor;
    [SerializeField] float[] colorAlpha;

    public Text speedTxt;

    KartController kartCtrl;

    float kartSpeed;

    bool isChange;

    void Start()
    {
        kartCtrl = kart.GetComponent<KartController>();

        isChange = false;
        circleBack.SetActive(false);
    }

    void Update()
    {
        kartSpeed = kartCtrl.speedKM * 2;
        speedTxt.text = kartSpeed.ToString("f0");

        if (kartCtrl.isBoostTriggered)
        {
            isChange = false;
            StartCoroutine(ChangeSpeedUIOff());
        }

        if (isChange == false)
        {
            ChangeSpeedUIToBoost(kartCtrl.isBoostTriggered);
            isChange = true;
        }
    }

    void ChangeSpeedUIToBoost(bool isBoost)
    {
        Color changeColor = (isBoost == true) ? boostColor : originColor;
        for (int i = 0; i < boostChangeUI.Length; i++)
        {
            if (boostChangeUI[i].transform.childCount > 0)
            {
                for (int j = 0; j < boostChangeUI[i].transform.childCount; j++)
                {
                    var temp = boostChangeUI[i].transform.GetChild(j).GetComponent<Image>();
                    changeColor.a = colorAlpha[i];
                    temp.color = changeColor;
                }
            }
            else
            {
                changeColor.a = colorAlpha[i];
                boostChangeUI[i].GetComponent<Image>().color = changeColor;
            }
        }
        circleBack.SetActive(isBoost);
    }

    IEnumerator ChangeSpeedUIOff()
    {
        yield return new WaitForSeconds(kartCtrl.boostDuration);
        isChange = false;
    }

    // 속도계 움직이기
    void ShakeUI()
    {

    }

    // needle 돌리기
    void RotateSpeedNeedle()
    {

    }

    // 원 속도에 따라 조절
}
