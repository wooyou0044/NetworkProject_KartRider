using Hashtable = ExitGames.Client.Photon.Hashtable;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;


public class LobbyManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private LobbyUIManager lobbyUiMgr;

    private List<RoomEntry> roomEntryList = new List<RoomEntry>();
    private Dictionary<string, RoomEntry> roomEntryMap = new Dictionary<string, RoomEntry>();    
    private Queue<string> availableRoomNumbers = new Queue<string>(); // �� ��ȣ ���� Queue
    private HashSet<string> usedRoomNumbers = new HashSet<string>(); // ��� ���� �� ��ȣ ����

    IEnumerator Start()
    {
        yield return new WaitUntil(() => PhotonNetwork.IsConnectedAndReady);
        PhotonNetwork.JoinLobby();
        InitializeRoomNumber(); //�� ��ȣ �ʱ�ȭ
    }
    private void InitializeRoomNumber()
    {
        HashSet<string> uniqueNumbers = new HashSet<string>();

        while (uniqueNumbers.Count < 100) // 100���� ������ �� ��ȣ ����
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
            usedRoomNumbers.Add(roomNumber); //��� ���� ��ȣ�� �߰�
            return roomNumber;
        }
        //ť�� �����ִ� �� ��ȣ�� ���� ���, UI�� ���� �޽��� ǥ��
        lobbyUiMgr.RoomJoinFaildeText("���� ���� �� �����ϴ�.");
        return null; //���� ��Ȳ�� ó���� �� �ֵ��� null ��ȯ
    }

    public void JoinRoom(string roomName)
    {//
        PhotonNetwork.JoinRoom(roomName);
    }
    public void CreateRoomBtnClick()
    {//�� ���� Ȯ�� ��ư�� ���� ��
        string roomName = lobbyUiMgr.roomNameInputField.text;
        string password = string.IsNullOrEmpty(lobbyUiMgr.roomPasswordInputField.text) ? null : lobbyUiMgr.roomPasswordInputField.text;
        string roomNumber = GetRoomNumber();

        if (string.IsNullOrEmpty(roomNumber))
            return;
        
        CreateRoom(roomName, password, roomNumber);
    }
    public void JoinRandomRoomBtn()
    {//������ �� ���� ��ư�� ����, ���� ������ �˾Ƽ� ������ ���� ������
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
            MaxPlayers = 8, //�ִ� ���� �÷��̾� ����
            EmptyRoomTtl = 0, //�濡 ����� ���ٸ� �ٷ� ���� �����ϵ��� ����     
            CustomRoomProperties = custom,
            CustomRoomPropertiesForLobby = new string[] { "RoomName", "Password", "RoomNumber" }
        };
        
        PhotonNetwork.CreateRoom(roomNumber, roomOptions, TypedLobby.Default);
    }
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        //���� ���� �õ� �� ���� ���ٸ� ���� �����ϱ� ���� �޼���
        //������ ������ �ϳ��� ���� �߰����� ����
        string[] roomNames = { "���Բ� īƮ���̴�", "��Ÿ�ù�9�� �𿩶�", "�� ������ �Ұ� ���׿�" };
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
            MaxPlayers = 8, //�ִ� ���� �÷��̾� ����
            EmptyRoomTtl = 0, //�濡 ����� ���ٸ� �ٷ� ���� �����ϵ��� ����
            CustomRoomProperties = custom,
            CustomRoomPropertiesForLobby = new string[] { "RoomName", "RoomNumber" }
        };
        
        PhotonNetwork.JoinRandomOrCreateRoom(custom, 8, MatchmakingMode.FillRoom, null, null, roomNumber, roomOptions, null);
    }
    public override void OnJoinedRoom()
    {
        PhotonNetwork.PlayerList[0].SetCustomProperties(new Hashtable() { { "Ű1", "���ڿ�" }, { "Ű2", 1 } });        
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
                //�κ� > �� �̵��� ���α׷����ٸ� ���� �ʿ䰡 ������..?
            }
            else
            {
                Debug.Log("�κ� ������");
                break;
            }
            yield return null;
            SceneCont.Instance.Oper.allowSceneActivation = true;
        }
    }
    public override void OnJoinedLobby()
    {
        Debug.Log("�κ� ����");
    }        
    
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        RoomListUpdate(roomList);
    }
    public void RoomListUpdateBtn()
    {
        
    }
    public void RoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (RoomInfo roomInfo in roomList)
        {
            if (roomInfo.RemovedFromList) // �� ���� ó��
            {
                RemoveRoomEntry(roomInfo.Name);
            }
            else
            {
                if (!roomEntryMap.ContainsKey(roomInfo.Name)) // ���ο� �� �߰�
                {
                    AddRoomToList(roomInfo);
                }
                else
                {
                    UpdateRoomEntry(roomInfo); // ���� �� ������Ʈ
                }
            }
        }
    }
    private void RemoveRoomEntry(string roomName)
    {
        if (roomEntryMap.ContainsKey(roomName))
        {
            RoomEntry entryToRemove = roomEntryMap[roomName];

            // UI ������Ʈ ����
            Destroy(entryToRemove.gameObject);

            // ����Ʈ�� ���ο��� ����
            roomEntryList.Remove(entryToRemove);
            roomEntryMap.Remove(roomName);
        }
    }
    private void UpdateRoomEntry(RoomInfo roomInfo)
    {
        if (roomEntryMap.ContainsKey(roomInfo.Name))
        {
            RoomEntry existingEntry = roomEntryMap[roomInfo.Name];
            existingEntry.SetRoomInfo(roomInfo); // �ֽ� RoomInfo�� ������Ʈ
        }        
    }
    public void AddRoomToList(RoomInfo roomInfo)
    {
        var roomEntry = Instantiate(lobbyUiMgr.roomPrefab, lobbyUiMgr.roomListPanel.transform);
        var roomEntryScript = roomEntry.GetComponent<RoomEntry>();
        roomEntryList.Add(roomEntryScript); //����Ʈ�� �߰�
        roomEntryMap.Add(roomInfo.Name, roomEntryScript);//�������� ��¥������Ʈ�� ����
        if (roomEntryScript != null)
        {
            bool hasPassword = roomEntryScript.IsPasswrod(roomInfo);
            bool isGameStart = roomEntryScript.IsGameStarted(roomInfo);
            bool isRoomFull = roomEntryScript.IsRoomFull(roomInfo);
            roomEntryScript.SetRoomInfo(roomInfo); //RoomInfo���� ���� ����

            roomEntry.onClick.AddListener(() =>
            {                
                if (hasPassword)
                {
                    ShowPasswordPrompt(roomInfo.Name, roomInfo.CustomProperties["Password"] as string);
                }
                else if (!isGameStart)
                {
                    lobbyUiMgr.RoomJoinFaildeText("������ �̹� ���� ���Դϴ�.");
                }
                else if (isRoomFull)
                {
                    lobbyUiMgr.RoomJoinFaildeText("���� ���� á���ϴ�.");
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
                lobbyUiMgr.RoomJoinFaildeText("�Է��� ��й�ȣ�� ��ġ���� �ʽ��ϴ�.");
            }
        });
    }
}