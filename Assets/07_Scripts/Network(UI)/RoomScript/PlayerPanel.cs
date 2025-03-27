using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

public class PlayerPanel : MonoBehaviourPun
{
    [SerializeField] public Image playerImg;
    [SerializeField] public TMP_Text PlayerNameText;
    [SerializeField] public Image playerIcon;

    private void Start()
    {
        if (photonView.IsMine)
        {
            GetComponent<PhotonView>().RPC("SetOwnInfo", RpcTarget.AllBuffered);
        }
    }

    [PunRPC]
    public void SetOwnInfo()
    {
        PlayerNameText.text = photonView.Controller.NickName;
        RoomManager roomManager = GameObject.Find("RoomManager").GetComponent<RoomManager>();
        for (int i = 0; i < roomManager.playerSlots.Length; i++) 
        { 
            if(roomManager.playerSlots[i].playerPanel == null)
            {
                roomManager.playerSlots[i].playerPanel = GetComponent<PlayerPanel>();
                transform.SetParent(roomManager.playerSlots[i].transform);
                break;
            }
        }
        GetComponent<RectTransform>().anchorMin = Vector3.zero;
        GetComponent<RectTransform>().anchorMax = Vector3.one;
        GetComponent<RectTransform>().localPosition = Vector3.zero;
    }
}
