using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KartUIController : MonoBehaviour
{
    [SerializeField] GameObject kart;
    [SerializeField] GameObject speedDashBoard;
    [SerializeField] Transform needle;
    [SerializeField] GameObject circleBack;
    [SerializeField] Image speedCircle;
    [SerializeField] GameObject[] boostChangeUI;
    [SerializeField] Color boostColor;
    [SerializeField] Color originColor;
    [SerializeField] float[] colorAlpha;

    public Text speedTxt;

    TestCHMKart kartCtrl;
    Animator speedAni;

    float kartSpeed;

    bool isChange;

    void Start()
    {
        isChange = false;
        circleBack.SetActive(false);
        speedAni = speedDashBoard.GetComponent<Animator>();
        speedAni.enabled = false;
    }

    void Update()
    {
        if (kartCtrl != null)
        {
            kartSpeed = kartCtrl.speedKM*2f;
            kartSpeed = Mathf.FloorToInt(kartSpeed);
            speedTxt.text = kartSpeed.ToString("f0");
            speedCircle.fillAmount = kartSpeed * 0.0025f;            
        }

        if(kartSpeed < 300.0f)
        {
            needle.rotation = Quaternion.Euler(0, 0, 135 - kartSpeed * 0.9f);
        }

        if (kartCtrl != null && kartCtrl.isBoostTriggered)
        {
            isChange = false;
            StartCoroutine(ChangeSpeedUIOff());
        }

        if (kartCtrl != null && isChange == false)
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
        speedAni.enabled = isBoost;
    }

    IEnumerator ChangeSpeedUIOff()
    {
        yield return new WaitForSeconds(kartCtrl.boostDuration);
        isChange = false;
    }

    // needle µ¹¸®±â
    void RotateSpeedNeedle()
    {

    }

    public void SetKart(GameObject instance)
    {
        kart = instance;
        kartCtrl = kart.GetComponent<TestCHMKart>();
    }
}
