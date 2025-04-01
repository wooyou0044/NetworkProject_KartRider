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

    bool allReady = false;
    private bool isReadyTextChange = false;
    private WaitForSeconds WFownS = new WaitForSeconds(1f); //1초 대기 코루틴에서 사용
    private WaitForSeconds WFthreeS = new WaitForSeconds(3f); //1초 대기 코루틴에서 사용
    private bool isCorRunning = false;

    private IEnumerator Start()
    {
        yield return new WaitUntil(() => PhotonNetwork.IsConnectedAndReady);
        PhotonNetwork.AutomaticallySyncScene = true;
        RoomInfoUpdate();
        JoinRoom();
        InitializeUI();
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            foreach (var slot in playerSlots)
            {
                if(slot.actorNumber == newMasterClient.ActorNumber)
                {
                    slot.playerPanel.readyImage.gameObject.SetActive(false);
                }
            }
            roomUIManger.startBtnText.text = "게임 시작";
            roomUIManger.startBtn.image.color = new Color(1, 1, 1, 0.5f);
        }
    }
    private void InitializeUI()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            roomUIManger.startBtnText.text = "게임 시작";
            roomUIManger.startBtn.image.color = new Color(1, 1, 1, 0.5f);
        }
        else
        {
            isReadyTextChange = true;
            roomUIManger.startBtnText.text = "준비 완료";
        }
    }
    public void JoinRoom()
    {
        var playerPanel = PhotonNetwork.Instantiate("PlayerPanelPrefab", transform.position, Quaternion.identity);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {        
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        foreach(var slot in playerSlots)
        {
            if (slot.actorNumber == otherPlayer.ActorNumber)
            {
                slot.isReady = false;
                slot.actorNumber = 0;
                slot.playerPanel = null;
            }            
        }
        UpdateAllPlayersReady();
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
            { "Password", roomPassword},
            { "IsGameStart", false }
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(newRoomInfo);
    }
    public void SetRoomMapChange()
    {
        roomUIManger.trackSelectPanel.gameObject.SetActive(false);
        string roomMap = roomUIManger.roomMapNameText.text;
        Hashtable newMapInfo = new Hashtable
        {
            { "Map", roomMap }
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(newMapInfo);
    }
    public void RoomInfoUpdate()
    {
        Hashtable roomProperties = PhotonNetwork.CurrentRoom.CustomProperties;
        roomUIManger.roomNameText.text = roomProperties.ContainsKey("RoomName") ? (string)roomProperties["RoomName"] : "방 이름 없음";
        roomUIManger.roomNumberText.text = roomProperties.ContainsKey("RoomNumber") ? (string)roomProperties["RoomNumber"] : "방 번호 없음";
        roomUIManger.roomMapNameText.text = roomProperties.ContainsKey("Map") ? (string)roomProperties["Map"]
        : "default"; ;
        var mapSprite = Resources.Load<Sprite>($"Maps/{roomUIManger.roomMapNameText.text}");
        roomUIManger.roomMapeImg.sprite = mapSprite != null ? mapSprite : Resources.Load<Sprite>($"Maps/{roomUIManger.roomMapNameText.text}");

        bool hasPassword = roomProperties.ContainsKey("Password") && !string.IsNullOrEmpty((string)roomProperties["Password"]);
        roomUIManger.roomPasswordText.text = hasPassword ? (string)roomProperties["Password"] : null;
        roomUIManger.SetPasswordUI(hasPassword);
    }

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (propertiesThatChanged.ContainsKey("AllPlayersReady"))
            {                
                roomUIManger.startBtn.image.color = new Color(1, 1, 1, 1f);
            }
            else
            {
                roomUIManger.startBtn.image.color = new Color(1, 1, 1, 0.5f);
            }
        }
        RoomInfoUpdate(); //변경된 방 속성을 룸에 반영
    }
    public override void OnLobbyStatisticsUpdate(List<TypedLobbyInfo> lobbyStatistics)
    {
        RoomInfoUpdate();
    }

    //인 게임으로 씬이동
    public void StartGameBtn()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (PhotonNetwork.CurrentRoom.PlayerCount < 2)
            {
                if (!isCorRunning)
                {
                    string massage = "혼자서는 게임을 시작할 수 없습니다. 다른 레이서를 기다려주세요.";
                    StartCoroutine(progressMessageCor(massage));
                }
                return;
            }
            if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("AllPlayersReady") &&
    (bool)PhotonNetwork.CurrentRoom.CustomProperties["AllPlayersReady"])
            {
                Hashtable gameStart = new Hashtable
                {
                    {"IsGameStart", true }
                };
                PhotonNetwork.CurrentRoom.SetCustomProperties(gameStart);
                PhotonNetwork.CurrentRoom.IsOpen = false;
                PhotonNetwork.LoadLevel(roomUIManger.roomMapNameText.text);
            }
            else
            {
                if (!isCorRunning)
                {
                    string massage = "모든 레이서가 준비가 되지 않아 게임을 시작할 수 없습니다.";
                    StartCoroutine(progressMessageCor(massage));
                }
            }
        }
        else
        {
            StartCoroutine(ReadyBtnEnable());
            if (isReadyTextChange)
            {
                isReadyTextChange = false;
                roomUIManger.startBtnText.text = "준비 취소";                
            }
            else
            {
                isReadyTextChange = true;
                roomUIManger.startBtnText.text = "준비 완료";
            }
        }
    }
    IEnumerator ReadyBtnEnable()
    {
        roomUIManger.startBtn.interactable = false;
        yield return WFownS;
        roomUIManger.startBtn.interactable = true;
    }
    IEnumerator progressMessageCor(string massage)
    {
        isCorRunning = true;
        roomUIManger.progressMessagePanel.gameObject.SetActive(true);
        roomUIManger.progressMessage(massage);
        yield return WFthreeS;
        roomUIManger.progressMessage("");
        roomUIManger.progressMessagePanel.gameObject.SetActive(false);
        isCorRunning = false;
    }

    public bool AllPlayersReady()
    {
        foreach (var slot in playerSlots)
        {
            if(slot.playerPanel != null && !PhotonNetwork.IsMasterClient)
            {
                if (slot.actorNumber > 0 && slot.isReady == false)
                {
                    return false; //준비되지 않은 플레이어가 있으면 false 반환                
                }
            }
        }
        return true; //모든 슬롯이 준비 완료
    }

    public void UpdateAllPlayersReady()
    {        
        if(PhotonNetwork.IsMasterClient)
        {
            allReady = true;
            foreach (var slot in playerSlots)
            {
                if (slot.actorNumber > 0 && slot.isReady == false && !slot.playerPanel.photonView.Owner.IsMasterClient)
                {
                    allReady = false;
                    break;
                }                
            }
        }
        Hashtable start = new Hashtable
        {
            {"AllPlayersReady", allReady}
        };
        Debug.Log(allReady+ "최종 단계 게임시작 가능?");
        PhotonNetwork.CurrentRoom.SetCustomProperties(start);
    }
}