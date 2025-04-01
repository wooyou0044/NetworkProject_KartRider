using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

public class PlayerPanel : MonoBehaviourPun
{
    [Header("Player 정보")]
    [SerializeField] public Image playerImg;
    [SerializeField] public TMP_Text PlayerNameText;
    [SerializeField] public Image playerIcon;

    [Header("준비 완료 이미지")]
    [SerializeField] public Image readyImage;

    [SerializeField] private RoomManager roomManager;
    private void Start()
    {
        roomManager = GameObject.Find("RoomManager").GetComponent<RoomManager>();
        if (photonView.IsMine)
        {
            GetComponent<PhotonView>().RPC("SetOwnInfo", RpcTarget.AllBuffered);
        }
    }
    [PunRPC]
    public void SetOwnInfo()
    {
        if(roomManager == null)
        {
            roomManager = GameObject.Find("RoomManager").GetComponent<RoomManager>();
        }
        PlayerNameText.text = photonView.Controller.NickName;
        for (int i = 0; i < roomManager.playerSlots.Length; i++)
        {
            if (roomManager.playerSlots[i].playerPanel == null)
            {
                roomManager.playerSlots[i].playerPanel = GetComponent<PlayerPanel>();
                roomManager.playerSlots[i].actorNumber = photonView.Owner.ActorNumber;
                roomManager.roomUIManger.startBtn.onClick.AddListener(roomManager.playerSlots[i].playerPanel.BtnClickTrigger);
                roomManager.playerSlots[i].isReady = false;
                transform.SetParent(roomManager.playerSlots[i].transform);
                roomManager.UpdateAllPlayersReady();
                break;
            }
        }
        GetComponent<RectTransform>().anchorMin = Vector3.zero;
        GetComponent<RectTransform>().anchorMax = Vector3.one;
        GetComponent<RectTransform>().localPosition = Vector3.zero;
    }
    public void BtnClickTrigger()
    {        
        if (photonView.IsMine && !PhotonNetwork.IsMasterClient)
        {
            GetComponent<PhotonView>().RPC("SetReady", RpcTarget.AllBuffered);
        }
    }
    [PunRPC]
    public void SetReady()
    {
        //부모객체에서 찾기
        Transform parentTransform = transform.parent;
        RoomManager roomManager = GameObject.Find("RoomManager").GetComponent<RoomManager>();
        if (parentTransform != null)
        {
            //부모의 컴포넌트 가져오기
            PlayerSlot parentSlot = parentTransform.GetComponent<PlayerSlot>();
            if (parentSlot != null)
            {
                if (!readyImage.gameObject.activeSelf)
                {
                    parentSlot.isReady = true;
                    readyImage.gameObject.SetActive(true);
                    roomManager.UpdateAllPlayersReady();
                }
                else
                {
                    parentSlot.isReady = false;
                    roomManager.UpdateAllPlayersReady();
                    readyImage.gameObject.SetActive(false);
                }
            }
        }
    }
}