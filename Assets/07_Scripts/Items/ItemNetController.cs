using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using UnityEngine;

public class ItemNetController : MonoBehaviour
{
    // 해당 순위 게임 오브젝트 반환하는 Script
    [SerializeField] RankUIController rankCtrl;

    PhotonView curPhotonView;

    TestCHMKart kartCtrl;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void RequestBarricade(int kartViewID)
    {
        PhotonView photonView = PhotonView.Find(kartViewID);
        if(photonView != null)
        {
            GameObject kartObject = photonView.gameObject;

            GameObject firstKart = rankCtrl.GetKartObjectByRank(1);
            if(firstKart != null && firstKart == kartObject)
            {
                PhotonView view = firstKart.GetPhotonView();
                view.RPC("MakeBarricade", RpcTarget.All);
            }
        }
    }
}
