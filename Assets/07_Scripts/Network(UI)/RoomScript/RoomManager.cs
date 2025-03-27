using Hashtable = ExitGames.Client.Photon.Hashtable;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;

public class RoomManager : MonoBehaviourPunCallbacks
{
    [SerializeField] public RoomUIManager roomUIManger;
    [SerializeField] public PlayerSlot[] playerSlots;
    
    private RoomEntry roomEntry;
    private PhotonView photonView;

    private IEnumerator Start()
    {
        photonView = GetComponent<PhotonView>();
        yield return new WaitUntil(() => PhotonNetwork.IsConnectedAndReady);
        RoomInfoUpdate();
        JoinRoom();
        InitializeUI();
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

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        newMasterClient = PhotonNetwork.MasterClient;
        if (PhotonNetwork.IsMasterClient)
        {
            //ResetAndBroadcastUI(); // 새로운 방장이 슬롯을 초기화
        }
    }
    public void JoinRoom()
    {
        var a = PhotonNetwork.Instantiate("PlayerPanelPrefab", transform.position, Quaternion.identity);        
    }
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        //플레이어 입장시 처리 해야할 것들
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        
    }
    
    public void OutRoomBtn()
    {
        if (PhotonNetwork.InRoom)
        {            
            StartCoroutine(LoadJoinLobby("LobbyScene"));
        }
    }
    public override void OnLeftRoom()
    {        
        SceneCont.Instance.Oper.allowSceneActivation = true;
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
    public void StartGame()
    {
        if(PhotonNetwork.IsMasterClient)
        {
            //인 게임으로 씬이동
            Hashtable roomProperties = PhotonNetwork.CurrentRoom.CustomProperties;
            
            PhotonNetwork.LoadLevel("DaisyCircuit");
        }
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
