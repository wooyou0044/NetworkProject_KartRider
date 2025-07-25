using Hashtable = ExitGames.Client.Photon.Hashtable;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Threading.Tasks;
using System.Linq;
using System.Threading;


public class LobbyManager : MonoBehaviourPunCallbacks
{
    [SerializeField]private LobbyUIManager lobbyUiMgr;
    private List<RoomInfo> roomInfos = new List<RoomInfo>();
    private Dictionary<string, RoomEntry> roomEntryMap = new Dictionary<string, RoomEntry>();
    private Queue<string> availableRoomNumbers = new Queue<string>(); // �� ��ȣ ���� Queue
    private HashSet<string> usedRoomNumbers = new HashSet<string>(); // ��� ���� �� ��ȣ ����
    private Coroutine roomListUpdateCor;
    /// <summary>
    /// ��ŸƮ�� �ڷ�ƾ���� �Ͽ� ��������� ���
    /// Ÿ��Ʋ ������ �Ѿ���鼭 �� ���濡 ���� �������� ���� �ޱ� ���� �۾�
    /// ��Ʈ��ũ���� ������ �Ϸ� �Ǹ� �κ� ������ �õ���
    /// </summary>
    /// <returns></returns>
    IEnumerator Start()
    {
        yield return new WaitUntil(() => PhotonNetwork.IsConnectedAndReady);
        PhotonNetwork.JoinLobby();
        lobbyUiMgr.ClickOffPanelActive(false);//�κ� ������ �ϴ� ��
        InitializeRoomNumber(); //�� ��ȣ �ʱ�ȭ
    }

    /// <summary>
    /// ���̸��� ���������� �����ϰ� �� ���̱� ������ ����� ���� ��ȣ�� �ʿ�
    /// �ִ��� ��ġ���ʴ� �� ��ȣ�� ���� �������� 100���� ���ڸ� ����
    /// </summary>
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
    /// <summary>
    /// ���� ��ȣ�� ������̶�� �� ���� ���� ������ �����
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// �������ϴ� �濡 ���ν�Ű�� ���� �޼ҵ�
    /// </summary>
    /// <param name="roomName">�� �̸� ��Ī</param>
    public void JoinRoom(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
    }

    /// <summary>
    /// �� ���� ��ư�� ���� ��ư Ŭ�� �� 
    /// ��ǲ �ʵ忡 �ۼ��� �̸����� ���� ������
    /// �н����� �� �Ǵ� ��ĭ�� Ȯ��, ������ ���� ������ �������� ����
    /// �� �ѹ��� �޾ƿͼ� ���� ���� ��ȣ�� ���ٸ� ������� ����
    /// </summary>
    /// 
    public void CreateRoomBtnClick()
    {
        lobbyUiMgr.ClickOffPanelActive(true);
        string roomName = lobbyUiMgr.roomNameInputField.text;
        string password = string.IsNullOrEmpty(lobbyUiMgr.roomPasswordInputField.text) ? null : lobbyUiMgr.roomPasswordInputField.text;
        string roomNumber = GetRoomNumber();

        if (string.IsNullOrEmpty(roomNumber))
            return;

        CreateRoom(roomName, password, roomNumber); //���濡�� �����ϴ� �޼��尡 �ƴ� ���� �ż���
    }

    /// <summary>
    /// ������ �� ���� ��ư�� ����, ���� ������ ������ ���� ������
    /// </summary>
    public void JoinRandomRoomBtn()
    {
        lobbyUiMgr.ClickOffPanelActive(true);
        PhotonNetwork.JoinRandomRoom();
    }

    /// <summary>
    /// RoomNumberJoinBtn�� ����Ǿ� ����
    /// ��ѹ��� �´� �濡 ��
    /// �̸����� ���� ���� ������ �̸��� �ߺ������� �����ϱ� ������ 
    /// ���ϴ� �濡 �����ϱ� ���ؼ� ������ �±װ� �ʿ� �߰� �� �±׸� ������ 6�ڸ� ���ڷ� ������
    /// </summary>
    public void RoomNumberJoinBtn()
    {
        lobbyUiMgr.ClickOffPanelActive(true);
        PhotonNetwork.JoinRoom(lobbyUiMgr.roomNumberInputField.text);
    }

    /// <summary>
    /// �� ���� �޼���
    /// �����Ʈ��ũ���� �������ִ� CreateRoom�� ����ϱ� ���� Hashtable�� Ȱ���Ͽ� RoomOptions�� Ŀ���� �� �� ����
    /// �����Ʈ��ũ���� �����ϴ� CustomRoomProperties, CustomRoomPropertiesForLobby�� Ȱ���Ͽ�
    /// �ɼ��� �߰� �� �����ϴ� �͵� ������
    /// </summary>
    /// <param name="roomName"></param>
    /// <param name="password"></param>
    /// <param name="roomNumber"></param>
    public void CreateRoom(string roomName, string password, string roomNumber)
    {       
        Hashtable custom = new Hashtable
        {
            { "RoomName", roomName },
            { "Password", password },
            { "RoomNumber", roomNumber },
            { "Map", "default"},
            { "IsGameStart", false }
        };

        RoomOptions roomOptions = new RoomOptions
        {
            MaxPlayers = 8, //�ִ� ���� �÷��̾� ����
            EmptyRoomTtl = 0, //�濡 ����� ���ٸ� �ٷ� ���� �����ϵ��� ����
            CustomRoomProperties = custom,
            CustomRoomPropertiesForLobby = new string[] { "RoomName", "Password", "RoomNumber","Map","IsGameStart" }
        };

        PhotonNetwork.CreateRoom(roomNumber, roomOptions, TypedLobby.Default); //�κ�� �� ���ۿ� ������ �κ�Ÿ���� �⺻
    }

    /// <summary>
    /// ���� ���� Ŭ�� �� ����
    /// �濡 ���� ���ߴٸ� ������ ���� ���鵵�� ������
    /// CreateRoom�� �ٸ��� �������� ������ ���� ��й�ȣ ������ ���� ���� ��� �뿡 ���� ������ ���� ������ �����ϵ��� ������ ����
    /// ���忡 ���� ���� �� ���� ���� ����� �������忡�� ������� �ν����� ���� ���̱� ������ OnJoinRandomFailed ���濡�� �����Ǵ� �޼��忡�� ����
    /// </summary>
    /// <param name="returnCode"> �� ���� ���п� ���� �����ڵ� : ���� �������� ���� </param>
    /// <param name="message"> �� ���� ���� ���� �޼��� : ���� �������� ���� </param>
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
            { "Password", "" },
            { "RoomNumber", roomNumber },
            { "Map", "default"},
            { "IsGameStart", false }
        };

        RoomOptions roomOptions = new RoomOptions
        {
            MaxPlayers = 8, //�ִ� ���� �÷��̾� ����
            EmptyRoomTtl = 0, //�濡 ����� ���ٸ� �ٷ� ���� �����ϵ��� ����
            CustomRoomProperties = custom,
            CustomRoomPropertiesForLobby = new string[] { "RoomName", "Password", "RoomNumber", "Map", "IsGameStart" }
        };

        PhotonNetwork.JoinRandomOrCreateRoom(custom, 8, MatchmakingMode.FillRoom, null, null, roomNumber, roomOptions, null);
    }

    /// <summary>
    /// OnJoinRoom�� ���� �Ǹ� �뿡 ���� ���� (����ȯ) �ڷ�ƾ ����
    /// </summary>
    public override void OnJoinedRoom()
    {
        StartCoroutine(LoadJoinRoom("RoomScene"));
    }

    /// <summary>
    /// ��� �� ��ȯ�� ��Ʈ��ũ �������� ������ �ּ�ȭ �ϱ� ���� �ε� �ð��� ���� �߰��ؼ� ����
    /// </summary>
    IEnumerator LoadJoinRoom(string sceneName)
    {
        SceneCont.Instance.Oper = SceneCont.Instance.SceneAsync(sceneName);
        SceneCont.Instance.Oper.allowSceneActivation = false;
        while (SceneCont.Instance.Oper.isDone == false)
        {
            if (SceneCont.Instance.Oper.progress < 0.9f)
            {
                //�κ񿡼� ������ �̵��� ���α׷����ٸ� ���� �ʿ䰡 ������..?
            }
            else
            {
                SceneCont.Instance.Oper.allowSceneActivation = true;
                break;
            }
            yield return null;
        }
    }

    /// <summary>
    /// �κ������� �Ϸ� �Ǿ��ٸ� �븮��Ʈ�� ������Ʈ �ϰ�
    /// ������Ʈ �ڷ�ƾ ����
    /// </summary>
    public override void OnJoinedLobby()
    {
        roomListUpdateCor = StartCoroutine(RoomListUpdateCor());
    }

    /// <summary>
    /// �κ񿡼� ���� �� �븮��Ʈ ������Ʈ �ڷ�ƾ ���߱�
    /// �� �̵��� ������ �� ��ȯ���� ���� �ı��� �� ��������.... ��
    /// </summary>
    public override void OnLeftLobby()
    {
        StopAllCoroutines();
    }

    /// <summary>
    /// �κ񿡼� ���̴� ���� ������Ʈ ��
    /// ���濡�� ������Ʈ �Ǵ� ����� ���� ���� ����Ʈ�� ��Ƽ� ���
    /// </summary>
    /// <param name="roomList"></param>
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        //RoomListUpdate(roomList); //��ȭ�� �����Ǹ� �ڵ����� ������Ʈ(����ȭ ������ ���ڽ��ϴ�.)
        roomInfos = roomList; //�뿡 ��ȭ�� �����Ǹ� roomInfos�� ���� ������Ʈ
    }

    /// <summary>
    /// ���� ��ư�� ������ ���� ���Ƿ� �κ�Room ������Ʈ
    /// </summary>
    public void RoomListUpdateBtn()
    {
        RoomListUpdate(roomInfos);
    }

    /// <summary>
    ///15�� ���� �� ����Ʈ �ڵ����� ������Ʈ �ǵ��� ������
    ///�ð��� �ٲ㵵 �˴ϴ�.
    /// </summary>
    /// <example waitForSec = new WaitForSeconds(s);> s�� ���ϴ� �ð��� ������ �� </example>
    IEnumerator RoomListUpdateCor()
    {
        WaitForSeconds waitForSec = new WaitForSeconds(1f);

        while (PhotonNetwork.InLobby)
        {
            RoomListUpdate(roomInfos);
            yield return waitForSec;
        }
    }

    /// <summary>
    /// ���濡�� OnRoomListUpdate�� ȣ�� �Ǹ� ��ü���� �� ����Ʈ ������Ʈ�� ������
    /// ���� ���� ���� �ǰų� �������ų� ���°� ��ȭ�Ǹ� �ڵ����� ���� �ǵ��� ����
    /// </summary>
    /// <param name="roomList">OnRoomListUpdate���� �� ����Ʈ�� �޾ƿͼ� ó����</param>
    public void RoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (RoomInfo roomInfo in roomList)
        {
            if (roomInfo.RemovedFromList) //�� ���� ó��
            {
                RemoveRoomEntry(roomInfo.Name);
            }
            else
            {
                if (!roomEntryMap.ContainsKey(roomInfo.Name)) //���ο� �� �߰�
                {
                    AddRoomToList(roomInfo);
                }
                else
                {
                    UpdateRoomEntry(roomInfo); //���� �� ������Ʈ
                }
            }
        }
    }
    /// <summary>
    /// �� ������Ʈ�� ������ ����ϴ� �޼���
    /// ������ �Ϸ� �Ǹ� ����Ʈ�� ��ųʸ����� ������
    /// </summary>
    private void RemoveRoomEntry(string roomName)
    {
        if (roomEntryMap.ContainsKey(roomName))
        {
            RoomEntry entryToRemove = roomEntryMap[roomName];

            // UI ������Ʈ ����
            Destroy(entryToRemove.gameObject);

            // ����Ʈ�� ���ο��� ����
            roomEntryMap.Remove(roomName);
        }
    }
    /// <summary>
    /// �� ������ ��ȭ �Ǿ��ٸ� �ش� ���� ������ �ٲ� ������ �ٽ� ������
    /// </summary>
    private void UpdateRoomEntry(RoomInfo roomInfo)
    {
        if (roomEntryMap.ContainsKey(roomInfo.Name))
        {
            RoomEntry existingEntry = roomEntryMap[roomInfo.Name];
            existingEntry.SetRoomInfo(roomInfo); //�ֽ� RoomInfo�� ������Ʈ
        }
    }
    /// <summary>
    /// �κ� �� ������Ʈ�� �����ϴ� �޼���
    /// �κ� �� ������Ʈ�� �����ϰ� �ش� ������ �뿣Ʈ��.SetRoomInfo�� �޼ҵ带 ���� �ش� ������ ������
    /// ������Ʈ�� ������ �Ǿ��ٸ� ������Ʈ�� ��ư�� ������
    /// ��ư ������ ���� �ش� ���� Ŭ���ϸ� �� ������ �̵��ϵ��� �ϴ� ������ ��
    /// �濡 ���� �Ӽ����� �ִٸ�?(������ ���� �Ǿ��ų�, ��й�ȣ�� �ְų� ��... �ɼ� �߰����� RoomEntry ��ũ��Ʈ���� �� ��)
    /// </summary>
    /// <param name="roomInfo">������ �������� �޾Ƽ� ó���� </param>
    public void AddRoomToList(RoomInfo roomInfo)
    {
        var roomEntry = Instantiate(lobbyUiMgr.roomPrefab, lobbyUiMgr.roomListPanel.transform); //�� ������Ʈ ����
        var roomEntryScript = roomEntry.GetComponent<RoomEntry>(); //������ �뿡 RoomEntry ��ũ��Ʈ ������Ʈ ���
        roomEntryMap.Add(roomInfo.Name, roomEntryScript);//�������� ��¥������Ʈ�� ����(��ųʸ��� �־ �Ǳ���)
        
        if (roomEntryScript != null)
        {
            roomEntryScript.SetRoomInfo(roomInfo); //RoomInfo���� ���� ����
            
            roomEntry.onClick.AddListener(() =>
            {
                //FirstOrDefault �� ���� roomInfos���� roomInfo���� ���� �̸��� �´� �͵� ��
                //�ִٸ� �ش� ��ü�� ��ȯ���ְ� ���ٸ� null�� ��ȯ�� ��
                //roomInfo = roomInfos.FirstOrDefault(r => r.CustomProperties["RoomNumber"].ToString() == roomEntryScript.roomNumberText.text);
                if (roomEntryScript.IsPasswrod(roomInfo))
                {
                    ShowPasswordPrompt(roomInfo.Name, roomEntryScript.roomPasswordText.text);
                }
                else if (roomEntryScript.IsGameStarted(roomInfo))
                {
                    RoomListUpdate(roomInfos);
                    lobbyUiMgr.RoomJoinFaildeText("������ �̹� ���� ���Դϴ�.");
                }
                else if (roomEntryScript.IsRoomFull(roomInfo))
                {
                    RoomListUpdate(roomInfos);
                    lobbyUiMgr.RoomJoinFaildeText("���� ���� á���ϴ�.");
                }
                else if (roomInfo == null)
                {
                    RoomListUpdate(roomInfos);
                    lobbyUiMgr.RoomJoinFaildeText("���� �����ϴ�.");
                }
                else
                {
                    JoinRoom(roomInfo.Name);
                }
            });
        }
    }
    /// <summary>
    /// �濡 ��й�ȣ�� ���� ��� ����� �޼���
    /// ��� �н����尡 �´ٸ� ����, ���� ���н� �����޼���
    /// </summary>
    /// <param name="roomName">���̸�</param>
    /// <param name="correctPassword">�н�����</param>
    public void ShowPasswordPrompt(string roomName, string correctPassword)
    {
        lobbyUiMgr.LockRoomPasswrodPanelActive(true);
        lobbyUiMgr.lockRoomConnectBtn.onClick.AddListener(() =>
        {
            string enteredPassword = lobbyUiMgr.lockRoomPasswordInputField.text;
            lobbyUiMgr.LockRoomPasswrodPanelActive(false);
            lobbyUiMgr.lockRoomPasswordInputField.text = "";
            if (enteredPassword == correctPassword)
            {
                JoinRoom(roomName);
            }
            else
            {
                RoomListUpdate(roomInfos);
                lobbyUiMgr.RoomJoinFaildeText("�Է��� ��й�ȣ�� ��ġ���� �ʽ��ϴ�.");
            }
        });
    }    
}