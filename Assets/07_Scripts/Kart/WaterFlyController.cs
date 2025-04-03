using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WaterFlyController : MonoBehaviour
{
    [SerializeField] ItemNetController itemNetCtrl;
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

    public int getExitPhotonViewID { get; set; }

    void Start()
    {
        hitNumText = hitNumBackImage.GetComponentInChildren<Text>();
        rightArrowText = rightArrowImage.GetComponentInChildren<Text>();
        leftArrowText = leftArrowImage.GetComponentInChildren<Text>();

        isRight = false;
    }

    void Update()
    {
        elapsedTime += Time.deltaTime;
        timeGageImage.fillAmount = 1 - (elapsedTime / exitTimer);
        //currentDigit = ((int)elapsedTime % 10) + pressNum;
        currentDigit = (int)elapsedTime + pressNum;

        hitGageImge.fillAmount = 1 - (currentDigit / exitTimer);
        // 물파리에서 빠져나왔을 때도 초기화 필요
        if(currentDigit >= exitTimer)
        {
            hitNumBackImage.transform.localScale = new Vector3(1, 1, 1);

            hitNumText.text = "0";
            //ResetTimer();
            //ItemNetController에 보내야 함
            itemNetCtrl.ExitWaterFlyForAll(getExitPhotonViewID);
        }
        else
        {
            if (currentDigit != lastDigit)
            {
                hitNumText.text = (exitTimer - currentDigit).ToString();
                lastDigit = currentDigit;
                hitNumBackImage.transform.localScale = new Vector3(1, 1, 1);
            }
            else if (hitNumBackImage.transform.localScale.x > 1)
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
            if(isRight == false && currentDigit < exitTimer)
            {
                rightArrowImage.color = Color.white;
                rightArrowText.color = Color.black;

                leftArrowImage.color = Color.black;
                leftArrowText.color = Color.white;
                pressNum++;
                isRight = true;
                hitNumBackImage.transform.localScale = new Vector3(1.5f, 1.5f, 0);
            }
        }
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            if(isRight == true && currentDigit < exitTimer)
            {
                leftArrowImage.color = Color.white;
                leftArrowText.color = Color.black;

                rightArrowImage.color = Color.black;
                rightArrowText.color = Color.white;
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
        isRight = false;

        timeGageImage.fillAmount = 1;
        hitGageImge.fillAmount = 1;
    }
}
