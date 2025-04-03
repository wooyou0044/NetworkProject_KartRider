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
    //[SerializeField] Sprite itemImg;

    [SerializeField] GameObject boostSpeed;

    TestCHMKart kartCtrl;
    KartInventory kartInventory;

    bool isItemUseNotYet;
    float blinkSpeed = 0.5f;

    Animator boostAni;
    Image boostSpeedImg;

    Sprite itemImg;

    void Start()
    {
        //itemCount = 0;

        boostAni = boostSpeed.GetComponent<Animator>();
        boostSpeedImg = boostSpeed.GetComponent<Image>();
        boostAni.enabled = false;
        boostSpeedImg.enabled = false;
    }

    void Update()
    {
        if (kartCtrl != null)
        {
            if(kartInventory == null)
            {
                kartInventory = kartCtrl.gameObject.GetComponent<KartInventory>();
            }

            if (kartCtrl.isRacingStart == true)
            {
                driftDurationImg.fillAmount = (kartCtrl.boostGauge / kartCtrl.maxBoostGauge);
            }
            if (kartInventory.isItemCreate && kartInventory.itemNum <= 2)
            {
                MakeInventoryItemSlot();
                kartInventory.isItemCreate = false;
                StopCoroutine(ItemUseNotYet());
            }
            
            if(kartCtrl.isItemUsed == true)
            {
                if(kartInventory.itemNum == 0)
                {
                    itemInventImg[0].GetComponent<Image>().color = new Color(0, 0, 0, 0.6156863f);
                    itemInventImg[kartInventory.itemNum].GetChild(0).gameObject.SetActive(false);
                    isItemUseNotYet = false;
                }
                else
                {
                    itemInventImg[kartInventory.itemNum].GetChild(0).gameObject.SetActive(false);
                    itemImg = kartInventory.GetItemImage();
                    itemInventImg[kartInventory.itemNum - 1].GetChild(0).GetComponent<Image>().sprite = itemImg;
                }
                //kartInventory.RemoveItem();
                kartCtrl.isItemUsed = false;
            }

            if (kartCtrl != null && kartCtrl.isBoostTriggered == true)
            {
                boostAni.enabled = true;
                boostSpeedImg.enabled = true;
            }
            if (kartCtrl != null && kartCtrl.isBoostTriggered == false)
            {
                boostAni.enabled = false;
                boostSpeedImg.enabled = false;
            }

        }

        // 나중에 부스트 뿐만 아니라 아이템이 들어오면 조정 필요
        //if (kartCtrl != null && kartCtrl.isBoostCreate && kartCtrl.boostCount <= 2)
        //{
        //    MakeInventoryItemSlot();
        //    kartCtrl.isBoostCreate = false;
        //    StopCoroutine(ItemUseNotYet());
        //}

        // 임시로
        //if (kartCtrl != null && kartCtrl.isBoostUsed == true)
        //{
        //    // 맨 앞 슬롯에 들어있는 이미지 없애기
        //    // 작은 슬롯에 있는 이미지 맨 앞 슬롯에 넣기
        //    if (kartCtrl.boostCount == 0)
        //    {
        //        itemInventImg[0].GetComponent<Image>().color = new Color(0, 0, 0, 0.6156863f);
        //        itemInventImg[kartCtrl.boostCount].GetChild(0).gameObject.SetActive(false);
        //        isItemUseNotYet = false;
        //    }
        //    else
        //    {
        //        itemInventImg[kartCtrl.boostCount].GetChild(0).gameObject.SetActive(false);
        //        itemInventImg[kartCtrl.boostCount - 1].GetChild(0).GetComponent<Image>().sprite = itemImg;
        //    }

        //    kartCtrl.isBoostUsed = false;
        //}

        //if (kartCtrl != null && kartCtrl.isBoostTriggered == true)
        //{
        //    boostAni.enabled = true;
        //    boostSpeedImg.enabled = true;
        //}
        //if (kartCtrl != null && kartCtrl.isBoostTriggered == false)
        //{
        //    boostAni.enabled = false;
        //    boostSpeedImg.enabled = false;
        //}
    }

    void MakeInventoryItemSlot()
    {
        //Image slot = null;
        //if (itemInventImg[kartCtrl.boostCount - 1].childCount != 0)
        //{
        //    itemInventImg[kartCtrl.boostCount - 1].GetChild(0).gameObject.SetActive(true);
        //    slot = itemInventImg[kartCtrl.boostCount - 1].GetChild(0).GetComponent<Image>();
        //}
        //else
        //{
        //    slot = Instantiate(itemImgSlot, itemInventImg[kartCtrl.boostCount - 1]);
        //}

        //slot.sprite = itemImg;

        //isItemUseNotYet = true;

        //if(kartCtrl.boostCount == 1)
        //{
        //    itemInventImg[0].GetComponent<Image>().color = new Color(1, 1, 1, 0.3686275f);
        //    StartCoroutine(ItemUseNotYet());
        //}

        Image slot = null;
        if (itemInventImg[kartInventory.itemNum - 1].childCount != 0)
        {
            itemInventImg[kartInventory.itemNum - 1].GetChild(0).gameObject.SetActive(true);
            slot = itemInventImg[kartInventory.itemNum - 1].GetChild(0).GetComponent<Image>();
        }
        else
        {
            slot = Instantiate(itemImgSlot, itemInventImg[kartInventory.itemNum - 1]);
        }

        itemImg = kartInventory.haveItem[kartInventory.itemNum - 1].itemImage;
        slot.sprite = itemImg;

        isItemUseNotYet = true;

        if (kartInventory.itemNum == 1)
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
