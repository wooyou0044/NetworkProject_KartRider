using Hashtable = ExitGames.Client.Photon.Hashtable;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.EditorTools;

public class RoomManager : MonoBehaviourPunCallbacks
{
    public PlayerSlot[] playerSlots;
    public RoomUIManager roomUIManger;

    private bool allReady = false;
    private bool isReadyTextChange = false;

    private bool isCorRunning = false;
    private WaitForSeconds WFownS = new WaitForSeconds(1f); //1초 대기 코루틴에서 사용
    private WaitForSeconds WFthreeS = new WaitForSeconds(3f); //1초 대기 코루틴에서 사용

    private IEnumerator Start()
    {
        yield return new WaitUntil(() => PhotonNetwork.IsConnectedAndReady);
        PhotonNetwork.AutomaticallySyncScene = true;
        RoomInfoUpdate();
        CreatePlayerPanel();
        ProgressReSet();
        InitializeUI();
    }
    /// <summary>
    /// 포톤네트워크에서 지원하는 콜백중 룸의 마스터(방장)을 바꾸는 메서드
    /// 마스터가 바뀌면 자신의 슬롯을 마스터 액터넘버로 지정한다.
    /// 준비완료 이미지를 끄고 준비완료 버튼을 뺀다.
    /// 준비 완료를 준비 시작으로 바꾼다.
    /// </summary>
    /// <param name="newMasterClient">마스터정보</param>
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            foreach (var slot in playerSlots)
            {
                if(slot.actorNumber == newMasterClient.ActorNumber)
                {
                    roomUIManger.startBtn.onClick.RemoveListener(slot.playerPanel.StartBtnClickTrigger);
                    slot.playerPanel.readyImage.gameObject.SetActive(false);
                }
            }
            roomUIManger.startBtnText.text = "게임 시작";
            roomUIManger.startBtn.image.color = new Color(1, 1, 1, 0.5f);
        }
    }
    /// <summary>
    /// 룸에 접속하면 버튼텍스트를 바꿔 UI적으로 재활용한다.
    /// 방장 : 게임 시작 버튼
    /// 일반 유저 : 준비 완료 버튼
    /// </summary>
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

    /// <summary>
    /// 다시 룸 씬으로 돌아오면 프로그레스를 0으로 초기화 한다.
    /// </summary>
    private void ProgressReSet()
    {
        Hashtable progress = new Hashtable();
        progress["LoadProgress"] = 0.0f;
        PhotonNetwork.LocalPlayer.SetCustomProperties(progress);
    }
    /// <summary>
    /// 슬롯에 넣을 플레이어 판넬을 생성한다.
    /// 포톤네트워크.인스턴세이트로 생성하여 네트워크에 뿌린다.
    /// </summary>
    public void CreatePlayerPanel()
    {
        var playerPanel = PhotonNetwork.Instantiate("PlayerPanelPrefab", transform.position, Quaternion.identity);
    }

    /// <summary>
    /// 포톤네트워크에서 지원하는 플레이어가 방에 들어온 것을 알수 있는 메소드
    /// newPlayer를 통해 방금 들어온 유저를 알 수 있다.
    /// 유저가 들어오면 콜백을 받음
    /// </summary>
    /// <param name="newPlayer">방금 방에 들어온 플레이어</param>
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
    }

    /// <summary>
    /// 포톤네트워크에서 지원하는 플레이어가 방을 나간것을 알 수 있는 메소드
    /// otherPlayer를 통해 방금 나간 플레이어를 알수 있다.
    /// 유저가 나가면 콜백을 받음
    /// 나간 유저의 자리를 정리하는 로직
    /// </summary>
    /// <param name="otherPlayer">방금 방을 떠난 플레이어</param>
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

    /// <summary>
    /// 나가기 버튼과 연결
    /// 나가기 버튼을 누르면 해당 룸을 나가고 로비로 이동한다.
    /// 로딩을 위해 코루틴 실행
    /// </summary>
    public void OutRoomBtn()
    {
        if (PhotonNetwork.InRoom)
        {
            StartCoroutine(LoadJoinLobby("LobbyScene"));
        }
    }

    /// <summary>
    /// 방을 빠져 나간 때 호출되는 콜백
    /// 방을 완전히 빠져 나간 것이 보장받으면 씬을 전환함
    /// </summary>
    public override void OnLeftRoom()
    {
        SceneCont.Instance.Oper.allowSceneActivation = true;
    }

    /// <summary>
    /// 씬 이동 관련 내용을 재활용하기 위한 코루틴
    /// 방을 나가거나, 인게임으로 이동하는 등의 룸안에서 씬이동이 발생했을 때 로딩을 띄움
    /// </summary>
    /// <param name="sceneName">변경할 씬 이름</param>
    /// <returns>요청이 완료 되면 코루틴 탈출</returns>
    IEnumerator LoadJoinLobby(string sceneName)
    {
        PhotonNetwork.LeaveRoom();
        SceneCont.Instance.Oper = SceneCont.Instance.SceneAsync(sceneName);
        SceneCont.Instance.Oper.allowSceneActivation = false;
        while (SceneCont.Instance.Oper.isDone == false)
        {
            if (SceneCont.Instance.Oper.progress < 0.9f)
            {
                
            }
            else
            {
                break;
            }
        }
        yield break;
    }
    /// <summary>
    /// 룸 이름과 룸패스워드를 설정할 수 있는 판넬
    /// 커스텀 프러퍼티를 활용하여 해당 내용을 바꾸고 저장함.
    /// </summary>
    public void SetRoomInfoChangeBtn()
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
    /// <summary>
    /// 방에서 트랙을 변경할 수 있는 버튼
    /// 설정을 누르면 셋팅한 맵으로 설정을 변경함.
    /// </summary>
    public void SetRoomMapChangeBtn()
    {
        roomUIManger.trackSelectPanel.gameObject.SetActive(false);
        string roomMap = roomUIManger.roomMapNameText.text;
        Hashtable newMapInfo = new Hashtable
        {
            { "Map", roomMap }
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(newMapInfo);
    }
    /// <summary>
    /// 커스텀 프러퍼티를 활용하여 방의 변경된 정보를 업데이트한다.
    /// 변경된 정보는 커스텀 프러퍼티를 타고 저장되어 로비에 반영되도록 함
    /// </summary>
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
    /// <summary>
    /// 포톤네트워크에서 제공하는 방에서 프러퍼티의 변경을 콜백받는 메서드
    /// 방에 변경된 내용을 뿌려준다.
    /// </summary>
    /// <param name="propertiesThatChanged">업데이트된 정보</param>
    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {        
        RoomInfoUpdate(); //변경된 방 속성을 룸에 반영
    }
    /// <summary>
    /// 포톤네트워크에서 제공하는 방에서 프러퍼티의 변경되었을 때 로비에서 그 정보를
    /// 콜백받는 메서드
    /// 방에 변경 된 내용을 로비에 뿌려준다.
    /// </summary>
    /// <param name="lobbyStatistics">로비가 많을 경우 또는 지정할 로비를 선택할 수 있음</param>
    public override void OnLobbyStatisticsUpdate(List<TypedLobbyInfo> lobbyStatistics)
    {
        RoomInfoUpdate();
    }

    /// <summary>
    /// 게임 시작, 준비 완료 버튼과 연결 됨
    /// 방장이라면 버튼을 눌렀을 때 방의 인원이 2명이상인지와 모든 플레이어가 준비완료 상태인지
    /// 확인한 뒤 조건에 맞지 않다면 메세지를 갈아낌
    /// 방장이 아닐경우 자신의 화면에서 버튼을 갈아낌
    /// </summary>
    public void StartGameBtn()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            //혼자 있으면 게임 못함
            if (PhotonNetwork.CurrentRoom.PlayerCount < 2)
            {
                if (!isCorRunning)
                {
                    string massage = "혼자서는 게임을 시작할 수 없습니다. 다른 레이서를 기다려주세요.";
                    StartCoroutine(progressMessageCor(massage));
                }
                return;
            }
            //커스턴 프러퍼티를 활용해 불값을 직접 받아 변경이 있다면 바로 업데이트되록 설정
            if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("AllPlayersReady") &&
    (bool)PhotonNetwork.CurrentRoom.CustomProperties["AllPlayersReady"])
            {
                // 게임이 시작되면 IsGameStart를 트루로 바꿔 방진입을 차단한다.
                Hashtable gameStart = new Hashtable
                {
                    {"IsGameStart", true }
                };
                PhotonNetwork.CurrentRoom.SetCustomProperties(gameStart);
                PhotonNetwork.CurrentRoom.IsOpen = false;//게임중 입장을 제한함
                PhotonNetwork.LoadLevel(roomUIManger.roomMapNameText.text);//맵과 같은 씬으로 이동함
            }
            else
            {
                //준비 완료 상태를 체크함
                if (!isCorRunning)
                {
                    string massage = "모든 레이서가 준비가 되지 않아 게임을 시작할 수 없습니다.";
                    StartCoroutine(progressMessageCor(massage));
                }
            }
        }
        else
        {
            //방장이 아닐경우 버튼을 갈아낌
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
    /// <summary>
    /// 버튼 광클 막기
    /// </summary>
    /// <returns>코루틴으로 1초에 한번씩 눌리도록 설정함</returns>
    IEnumerator ReadyBtnEnable()
    {
        roomUIManger.startBtn.interactable = false;
        yield return WFownS;
        roomUIManger.startBtn.interactable = true;
    }
    /// <summary>
    /// 방장에게 알려줄 내용이 있다면 해당 메세지로 갈아끼기 위한 코루틴
    /// 3초동안 지속 됨
    /// </summary>
    /// <param name="massage">출력 메세지</param>
    /// <returns></returns>
    IEnumerator progressMessageCor(string massage)
    {
        isCorRunning = true;//지속 호출을 막기위한 불변수
        roomUIManger.progressMessagePanel.gameObject.SetActive(true);
        roomUIManger.progressMessage(massage);
        yield return WFthreeS;
        roomUIManger.progressMessage("");//호출이 끝나면 메세지 초기화
        roomUIManger.progressMessagePanel.gameObject.SetActive(false);
        isCorRunning = false;
    }
    /// <summary>
    /// 커스텀 프러퍼티에 반환되는 값을 직접 넣기 위한 불변수
    /// 해당 내용이 커스텀프러퍼티를 통해 값이 전달되며 false 또는 true를 반환한다.
    /// </summary>
    /// <returns></returns>
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
    /// <summary>
    /// 커스텀 프러퍼티를 통해 유저들의 준비상태를 확인하는 메서드
    /// 방장은 유저들의 준비완료 상태를 받아 저장한다.
    /// 플레이어가 있는 모든 슬롯의 상태가 준비 된 상태라면 true를 반환하고 저장함
    /// </summary>
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
        //삼항연산자를 통해 버튼색을 바꿈
        //allReady가 true면 색을 진하게 false면 색을 흐리게 처리        
        roomUIManger.startBtn.image.color = allReady ? new Color(1, 1, 1, 1f) : new Color(1, 1, 1, 0.5f);
        Hashtable start = new Hashtable
        {
            {"AllPlayersReady", allReady}
        };        
        PhotonNetwork.CurrentRoom.SetCustomProperties(start);
    }
}