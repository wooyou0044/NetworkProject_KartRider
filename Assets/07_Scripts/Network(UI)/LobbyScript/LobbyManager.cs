using Hashtable = ExitGames.Client.Photon.Hashtable;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;


public class LobbyManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private LobbyUIManager lobbyUiMgr;
    [SerializeField] private RoomEntry roomEntry;

    private List<RoomInfo> currentRoomList = new List<RoomInfo>();
    private List<RoomEntry> roomEntryList = new List<RoomEntry>();
    private Dictionary<string, RoomEntry> roomEntryMap = new Dictionary<string, RoomEntry>();

    private Queue<string> availableRoomNumbers = new Queue<string>(); // 방 번호 관리 Queue
    private HashSet<string> usedRoomNumbers = new HashSet<string>(); // 사용 중인 방 번호 추적

    IEnumerator Start()
    {
        yield return new WaitUntil(() => PhotonNetwork.IsConnectedAndReady);
        PhotonNetwork.JoinLobby();
        InitializeRoomNumber(); //방 번호 초기화
    }
    private void InitializeRoomNumber()
    {
        HashSet<string> uniqueNumbers = new HashSet<string>();

        while (uniqueNumbers.Count < 100) // 100개의 고유한 방 번호 생성
        {
            string roomNumber = Random.Range(100000, 999999).ToString();
            uniqueNumbers.Add(roomNumber);
        }

        foreach (var number in uniqueNumbers)
        {
            availableRoomNumbers.Enqueue(number);
        }        
    }

    private string GetRoomNumber()
    {
        if (availableRoomNumbers.Count > 0)
        {
            string roomNumber = availableRoomNumbers.Dequeue();
            usedRoomNumbers.Add(roomNumber); //사용 중인 번호에 추가
            return roomNumber;
        }
        //큐에 남아있는 방 번호가 없을 경우, UI로 오류 메시지 표시
        lobbyUiMgr.RoomJoinFaildeText("방을 만들 수 없습니다.");
        return null; //오류 상황을 처리할 수 있도록 null 반환
    }

    public void JoinRoom(string roomName)
    {//
        PhotonNetwork.JoinRoom(roomName);
    }
    public void CreateRoomBtnClick()
    {//룸 생성 확인 버튼과 연결 됨
        string roomName = lobbyUiMgr.roomNameInputField.text;
        string password = string.IsNullOrEmpty(lobbyUiMgr.roomPasswordInputField.text) ? null : lobbyUiMgr.roomPasswordInputField.text;
        string roomNumber = GetRoomNumber();

        if (string.IsNullOrEmpty(roomNumber))
            return;
        
        CreateRoom(roomName, password, roomNumber);
    }
    public void JoinRandomRoomBtn()
    {//랜덤한 방 입장 버튼과 연결, 방이 없으면 알아서 랜덤한 방을 생성함
        PhotonNetwork.JoinRandomRoom();
    }

    public void CreateRoom(string roomName, string password, string roomNumber)
    {
        Hashtable custom = new Hashtable
        {
            { "RoomName", roomName },
            { "Password", password },
            { "RoomNumber", roomNumber }
        };

        RoomOptions roomOptions = new RoomOptions
        {
            MaxPlayers = 8, //최대 입장 플레이어 제한
            EmptyRoomTtl = 0, //방에 사람이 없다면 바로 방을 삭제하도록 지정     
            CustomRoomProperties = custom,
            CustomRoomPropertiesForLobby = new string[] { "RoomName", "Password", "RoomNumber" }
        };
        
        PhotonNetwork.CreateRoom(roomNumber, roomOptions, TypedLobby.Default);
    }
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        //랜덤 조인 시도 시 방이 없다면 방을 생성하기 위한 메서드
        //방제는 저기중 하나가 나옴 추가설정 가능
        string[] roomNames = { "다함께 카트라이더", "메타플밍9기 모여라", "방 제목을 할게 없네요" };
        int randomName = Random.Range(0, roomNames.Length);
        string randomRoomName = roomNames[randomName];
        string roomNumber = GetRoomNumber();
        Hashtable custom = new Hashtable
        {
            { "RoomName", randomRoomName },
            { "RoomNumber", roomNumber }
        };

        RoomOptions roomOptions = new RoomOptions
        {
            MaxPlayers = 8, //최대 입장 플레이어 제한
            EmptyRoomTtl = 0, //방에 사람이 없다면 바로 방을 삭제하도록 지정
            CustomRoomProperties = custom,
            CustomRoomPropertiesForLobby = new string[] { "RoomName", "RoomNumber" }
        };
        
        PhotonNetwork.JoinRandomOrCreateRoom(custom, 8, MatchmakingMode.FillRoom, null, null, roomNumber, roomOptions, null);
    }
    public override void OnJoinedRoom()
    {
        PhotonNetwork.PlayerList[0].SetCustomProperties(new Hashtable() { { "키1", "문자열" }, { "키2", 1 } });        
        StartCoroutine(LoadJoinRoom("RoomScene"));
    }
    IEnumerator LoadJoinRoom(string sceneName)
    {
        SceneCont.Instance.Oper = SceneCont.Instance.SceneAsync(sceneName);
        SceneCont.Instance.Oper.allowSceneActivation = false;
        while (SceneCont.Instance.Oper.isDone == false)
        {
            if (SceneCont.Instance.Oper.progress < 0.9f)
            {
                //로비 > 룸 이동에 프로그래스바를 만들 필요가 있을까..?
            }
            else
            {
                Debug.Log("로비 나가기");
                break;
            }
            yield return null;
            SceneCont.Instance.Oper.allowSceneActivation = true;
        }
    }
    public override void OnJoinedLobby()
    {
        Debug.Log("로비 입장");
    }    
    
    
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {        
        RoomListUpdate(roomList);
    }
    public void RoomListUpdateBtn()
    {
        RoomListUpdate(currentRoomList);
    }
    public void RoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (RoomInfo roomInfo in roomList)
        {
            if (roomInfo.RemovedFromList) // 방 삭제 처리
            {
                RemoveRoomEntry(roomInfo.Name);
            }
            else
            {
                if (!roomEntryMap.ContainsKey(roomInfo.Name)) // 새로운 방 추가
                {
                    AddRoomToList(roomInfo);
                }
                else
                {
                    UpdateRoomEntry(roomInfo); // 기존 방 업데이트
                }
            }
        }
    }
    private void RemoveRoomEntry(string roomName)
    {
        if (roomEntryMap.ContainsKey(roomName))
        {
            RoomEntry entryToRemove = roomEntryMap[roomName];

            // UI 오브젝트 삭제
            Destroy(entryToRemove.gameObject);

            // 리스트와 매핑에서 제거
            roomEntryList.Remove(entryToRemove);
            roomEntryMap.Remove(roomName);
        }
    }
    private void UpdateRoomEntry(RoomInfo roomInfo)
    {
        if (roomEntryMap.ContainsKey(roomInfo.Name))
        {
            RoomEntry existingEntry = roomEntryMap[roomInfo.Name];
            existingEntry.SetRoomInfo(roomInfo); // 최신 RoomInfo로 업데이트
        }
    }
    public void AddRoomToList(RoomInfo roomInfo)
    {
        var roomEntry = Instantiate(lobbyUiMgr.roomPrefab, lobbyUiMgr.roomListPanel.transform);
        var roomEntryScript = roomEntry.GetComponent<RoomEntry>();
        roomEntryScript.SetRoomInfo(roomInfo); // RoomInfo에서 정보 설정
        roomEntryList.Add(roomEntryScript); // 리스트에 추가
        roomEntryMap.Add(roomInfo.Name, roomEntryScript);

        if (roomEntryScript != null)
        {
            roomEntryScript.SetRoomInfo(roomInfo);

            roomEntry.onClick.AddListener(() =>
            {
                bool hasPassword = roomEntryScript.IsPasswrod(roomInfo);
                bool isGameStart = roomEntryScript.IsGameStarted(roomInfo);
                bool isRoomFull = roomEntryScript.IsRoomFull(roomInfo);

                if (hasPassword)
                {
                    ShowPasswordPrompt(roomInfo.Name, roomInfo.CustomProperties["Password"] as string);
                }
                else if (!isGameStart)
                {
                    lobbyUiMgr.RoomJoinFaildeText("게임이 이미 진행 중입니다.");
                }
                else if (isRoomFull)
                {
                    lobbyUiMgr.RoomJoinFaildeText("방이 가득 찼습니다.");
                }
                else
                {
                    JoinRoom(roomInfo.Name);
                }
            });
        }
    }
    public void ShowPasswordPrompt(string roomName, string correctPassword)
    {
        lobbyUiMgr.LockRoomPasswrodPanelActive(true);

        lobbyUiMgr.lockRoomConnectBtn.onClick.AddListener(() =>
        {
            string enteredPassword = lobbyUiMgr.lockRoomPasswordInputField.text;
            lobbyUiMgr.LockRoomPasswrodPanelActive(false);

            if (enteredPassword == correctPassword)
            {
                JoinRoom(roomName);
            }
            else
            {
                lobbyUiMgr.RoomJoinFaildeText("입력한 비밀번호가 일치하지 않습니다.");
            }
        });
    }
}