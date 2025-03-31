using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

public class PlayerPanel : MonoBehaviourPun
{
    [Header("Player ����")]
    [SerializeField] public Image playerImg;
    [SerializeField] public TMP_Text PlayerNameText;
    [SerializeField] public Image playerIcon;
    
    [Header("�غ� ��ư")]
    [SerializeField] private Button startBtn;
    [SerializeField] private Button readyBtn;
    [SerializeField] private Button readyCanCelBtn;

    [Header("�غ� �Ϸ� �̹���")]
    [SerializeField] private Image readyImage;

    private void Start()
    {
        startBtn = GameObject.Find("StartBtn").GetComponent<Button>();
        readyBtn = GameObject.Find("ReadyBtn").GetComponent<Button>();
        readyCanCelBtn = GameObject.Find("ReadyCanCelBtn").GetComponent<Button>();
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
                //readyImage.gameObject.SetActive(false);
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
            startBtn.gameObject.SetActive(true);
            readyBtn.gameObject.SetActive(false);
            readyCanCelBtn.gameObject.SetActive(false);
            startBtn.interactable = true;
        }
        else
        {
            startBtn.gameObject.SetActive(false);
            readyBtn.gameObject.SetActive(true);
            readyCanCelBtn.gameObject.SetActive(false);
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
