using Hashtable = ExitGames.Client.Photon.Hashtable;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;

public class RoomManager : MonoBehaviourPunCallbacks
{
    public PlayerSlot[] playerSlots;
    public RoomUIManager roomUIManger;

    private bool allReady = false;
    private bool isReadyTextChange = false;

    private bool isCorRunning = false;
    private WaitForSeconds WFownS = new WaitForSeconds(1f); //1�� ��� �ڷ�ƾ���� ���
    private WaitForSeconds WFthreeS = new WaitForSeconds(3f); //1�� ��� �ڷ�ƾ���� ���

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
    /// �����Ʈ��ũ���� �����ϴ� �ݹ��� ���� ������(����)�� �ٲٴ� �޼���
    /// �����Ͱ� �ٲ�� �ڽ��� ������ ������ ���ͳѹ��� �����Ѵ�.
    /// �غ�Ϸ� �̹����� ���� �غ�Ϸ� ��ư�� ����.
    /// �غ� �ϷḦ �غ� �������� �ٲ۴�.
    /// </summary>
    /// <param name="newMasterClient">����������</param>
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
            roomUIManger.startBtnText.text = "���� ����";
            roomUIManger.startBtn.image.color = new Color(1, 1, 1, 0.5f);
        }
    }
    /// <summary>
    /// �뿡 �����ϸ� ��ư�ؽ�Ʈ�� �ٲ� UI������ ��Ȱ���Ѵ�.
    /// ���� : ���� ���� ��ư
    /// �Ϲ� ���� : �غ� �Ϸ� ��ư
    /// </summary>
    private void InitializeUI()
    {

        if (PhotonNetwork.IsMasterClient)
        {
            roomUIManger.startBtnText.text = "���� ����";
            roomUIManger.startBtn.image.color = new Color(1, 1, 1, 0.5f);
        }
        else
        {
            isReadyTextChange = true;
            roomUIManger.startBtnText.text = "�غ� �Ϸ�";
        }
    }

    /// <summary>
    /// �ٽ� �� ������ ���ƿ��� ���α׷����� 0���� �ʱ�ȭ �Ѵ�.
    /// </summary>
    private void ProgressReSet()
    {
        Hashtable progress = new Hashtable();
        progress["LoadProgress"] = 0.0f;
        PhotonNetwork.LocalPlayer.SetCustomProperties(progress);
    }
    /// <summary>
    /// ���Կ� ���� �÷��̾� �ǳ��� �����Ѵ�.
    /// �����Ʈ��ũ.�ν��ϼ���Ʈ�� �����Ͽ� ��Ʈ��ũ�� �Ѹ���.
    /// </summary>
    public void CreatePlayerPanel()
    {
        var playerPanel = PhotonNetwork.Instantiate("PlayerPanelPrefab", transform.position, Quaternion.identity);
    }

    /// <summary>
    /// �����Ʈ��ũ���� �����ϴ� �÷��̾ �濡 ���� ���� �˼� �ִ� �޼ҵ�
    /// newPlayer�� ���� ��� ���� ������ �� �� �ִ�.
    /// ������ ������ �ݹ��� ����
    /// </summary>
    /// <param name="newPlayer">��� �濡 ���� �÷��̾�</param>
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
    }

    /// <summary>
    /// �����Ʈ��ũ���� �����ϴ� �÷��̾ ���� �������� �� �� �ִ� �޼ҵ�
    /// otherPlayer�� ���� ��� ���� �÷��̾ �˼� �ִ�.
    /// ������ ������ �ݹ��� ����
    /// ���� ������ �ڸ��� �����ϴ� ����
    /// </summary>
    /// <param name="otherPlayer">��� ���� ���� �÷��̾�</param>
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
    /// ������ ��ư�� ����
    /// ������ ��ư�� ������ �ش� ���� ������ �κ�� �̵��Ѵ�.
    /// �ε��� ���� �ڷ�ƾ ����
    /// </summary>
    public void OutRoomBtn()
    {
        if (PhotonNetwork.InRoom)
        {
            StartCoroutine(LoadJoinLobby("LobbyScene"));
        }
    }

    /// <summary>
    /// ���� ���� ���� �� ȣ��Ǵ� �ݹ�
    /// ���� ������ ���� ���� ���� ��������� ���� ��ȯ��
    /// </summary>
    public override void OnLeftRoom()
    {
        SceneCont.Instance.Oper.allowSceneActivation = true;
    }

    /// <summary>
    /// �� �̵� ���� ������ ��Ȱ���ϱ� ���� �ڷ�ƾ
    /// ���� �����ų�, �ΰ������� �̵��ϴ� ���� ��ȿ��� ���̵��� �߻����� �� �ε��� ���
    /// </summary>
    /// <param name="sceneName">������ �� �̸�</param>
    /// <returns>��û�� �Ϸ� �Ǹ� �ڷ�ƾ Ż��</returns>
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
    /// �� �̸��� ���н����带 ������ �� �ִ� �ǳ�
    /// Ŀ���� ������Ƽ�� Ȱ���Ͽ� �ش� ������ �ٲٰ� ������.
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
    /// �濡�� Ʈ���� ������ �� �ִ� ��ư
    /// ������ ������ ������ ������ ������ ������.
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
    /// Ŀ���� ������Ƽ�� Ȱ���Ͽ� ���� ����� ������ ������Ʈ�Ѵ�.
    /// ����� ������ Ŀ���� ������Ƽ�� Ÿ�� ����Ǿ� �κ� �ݿ��ǵ��� ��
    /// </summary>
    public void RoomInfoUpdate()
    {
        Hashtable roomProperties = PhotonNetwork.CurrentRoom.CustomProperties;
        roomUIManger.roomNameText.text = roomProperties.ContainsKey("RoomName") ? (string)roomProperties["RoomName"] : "�� �̸� ����";
        roomUIManger.roomNumberText.text = roomProperties.ContainsKey("RoomNumber") ? (string)roomProperties["RoomNumber"] : "�� ��ȣ ����";
        roomUIManger.roomMapNameText.text = roomProperties.ContainsKey("Map") ? (string)roomProperties["Map"]
        : "default"; ;
        var mapSprite = Resources.Load<Sprite>($"Maps/{roomUIManger.roomMapNameText.text}");
        roomUIManger.roomMapeImg.sprite = mapSprite != null ? mapSprite : Resources.Load<Sprite>($"Maps/{roomUIManger.roomMapNameText.text}");

        bool hasPassword = roomProperties.ContainsKey("Password") && !string.IsNullOrEmpty((string)roomProperties["Password"]);
        roomUIManger.roomPasswordText.text = hasPassword ? (string)roomProperties["Password"] : null;
        roomUIManger.SetPasswordUI(hasPassword);
    }
    /// <summary>
    /// �����Ʈ��ũ���� �����ϴ� �濡�� ������Ƽ�� ������ �ݹ�޴� �޼���
    /// �濡 ����� ������ �ѷ��ش�.
    /// </summary>
    /// <param name="propertiesThatChanged">������Ʈ�� ����</param>
    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {        
        RoomInfoUpdate(); //����� �� �Ӽ��� �뿡 �ݿ�
    }
    /// <summary>
    /// �����Ʈ��ũ���� �����ϴ� �濡�� ������Ƽ�� ����Ǿ��� �� �κ񿡼� �� ������
    /// �ݹ�޴� �޼���
    /// �濡 ���� �� ������ �κ� �ѷ��ش�.
    /// </summary>
    /// <param name="lobbyStatistics">�κ� ���� ��� �Ǵ� ������ �κ� ������ �� ����</param>
    public override void OnLobbyStatisticsUpdate(List<TypedLobbyInfo> lobbyStatistics)
    {
        RoomInfoUpdate();
    }

    /// <summary>
    /// ���� ����, �غ� �Ϸ� ��ư�� ���� ��
    /// �����̶�� ��ư�� ������ �� ���� �ο��� 2���̻������� ��� �÷��̾ �غ�Ϸ� ��������
    /// Ȯ���� �� ���ǿ� ���� �ʴٸ� �޼����� ���Ƴ�
    /// ������ �ƴҰ�� �ڽ��� ȭ�鿡�� ��ư�� ���Ƴ�
    /// </summary>
    public void StartGameBtn()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            //ȥ�� ������ ���� ����
            if (PhotonNetwork.CurrentRoom.PlayerCount < 2)
            {
                if (!isCorRunning)
                {
                    string massage = "ȥ�ڼ��� ������ ������ �� �����ϴ�. �ٸ� ���̼��� ��ٷ��ּ���.";
                    StartCoroutine(progressMessageCor(massage));
                }
                return;
            }
            //Ŀ���� ������Ƽ�� Ȱ���� �Ұ��� ���� �޾� ������ �ִٸ� �ٷ� ������Ʈ�Ƿ� ����
            if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("AllPlayersReady") &&
    (bool)PhotonNetwork.CurrentRoom.CustomProperties["AllPlayersReady"])
            {
                // ������ ���۵Ǹ� IsGameStart�� Ʈ��� �ٲ� �������� �����Ѵ�.
                Hashtable gameStart = new Hashtable
                {
                    {"IsGameStart", true }
                };
                PhotonNetwork.CurrentRoom.SetCustomProperties(gameStart);
                PhotonNetwork.CurrentRoom.IsOpen = false;//������ ������ ������
                PhotonNetwork.LoadLevel(roomUIManger.roomMapNameText.text);//�ʰ� ���� ������ �̵���
            }
            else
            {
                //�غ� �Ϸ� ���¸� üũ��
                if (!isCorRunning)
                {
                    string massage = "��� ���̼��� �غ� ���� �ʾ� ������ ������ �� �����ϴ�.";
                    StartCoroutine(progressMessageCor(massage));
                }
            }
        }
        else
        {
            //������ �ƴҰ�� ��ư�� ���Ƴ�
            StartCoroutine(ReadyBtnEnable());
            if (isReadyTextChange)
            {
                isReadyTextChange = false;
                roomUIManger.startBtnText.text = "�غ� ���";                
            }
            else
            {
                isReadyTextChange = true;
                roomUIManger.startBtnText.text = "�غ� �Ϸ�";
            }
        }
    }
    /// <summary>
    /// ��ư ��Ŭ ����
    /// </summary>
    /// <returns>�ڷ�ƾ���� 1�ʿ� �ѹ��� �������� ������</returns>
    IEnumerator ReadyBtnEnable()
    {
        roomUIManger.startBtn.interactable = false;
        yield return WFownS;
        roomUIManger.startBtn.interactable = true;
    }
    /// <summary>
    /// ���忡�� �˷��� ������ �ִٸ� �ش� �޼����� ���Ƴ��� ���� �ڷ�ƾ
    /// 3�ʵ��� ���� ��
    /// </summary>
    /// <param name="massage">��� �޼���</param>
    /// <returns></returns>
    IEnumerator progressMessageCor(string massage)
    {
        isCorRunning = true;//���� ȣ���� �������� �Һ���
        roomUIManger.progressMessagePanel.gameObject.SetActive(true);
        roomUIManger.progressMessage(massage);
        yield return WFthreeS;
        roomUIManger.progressMessage("");//ȣ���� ������ �޼��� �ʱ�ȭ
        roomUIManger.progressMessagePanel.gameObject.SetActive(false);
        isCorRunning = false;
    }
    /// <summary>
    /// Ŀ���� ������Ƽ�� ��ȯ�Ǵ� ���� ���� �ֱ� ���� �Һ���
    /// �ش� ������ Ŀ����������Ƽ�� ���� ���� ���޵Ǹ� false �Ǵ� true�� ��ȯ�Ѵ�.
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
                    return false; //�غ���� ���� �÷��̾ ������ false ��ȯ                
                }
            }
        }
        return true; //��� ������ �غ� �Ϸ�
    }
    /// <summary>
    /// Ŀ���� ������Ƽ�� ���� �������� �غ���¸� Ȯ���ϴ� �޼���
    /// ������ �������� �غ�Ϸ� ���¸� �޾� �����Ѵ�.
    /// �÷��̾ �ִ� ��� ������ ���°� �غ� �� ���¶�� true�� ��ȯ�ϰ� ������
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
        //���׿����ڸ� ���� ��ư���� �ٲ�
        //allReady�� true�� ���� ���ϰ� false�� ���� �帮�� ó��        
        roomUIManger.startBtn.image.color = allReady ? new Color(1, 1, 1, 1f) : new Color(1, 1, 1, 0.5f);
        Hashtable start = new Hashtable
        {
            {"AllPlayersReady", allReady}
        };        
        PhotonNetwork.CurrentRoom.SetCustomProperties(start);
    }
}