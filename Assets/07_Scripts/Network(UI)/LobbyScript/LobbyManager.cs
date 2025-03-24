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
        InitializeRoomNumbers(); //방 번호 초기화
    }    
    private void InitializeRoomNumbers()
    {
        HashSet<string> uniqueNumbers = new HashSet<string>();
        System.Random random = new System.Random();

        while (uniqueNumbers.Count < 100) // 100개의 고유한 방 번호 생성
        {
            string roomNumber = random.Next(100000, 999999).ToString();
            uniqueNumbers.Add(roomNumber);
        }

        foreach (var number in uniqueNumbers)
        {
            availableRoomNumbers.Enqueue(number);
        }
        Debug.Log($"현재 큐 상태: {availableRoomNumbers.Count}");
    }

    private string GetUniqueRoomNumber()
    {
        if (availableRoomNumbers.Count > 0)
        {
            string roomNumber = availableRoomNumbers.Dequeue();
            usedRoomNumbers.Add(roomNumber); //사용 중인 번호에 추가
            return roomNumber;
        }
        //큐에 남아있는 방 번호가 없을 경우, UI로 오류 메시지 표시
        lobbyUiMgr.RoomJoinFaildeText("남아있는 방 번호가 없습니다");
        return null; //오류 상황을 처리할 수 있도록 null 반환
    }

    public void JoinRoom(string roomName)
    {
        //if (!PhotonNetwork.InLobby)
        //{
        //    PhotonNetwork.JoinLobby();
        //    return;
        //}
        PhotonNetwork.JoinRoom(roomName);
    }
    public void JoinRandomRoomBtn()
    {
        PhotonNetwork.JoinRandomRoom();
    }
    public override void OnJoinRandomFailed(short returnCode, string message)
    {

        string[] roomNames = { "다함께 카트라이더", "메타플밍9기 모여라", "방 제목을 할게 없네요" };
        int randomName = Random.Range(0, roomNames.Length);
        string randomRoomName = roomNames[randomName];
        string roomNumber = GetUniqueRoomNumber();
        Hashtable custom = new Hashtable
        {
            { "RoomName", randomRoomName },
            { "RoomNumber", roomNumber }
        };

        RoomOptions roomOptions = new RoomOptions
        {
            MaxPlayers = 8,
            EmptyRoomTtl = 0,
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
            }
            else
            {
                Debug.Log("로비 나가기 호출!");
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

    
    public override void OnLeftLobby()
    {
        Debug.Log("로비 퇴장");
    }
    private void ReleaseRoomNumber(string roomNumber)
    {
        if (usedRoomNumbers.Contains(roomNumber))
        {
            usedRoomNumbers.Remove(roomNumber);
            availableRoomNumbers.Enqueue(roomNumber); // 재활용
        }
        else
        {
            Debug.LogWarning($"해당 방 번호({roomNumber})는 사용 중이 아닙니다.");
        }
    }
    
    public void CreateRoomBtnClick()
    {
        string roomName = lobbyUiMgr.roomNameInputField.text;
        string password = string.IsNullOrEmpty(lobbyUiMgr.roomPasswordInputField.text) ? null : lobbyUiMgr.roomPasswordInputField.text;
        string roomNumber = GetUniqueRoomNumber();

        if (string.IsNullOrEmpty(roomNumber))
            return;

        
        CreateRoom(roomName, password, roomNumber);
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
            MaxPlayers = 8,
            EmptyRoomTtl = 0,            
            CustomRoomProperties = custom,
            CustomRoomPropertiesForLobby = new string[] { "RoomName", "Password", "RoomNumber" }
        };
        
        PhotonNetwork.CreateRoom(roomNumber, roomOptions, TypedLobby.Default);
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