using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using UnityEngine;

public class ItemNetController : MonoBehaviour
{
    // 해당 순위 게임 오브젝트 반환하는 Script
    [SerializeField] RankUIController rankCtrl;
    [SerializeField] WaterFlyController waterFlyCtrl;

    List<GameObject> items;
    PhotonView curPhotonView;

    TestCHMKart kartCtrl;

    void Start()
    {
        curPhotonView = GetComponent<PhotonView>();
        items = new List<GameObject>();
    }

    void Update()
    {
        
    }

    public void RegisterItem(GameObject itemPreab)
    {
        items.Add(itemPreab);
    }

    public void RequestBarricade(int kartViewID)
    {
        PhotonView photonView = PhotonView.Find(kartViewID);

        if (photonView != null)
        {
            GameObject kartObject = photonView.gameObject;

            GameObject firstKart = rankCtrl.GetKartObjectByRank(1);
            TestCHMKart firstKartCtrl = firstKart.GetComponent<TestCHMKart>();
            PhotonView firstKartView = firstKart.GetPhotonView();
            if(firstKartView == null)
            {
                return;
            }
            if (firstKart != kartObject && firstKartCtrl.isUsingShield == false)
            {
                firstKartView.RPC("SendCheckPointIndex", firstKartView.Owner);
            }
        }
    }
    
    public void MakeBarricadeSink(float sinkTime)
    {
        StartCoroutine(StartSinkBarricade(sinkTime));
    }

    IEnumerator StartSinkBarricade(float destroyTime)
    {
        yield return new WaitForSeconds(destroyTime);
        curPhotonView.RPC("GoDownBarricadeRPC", RpcTarget.All);
    }

    [PunRPC]
    void GoDownBarricadeRPC()
    {
        int count = 0;

        foreach(GameObject item in items)
        {
            BarricadeController barricadeCtrl = item.GetComponent<BarricadeController>();
            if(barricadeCtrl != null)
            {
                barricadeCtrl.GoDownBarricade();
                count++;

                if(count >= 3)
                {
                    return;
                }
            }
        }
    }

    //public void RequestWaterFly(int kartViewID, int kartRank)
    //{
    //    if (kartRank <= 1)
    //    {
    //        return;
    //    }
    //    PhotonView photonView = PhotonView.Find(kartViewID);
    //    if (photonView != null)
    //    {
    //        GameObject kartObject = photonView.gameObject;

    //        GameObject frontKart = rankCtrl.GetKartObjectByRank(kartRank - 1);
    //        TestCHMKart frontKartCtrl = frontKart.GetComponent<TestCHMKart>();
    //        if (frontKart == null || frontKart == kartObject)
    //        {
    //            return;
    //        }
    //        if (frontKart != kartObject && frontKartCtrl.isUsingShield == false)
    //        {
    //            PhotonView view = frontKart.GetPhotonView();
    //            view.RPC("StuckInWaterFly", RpcTarget.All);

    //            view.RPC("ActiveWaterFlyUI", view.Owner);
    //        }
    //    }
    //}

    public void RequestWaterFly(int kartViewID, int kartRank, float delayTime = 2f)
    {
        if (kartRank <= 1)
        {
            return;
        }
        PhotonView photonView = PhotonView.Find(kartViewID);
        if (photonView != null)
        {
            GameObject kartObject = photonView.gameObject;

            GameObject frontKart = rankCtrl.GetKartObjectByRank(kartRank - 1);
            TestCHMKart frontKartCtrl = frontKart.GetComponent<TestCHMKart>();
            if (frontKart == null || frontKart == kartObject)
            {
                return;
            }
            if (frontKart != kartObject && frontKartCtrl.isUsingShield == false)
            {
                PhotonView view = frontKart.GetPhotonView();

                //view.RPC("StuckInWaterFly", RpcTarget.All);
                //view.RPC("ActiveWaterFlyUI", view.Owner);

                view.RPC("PlayWaterFlyWaringSound", view.Owner);

                if(PhotonNetwork.IsMasterClient)
                {
                    double executeTime = PhotonNetwork.Time + delayTime;
                    StartCoroutine(DelayWaterFly(view, executeTime));
                }
                else
                {
                    curPhotonView.RPC("RequestDelayedWaterFly", RpcTarget.MasterClient, view.ViewID, PhotonNetwork.Time + delayTime);
                }
            }
        }
    }

    [PunRPC]
    void RequestDelayedWaterFly(int viewId, double executeTime)
    {
        PhotonView view = PhotonView.Find(viewId);
        if(view != null)
        {
            StartCoroutine(DelayWaterFly(view, executeTime));
        }
    }

    IEnumerator DelayWaterFly(PhotonView targetView, double executeTime)
    {
        while(PhotonNetwork.Time<executeTime)
        {
            yield return null;
        }
        if(targetView != null)
        {
            targetView.RPC("StuckInWaterFly", RpcTarget.All);
            targetView.RPC("ActiveWaterFlyUI", targetView.Owner);
        }
    }

    [PunRPC]
    public void DisableItem(int index)
    {
        if(index >= 0 && index < items.Count)
        {
            GameObject targetItem = items[index];
            items[index].SetActive(false);
            items.Remove(targetItem);
        }
    }


    public void RequestDisableItem(GameObject hitItem)
    {
        int index = items.IndexOf(hitItem);
        if(index != -1)
        {
            curPhotonView.RPC("DisableItem", RpcTarget.All, index);
        }
    }

    public void ExitWaterFlyForAll(int kartViewID)
    {
        PhotonView view = PhotonView.Find(kartViewID);

        if(view == null)
        {
            return;
        }
        view.RPC("InActiveInWaterFlyUI", view.Owner);
        view.RPC("ExitInWaterFly", RpcTarget.All);
    }


    public GameObject GetLastItem()
    {
        for(int i=items.Count - 1; i>= 0; i--)
        {
            if (items[i] != null)
            {
                ItemManager itemData = items[i].GetComponent<ItemController>()?.item;
                if(itemData != null && itemData.itemType == ItemType.waterFly)
                {
                    return items[i];
                }
            }
        }
        return null;
    }
}
