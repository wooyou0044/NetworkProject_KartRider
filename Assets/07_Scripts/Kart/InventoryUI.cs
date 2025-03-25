using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class InventoryUI : MonoBehaviour
{
    [SerializeField] GameObject kart;
    [SerializeField] Image driftDurationImg;
    [SerializeField] Transform[] itemInventImg;
    [SerializeField] Image itemImgSlot;
    // 슬롯에 들어갈 아이템 이미지
    [SerializeField] Sprite itemImg;

    TestCHMKart kartCtrl;

    public int itemCount { get; set; }

    bool isItemUseNotYet;
    float blinkSpeed = 0.5f;

    void Start()
    {
        itemCount = 0;
    }

    void Update()
    {
        if (kartCtrl != null)
        {
            driftDurationImg.fillAmount = (kartCtrl.boostGauge / kartCtrl.maxBoostGauge);     
        }

        // 나중에 부스트 뿐만 아니라 아이템이 들어오면 조정 필요
        if(kartCtrl != null && kartCtrl.isBoostCreate && itemCount < 2)
        {
            MakeInventoryItemSlot();
            kartCtrl.isBoostCreate = false;
            StopCoroutine(ItemUseNotYet());
        }

        // 임시로
        if(Input.GetKeyDown(KeyCode.H))
        {
            itemCount--;
            // 맨 앞 슬롯에 들어있는 이미지 없애기
            // 작은 슬롯에 있는 이미지 맨 앞 슬롯에 넣기
            if(itemCount == 0)
            {
                itemInventImg[0].GetComponent<Image>().color = new Color(0, 0, 0, 0.6156863f);
                itemInventImg[itemCount].GetChild(0).gameObject.SetActive(false);
                isItemUseNotYet = false;
            }
            else
            {
                itemInventImg[itemCount].GetChild(0).gameObject.SetActive(false);
                itemInventImg[itemCount - 1].GetChild(0).GetComponent<Image>().sprite = itemImg;
            }
        }
    }

    void MakeInventoryItemSlot()
    {
        Image slot = null;
        if (itemInventImg[itemCount].childCount != 0)
        {
            itemInventImg[itemCount].GetChild(0).gameObject.SetActive(true);
            slot = itemInventImg[itemCount].GetChild(0).GetComponent<Image>();
        }
        else
        {
            slot = Instantiate(itemImgSlot, itemInventImg[itemCount]);
        }

        slot.sprite = itemImg;
        // 컨트롤 키를 누르면 itemCount--;
        itemCount++;

        isItemUseNotYet = true;

        if(itemCount == 1)
        {
            itemInventImg[0].GetComponent<Image>().color = new Color(1, 1, 1, 0.3686275f);
            StartCoroutine(ItemUseNotYet());
        }
    }

    IEnumerator ItemUseNotYet()
    {
        while(isItemUseNotYet)
        {
            Color color = itemInventImg[0].GetComponent<Image>().color;
            color = (color.r == 1) ? new Color(0.6f, 0.6f, 0.6f, 0.3686275f) : new Color(1, 1, 1, 0.3686275f);
            itemInventImg[0].GetComponent<Image>().color = color;
            yield return new WaitForSeconds(blinkSpeed);
        }
    }
    
    public void SetKart(GameObject instance)
    {
        kart = instance;
        kartCtrl = kart.GetComponent<TestCHMKart>();
    }    
}
