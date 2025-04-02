using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using UnityEngine;

public class ItemNetController : MonoBehaviour
{
    // 해당 순위 게임 오브젝트 반환하는 Script
    [SerializeField] RankUIController rankCtrl;
    [SerializeField] MapManager mapManager;

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

    //public void RequestBarricade(int kartViewID)
    //{
    //    PhotonView photonView = PhotonView.Find(kartViewID);
    //    if (photonView != null)
    //    {
    //        GameObject kartObject = photonView.gameObject;

    //        GameObject firstKart = rankCtrl.GetKartObjectByRank(1);
    //        TestCHMKart firstKartCtrl = firstKart.GetComponent<TestCHMKart>();
    //        if (firstKart != kartObject && firstKartCtrl.isUsingShield == false)
    //        {
    //            PhotonView view = firstKart.GetPhotonView();
    //            view.RPC("MakeBarricade", RpcTarget.MasterClient, firstKart.GetPhotonView().ViewID);
    //        }
    //    }
    //}

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

    public GameObject GetFirstKartObject()
    {
        return rankCtrl.GetKartObjectByRank(1);
    }

    public void RequestWaterFly(int kartViewID, int kartRank, float waterFlyBombTime)
    {
        if(kartRank <= 1)
        {
            return;
        }
        PhotonView photonView = PhotonView.Find(kartViewID);
        if (photonView != null)
        {
            GameObject kartObject = photonView.gameObject;

            GameObject frontKart = rankCtrl.GetKartObjectByRank(kartRank - 1);
            TestCHMKart frontKartCtrl = frontKart.GetComponent<TestCHMKart>();
            if(frontKart == null || frontKart == kartObject)
            {
                return;
            }
            if(frontKart != kartObject && frontKartCtrl.isUsingShield == false)
            {
                PhotonView view = frontKart.GetPhotonView();
                view.RPC("StuckInWaterFly", RpcTarget.All);

                double exitTime = PhotonNetwork.Time + waterFlyBombTime;
                StartExitWaterFlyForAll(view.ViewID, exitTime);
            }
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

    public void StartExitWaterFlyForAll(int kartViewID, double exitTime)
    {
        PhotonView photonView = PhotonView.Find(kartViewID);
        if(photonView == null)
        {
            return;
        }
        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(ExitWaterFlyForAll(photonView, exitTime));
        }
        else
        {
            curPhotonView.RPC("RequestExitWaterFly", RpcTarget.MasterClient, kartViewID, exitTime);
        }
    }

    [PunRPC]
    void RequestExitWaterFly(int kartViewID, double exitTime)
    {
        StartExitWaterFlyForAll(kartViewID, exitTime);
    }

    IEnumerator ExitWaterFlyForAll(PhotonView targetView, double exitTime)
    {
        double currentTime;
        do
        {
            currentTime = PhotonNetwork.Time;
            yield return null;
        }
        while (currentTime < exitTime);

        if(targetView != null)
        {
            targetView.RPC("ExitInWaterFly", RpcTarget.All);
        }
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
