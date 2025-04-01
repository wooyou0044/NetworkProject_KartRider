using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using UnityEngine;

public class ItemNetController : MonoBehaviour
{
    // 해당 순위 게임 오브젝트 반환하는 Script
    [SerializeField] RankUIController rankCtrl;

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
        if(photonView != null)
        {
            GameObject kartObject = photonView.gameObject;

            GameObject firstKart = rankCtrl.GetKartObjectByRank(1);
            
            if (firstKart != kartObject)
            {
                PhotonView view = firstKart.GetPhotonView();
                view.RPC("MakeBarricade", RpcTarget.All);
            }
        }
    }

    public void RequestWaterFly(int kartViewID, int kartRank)
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
            if(frontKart == null || frontKart == kartObject)
            {
                return;
            }
            PhotonView view = frontKart.GetPhotonView();
            view.RPC("StuckInWaterFly", RpcTarget.All);
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
}
