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
    
    [Header("준비 버튼")]
    [SerializeField] private Button startBtn;
    [SerializeField] private Button readyBtn;
    [SerializeField] private Button readyCanCelBtn;

    [Header("준비 완료 이미지")]
    [SerializeField] private Image readyImage;

    private void Start()
    {
        if (photonView.IsMine)
        {
            GetComponent<PhotonView>().RPC("SetOwnInfo", RpcTarget.AllBuffered);
        }
        InitializeUI();
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
                readyImage.gameObject.SetActive(false);
                transform.SetParent(roomManager.playerSlots[i].transform);
                break;
            }
        }
        GetComponent<RectTransform>().anchorMin = Vector3.zero;
        GetComponent<RectTransform>().anchorMax = Vector3.one;
        GetComponent<RectTransform>().localPosition = Vector3.zero;
    }

    private void InitializeUI()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            startBtn = GameObject.Find("StartBtn").GetComponent<Button>();
            startBtn.gameObject.SetActive(true);
            startBtn.interactable = true;
        }
        else
        {
            readyBtn = GameObject.Find("StartBtn").GetComponent<Button>();
            readyCanCelBtn = GameObject.Find("StartBtn").GetComponent<Button>();            
            startBtn.gameObject.SetActive(false);
            readyBtn.gameObject.SetActive(true);
        }
    }

    private void BtnClickTrigger()
    {
        if (photonView.IsMine)
        {
            GetComponent<PhotonView>().RPC("SetOwnInfo", RpcTarget.AllBuffered);
        }
    }
}
