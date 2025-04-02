using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WaterFlyController : MonoBehaviour
{
    [SerializeField] Image leftArrowImage;
    [SerializeField] Image rightArrowImage;
    [SerializeField] Image hitNumBackImage;
    [SerializeField] Image timeGageImage;
    [SerializeField] Image hitGageImge;
    [SerializeField] float exitTimer;

    Text leftArrowText;
    Text rightArrowText;

    Text hitNumText;

    float elapsedTime;
    int lastDigit = -1;
    int currentDigit = 0;

    int pressNum;

    bool isRight;

    void Start()
    {
        hitNumText = hitNumBackImage.GetComponentInChildren<Text>();
        rightArrowText = rightArrowImage.GetComponentInChildren<Text>();
        leftArrowText = leftArrowImage.GetComponentInChildren<Text>();
    }

    void Update()
    {
        elapsedTime += Time.deltaTime;
        timeGageImage.fillAmount = 1 - (elapsedTime / exitTimer);
        currentDigit = ((int)elapsedTime % 10) + pressNum;
        hitGageImge.fillAmount = 1 - (currentDigit / exitTimer);
        // 물파리에서 빠져나왔을 때도 초기화 필요
        if(currentDigit >= exitTimer)
        {
            ResetTimer();
        }
        else if(currentDigit != lastDigit)
        {
            hitNumText.text = (exitTimer - currentDigit).ToString();
            lastDigit = currentDigit;
            hitNumBackImage.transform.localScale = new Vector3(1, 1, 1);
        }
        else
        {
            if(hitNumBackImage.transform.localScale.x > 1)
            {
                hitNumBackImage.transform.localScale -= new Vector3(Time.deltaTime, Time.deltaTime, 0);
            }
        }
        //else
        //{
        //    hitNumBackImage.transform.localScale += new Vector3(Time.deltaTime, Time.deltaTime, 0);
        //}


        if(Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            if(isRight == false)
            {
                rightArrowImage.color = Color.black;
                rightArrowText.color = Color.white;

                leftArrowImage.color = Color.white;
                leftArrowText.color = Color.black;
                pressNum++;
                isRight = true;
                hitNumBackImage.transform.localScale = new Vector3(1.5f, 1.5f, 0);
            }
        }
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            if(isRight == true)
            {
                leftArrowImage.color = Color.black;
                leftArrowText.color = Color.white;

                rightArrowImage.color = Color.white;
                rightArrowText.color = Color.black;
                pressNum++;
                isRight = false;
                hitNumBackImage.transform.localScale = new Vector3(1.5f, 1.5f, 0);
            }
        }
    }

    public void ResetTimer()
    {
        elapsedTime = 0;
        currentDigit = 0;
        pressNum = 0;
        lastDigit = -1;

        timeGageImage.fillAmount = 1;
        hitGageImge.fillAmount = 1;
    }
}
