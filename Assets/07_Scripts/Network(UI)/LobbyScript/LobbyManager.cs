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
    private void Start()
    {
        PhotonNetwork.JoinLobby();
    }
    public void JoinRandomRoom()
    {
        PhotonNetwork.JoinRandomOrCreateRoom();
    }
    public void CreateRoomBtnClick()
    {
        string roomName = lobbyUiMgr.roomNameInputField.text;
        string passwrod = lobbyUiMgr.roomPasswordInputField.text;
        int roomIndex = GetNextRoomIndex();
        //if(string.IsNullOrEmpty(roomName) )
        //{
        //방 이름이 비어있다면 리스트에 저장된 것들 중 하나로 띄워주기 나중에
        //}
        if (string.IsNullOrEmpty(passwrod))
        {
            passwrod = null;
        }
        CreateRoom(roomName, passwrod, roomIndex);
    }
    private int GetNextRoomIndex()
    {
        return currentRoomList.Count + 1;
    }
    public void JoinRoom()
    {
        PhotonNetwork.JoinRoom(roomEntry.roomNameText.text);
    }
    public override void OnJoinedRoom()
    {
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
                SceneCont.Instance.Oper.allowSceneActivation = true;
            }
            yield return null;
        }
    }

    public void JoinRoom(string roomName)
    {
        if (!PhotonNetwork.InLobby)
        {
            PhotonNetwork.JoinLobby();
            return;
        }
        PhotonNetwork.JoinRoom(roomName);
    }
    public void CreateRoom(string roomName, string password, int roomNumber)
    {//크리에이트 룸 커스텀 하는 곳
        Hashtable custom = new Hashtable
        {
            { "RoomName", roomName },//룸 이름과
            { "Password", password },//패스워드를 확인하여 초기화
            { "RoomNumber", roomNumber }
        };

        RoomOptions roomOptions = new RoomOptions
        {
            MaxPlayers = 8,
            //초기 설정 내장 된 커스텀 프러퍼티에 custom 내용 저장
            CustomRoomProperties = custom,
            //커스텀 된 프러퍼티를 가져와서 세팅
            CustomRoomPropertiesForLobby = new string[] { "RoomName", "Password","RoomNumber" },            
        };
        //커스텀 된 정보를 토대로 진짜 룸을 생성함.
        PhotonNetwork.CreateRoom(roomName, roomOptions, TypedLobby.Default);
    }

    public void RandomCreateRoom(string roomName, string password, int roomNumber)
    {//크리에이트 룸 커스텀 하는 곳
        Hashtable custom = new Hashtable
        {
            { "RoomName", roomName },//룸 이름과            
            { "RoomNumber", roomNumber }
        };

        RoomOptions roomOptions = new RoomOptions
        {
            MaxPlayers = 8,
            //초기 설정 내장 된 커스텀 프러퍼티에 custom 내용 저장
            CustomRoomProperties = custom,
            //커스텀 된 프러퍼티를 가져와서 세팅
            CustomRoomPropertiesForLobby = new string[] { "RoomName", "RoomNumber" },
        };
        //룸을 찾거나 없을 경우 조건에 따라 방을 만듬
        PhotonNetwork.JoinRandomOrCreateRoom(custom, 8, MatchmakingMode.RandomMatching,TypedLobby.Default, null, roomName, roomOptions, null);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        UpdateCurrentRoomList(roomList);
        Dictionary<string, RoomEntry> existingRoomEntries = new Dictionary<string, RoomEntry>();
        // 기존 UI에서 RoomEntry들을 매핑
        foreach (Transform child in lobbyUiMgr.roomListPanel.transform)
        {
            var roomEntry = child.GetComponent<RoomEntry>();
            if (roomEntry != null)
            {
                existingRoomEntries[roomEntry.roomNameText.text] = roomEntry;
            }
        }

        // 업데이트된 방 리스트 처리
        foreach (RoomInfo roomInfo in roomList)
        {
            if (existingRoomEntries.ContainsKey(roomInfo.Name))
            {
                // 방이 이미 존재하면 UI 업데이트
                existingRoomEntries[roomInfo.Name].SetRoomInfo(roomInfo);
            }
            else
            {
                // 새로 추가된 방만 UI 생성
                AddRoomToList(roomInfo);
            }
            // 기존 목록에서 처리된 방 제거 (추가된 방 남기기 위해)
            existingRoomEntries.Remove(roomInfo.Name);
        }
        // 삭제된 방 UI 제거
        foreach (var remainingEntry in existingRoomEntries.Values)
        {
            Destroy(remainingEntry.gameObject);
        }
    }
    public void UpdateCurrentRoomList(List<RoomInfo> roomList)
    {
        currentRoomList.Clear();
        currentRoomList.AddRange(roomList);
    }
    public void RoomResetBtnClick()
    {
        OnRoomListUpdate(currentRoomList);
    }
    public void AddRoomToList(RoomInfo roomInfo)
    {
        // 방 리스트 프리팹 생성
        var roomEntry = Instantiate(lobbyUiMgr.roomPrefab, lobbyUiMgr.roomListPanel.transform);
        var roomEntryScript = roomEntry.GetComponent<RoomEntry>();
        if (roomEntryScript != null)
        {
            //방의 세팅 정보를 넘김
            roomEntryScript.SetRoomInfo(roomInfo);

        }
        roomEntry.onClick.AddListener(() =>
        {
            // 방 상태 체크
            bool hasPassword = roomEntryScript.IsPasswrod(roomInfo);
            bool isGameStart = roomEntryScript.IsGameStarted(roomInfo);
            bool isRoomFull = roomEntryScript.IsRoomFull(roomInfo);

            if (hasPassword)
            {
                ShowPasswordPrompt(roomInfo.Name, roomInfo.CustomProperties["Password"] as string);
                return;
            }
            else if (isGameStart)
            {
                lobbyUiMgr.RoomJoinFaildeText("게임이 이미 진행중이라 입장할 수 없습니다.");
                return;
            }
            else if (isRoomFull)
            {
                lobbyUiMgr.RoomJoinFaildeText("자리가 없어 입장할 수 없습니다.");
                return;
            }
            else if (roomInfo == null)
            {
                lobbyUiMgr.RoomJoinFaildeText("입장하려는 방이 없습니다.");
                return;
            }
            JoinRoom(roomInfo.Name);
        });
    }
    public void ShowPasswordPrompt(string roomName, string correctPassword)
    {
        // 비밀번호 입력창을 활성화
        lobbyUiMgr.LockRoomPasswrodPanelActive(true);

        // 확인 버튼 동작 설정
        lobbyUiMgr.lockRoomConnectBtn.onClick.AddListener(() =>
        {
            string enteredPassword = lobbyUiMgr.lockRoomPasswordInputField.text;
            //비밀번호 입력 후 입장을 했다면
            lobbyUiMgr.LockRoomPasswrodPanelActive(false);
            if (enteredPassword == correctPassword)
            {                
                JoinRoom(roomName); // 비밀번호 일치 시 방 입장
            }
            else
            {
                lobbyUiMgr.RoomJoinFaildeText("입력하신 비밀번호가 일치하지 않습니다. 다시 확인해 주세요.");
            }
        });
    }
}