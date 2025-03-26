using Hashtable = ExitGames.Client.Photon.Hashtable;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System;

public class RoomManager : MonoBehaviourPunCallbacks
{
    [SerializeField] public RoomUIManager roomUIManger;
    [SerializeField] public PlayerSlot[] playerSlots;

    private Player[] players = PhotonNetwork.PlayerList;
    private RoomEntry roomEntry;
    private PhotonView photonView;

    private IEnumerator Start()
    {
        photonView = GetComponent<PhotonView>();
        yield return new WaitUntil(() => PhotonNetwork.IsConnectedAndReady);
        RoomInfoUpdate();
        //AssignLocalPlayerToSlot();
        InitializeUI();
        if(PhotonNetwork.LocalPlayer != null)
        {
            AssignLocalPlayerToSlot();
        }
    }

    private void InitializeUI()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            roomUIManger.startBtn.gameObject.SetActive(true);
            roomUIManger.readyBtn.gameObject.SetActive(false);
            roomUIManger.startBtn.interactable = true;
            
        }
        else
        {
            roomUIManger.startBtn.gameObject.SetActive(false);
            roomUIManger.readyBtn.gameObject.SetActive(true);
        }
    }

    private void AssignLocalPlayerToSlot()
    {
        foreach(var slot in playerSlots)
        {
            if (slot.IsEmpty) // 슬롯이 비어 있는지 확인
            {
                int slotIndex = Array.IndexOf(playerSlots, slot);
                Debug.Log($"{slotIndex}번 슬롯에 할당됨");

                // 로컬에서 슬롯 초기화
                slot.CreatePlayerPanel();
                slot.playerPanel.UpdatePanel(PhotonNetwork.LocalPlayer.NickName);

                // 다른 클라이언트에 슬롯 정보 전송
                photonView.RPC("UpdateSlotForAllClients", RpcTarget.All, slotIndex,PhotonNetwork.LocalPlayer.NickName);
                break;
            }
        }
    }

    [PunRPC]
    public void UpdateSlotForAllClients(int slotIndex, string playerName)
    {
        if (slotIndex >= 0 && slotIndex < playerSlots.Length) // 슬롯 인덱스 유효성 확인
        {
            Debug.Log($"{slotIndex}번 슬롯 업데이트 중");

            // 패널이 이미 생성된 경우 중복 생성 방지
            if (playerSlots[slotIndex].playerPanel == null)
            {
                playerSlots[slotIndex].CreatePlayerPanel();
            }

            // 기존 데이터를 덮어쓰지 않도록 데이터 갱신만 수행
            if (playerSlots[slotIndex].playerPanel.PlayerNameText.text != playerName)
            {
                playerSlots[slotIndex].playerPanel.UpdatePanel(playerName);
            }
        }
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            //ResetAndBroadcastUI(); // 새로운 방장이 슬롯을 초기화
        }

    }
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            // 현재 슬롯 정보를 새로운 플레이어에게 전달
            SyncSlotsForNewPlayer(newPlayer);
        }

    }
    private void SyncSlotsForNewPlayer(Player newPlayer)
    {
        for (int i = 0; i < playerSlots.Length; i++)
        {
            if (!playerSlots[i].IsEmpty) // 비어 있지 않은 슬롯만 전송
            {
                photonView.RPC("UpdateSlotForNewPlayer", newPlayer, i, playerSlots[i].playerPanel.PlayerNameText.text);
            }
        }
    }
    [PunRPC]
    public void UpdateSlotForNewPlayer(int slotIndex, string playerName)
    {
        if (slotIndex >= 0 && slotIndex < playerSlots.Length) // 슬롯 유효성 확인
        {
            if (playerSlots[slotIndex].playerPanel == null) // 패널이 없는 경우 생성
            {
                playerSlots[slotIndex].CreatePlayerPanel();
            }

            // 슬롯에 데이터 업데이트
            playerSlots[slotIndex].playerPanel.UpdatePanel(playerName);
        }
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            //ResetAndBroadcastUI();
        }
    }
    private void ResetAndBroadcastUI()
    {
        foreach (var slot in playerSlots)
        {
            slot.ClearPlayerPanel(); // 모든 슬롯 초기화
        }

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            foreach (var slot in playerSlots)
            {
                if (slot.IsEmpty)
                {
                    slot.AssignPlayer(player.ActorNumber, player.NickName);
                    break;
                }
            }
        }
        //photonView.RPC("UpdateUIForAllClients", RpcTarget.Others, PhotonNetwork.PlayerList);
    }
    [PunRPC]
    public void UpdateUIForAllClients(Player[] players)
    {
        //foreach (var slot in playerSlots)
        //{
        //    slot.ClearPlayerPanel(); // 모든 슬롯 초기화
        //}
        //foreach (var player in players)
        //{
        //    foreach (var slot in playerSlots)
        //    {
        //        if (slot.IsEmpty)
        //        {
        //            slot.AssignPlayer(player.ActorNumber, player.NickName);
        //            break;
        //        }
        //    }
        //}
    }

    public void OutRoomBtn()
    {
        if (PhotonNetwork.InRoom)
        {            
            StartCoroutine(LoadJoinLobby("LobbyScene"));
        }
    }
    IEnumerator LoadJoinLobby(string sceneName)
    {
        PhotonNetwork.LeaveRoom();
        SceneCont.Instance.Oper = SceneCont.Instance.SceneAsync(sceneName);
        SceneCont.Instance.Oper.allowSceneActivation = false;
        while (SceneCont.Instance.Oper.isDone == false)
        {
            if (SceneCont.Instance.Oper.progress < 0.9f)
            {
                //룸 내의 모든 행동 금지시키기.. 로비 이동중... 띄우기
            }
            else
            {
                break;
            }
        }        
        yield break; 
    }
    public void SetRoomInfoChange()
    {
        roomUIManger.roomInfoChangePanel.gameObject.SetActive(false);
        string roomName = roomUIManger.roomNameChangeField.text;
        string roomPassword = roomUIManger.roomPasswordChangeField.text;

        Hashtable newRoomInfo = new Hashtable
        {
            { "RoomName", roomName},
            { "Password", roomPassword}
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(newRoomInfo);
    }
    public void SetRoomMapChange()
    {
        string MapName = roomUIManger.roomMapNameText.text;
        Hashtable newMapInfo = new Hashtable
        {
            { "Map", MapName}
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(newMapInfo);
    }
    public void RoomInfoUpdate()
    {
        Hashtable roomProperties = PhotonNetwork.CurrentRoom.CustomProperties;
        roomUIManger.roomNameText.text = roomProperties.ContainsKey("RoomName") ? (string)roomProperties["RoomName"] : "방 이름 없음";
        roomUIManger.roomNumberText.text = roomProperties.ContainsKey("RoomNumber") ? (string)roomProperties["RoomNumber"] : "방 번호 없음";
        string mapName = roomProperties.ContainsKey("Map") ? (string)roomProperties["Map"]
        : "default";
        
        var mapSprite = Resources.Load<Sprite>($"Maps/{mapName}");
        roomUIManger.roomMapeImg.sprite = mapSprite != null ? mapSprite : Resources.Load<Sprite>("Maps/default");

        bool hasPassword = roomProperties.ContainsKey("Password") && !string.IsNullOrEmpty((string)roomProperties["Password"]);
        roomUIManger.roomPasswordText.text = hasPassword ? (string)roomProperties["Password"] : null;
        roomUIManger.SetPasswordUI(hasPassword);
    }
    
    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        RoomInfoUpdate(); //변경된 방 속성을 룸에 반영        
    }
    public override void OnLobbyStatisticsUpdate(List<TypedLobbyInfo> lobbyStatistics)
    {
        SetRoomInfoChange();

    }
    public override void OnLeftLobby()
    {
        Debug.Log("로비 퇴장했습니다.");
    }
    public void StartGame()
    {
        if(PhotonNetwork.IsMasterClient)
        {
            //인 게임으로 씬이동
            //PhotonNetwork.LoadLevel("InGameScene");
        }
    }
    public override void OnLeftRoom()
    {        
        SceneCont.Instance.Oper.allowSceneActivation = true;
    }        

    public void PlayerBtnController()
    {//준비 상태가 아니라면 준비상태로만들기
        if(roomUIManger.readyBtn.gameObject.activeSelf)
        {
            roomUIManger.readyBtn.gameObject.SetActive(false);
            roomUIManger.readyCanCelBtn.gameObject.SetActive(true);
        }
        else
        {//준비상태라면 준비 취소
            roomUIManger.readyBtn.gameObject.SetActive(true);
            roomUIManger.readyCanCelBtn.gameObject.SetActive(false);
        }
    }    
}
