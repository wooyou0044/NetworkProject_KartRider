using Hashtable = ExitGames.Client.Photon.Hashtable;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private LobbyUIManager lobbyUiMgr;
    [SerializeField] private RoomEntry roomEntry;

    private List<RoomInfo> currentRoomList = new List<RoomInfo>();
    private Queue<string> availableRoomNumbers = new Queue<string>(); // 방 번호 관리 Queue
    private HashSet<string> usedRoomNumbers = new HashSet<string>(); // 사용 중인 방 번호 추적

    private void Start()
    {
        PhotonNetwork.JoinLobby();
        InitializeRoomNumbers(); //방 번호 초기화
        lobbyUiMgr.roomReSetBtn.onClick.AddListener(() => OnRoomListUpdate(currentRoomList)); // 버튼 이벤트 연결
    }
    public void ConnectedOn()
    {
        Debug.Log(PhotonNetwork.IsConnected +"마스터");
        Debug.Log(PhotonNetwork.InLobby+"로비");
        Debug.Log(PhotonNetwork.InRoom+"룸");
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

        CreateRoom(roomName, password, int.Parse(roomNumber));
    }

    public void CreateRoom(string roomName, string password, int roomNumber)
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
            CustomRoomProperties = custom,
            CustomRoomPropertiesForLobby = new string[] { "RoomName", "Password", "RoomNumber" }
        };

        PhotonNetwork.CreateRoom(roomName, roomOptions, TypedLobby.Default);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        UpdateCurrentRoomList(roomList);

        Dictionary<string, RoomEntry> existingRoomEntries = new Dictionary<string, RoomEntry>();
        foreach (Transform child in lobbyUiMgr.roomListPanel.transform)
        {
            var roomEntry = child.GetComponent<RoomEntry>();
            if (roomEntry != null)
            {
                existingRoomEntries[roomEntry.roomNameText.text] = roomEntry;
            }
        }

        //HashSet<string> updatedRoomNames = new HashSet<string>(roomList.Select(room => room.Name));
        List<RoomEntry> entriesToRemove = new List<RoomEntry>();

        //foreach (var entry in existingRoomEntries)
        //{
        //    if (!updatedRoomNames.Contains(entry.Key))
        //    {
        //        entriesToRemove.Add(entry.Value);
        //    }
        //}

        foreach (var entryToRemove in entriesToRemove)
        {
            Destroy(entryToRemove.gameObject);
        }

        foreach (RoomInfo roomInfo in roomList)
        {
            if (existingRoomEntries.ContainsKey(roomInfo.Name))
            {
                existingRoomEntries[roomInfo.Name].SetRoomInfo(roomInfo);
            }
            else
            {
                AddRoomToList(roomInfo);
            }
        }
    }
    public void ResetBtn()
    {

    }
    public void UpdateCurrentRoomList(List<RoomInfo> roomList)
    {
        currentRoomList.Clear();
        currentRoomList.AddRange(roomList);
    }

    public void AddRoomToList(RoomInfo roomInfo)
    {
        var roomEntry = Instantiate(lobbyUiMgr.roomPrefab, lobbyUiMgr.roomListPanel.transform);
        var roomEntryScript = roomEntry.GetComponent<RoomEntry>();
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