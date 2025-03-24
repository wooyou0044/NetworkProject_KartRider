using Hashtable = ExitGames.Client.Photon.Hashtable;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;


public class RoomManager : MonoBehaviourPunCallbacks
{
    [SerializeField] public RoomUIManager roomUIManger;
    private Player[] players = PhotonNetwork.PlayerList;
    private RoomEntry roomEntry;
    private void Start()
    {
        RoomInfoUpdate();
        foreach (var player in players)
        {
            //이미지로 띄워주면 됨
            //배열의 0 번부터 차례대로
            Debug.Log("방 안의 사람들 목록"+ player.NickName);
        }
        
        if (PhotonNetwork.IsMasterClient)
        {
            roomUIManger.startBtn.gameObject.SetActive(true);
            roomUIManger.readyBtn.gameObject.SetActive(false);
            //모든 플레이어가 준비완료 되면 트루로 바꾸기(업데이트 RPC쏴야됨)
            roomUIManger.startBtn.interactable = true;
            int max = PhotonNetwork.CurrentRoom.MaxPlayers - 1;

            PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable
            {
                {"0", PhotonNetwork.LocalPlayer.ActorNumber }, { "1",0},
                { "2", 2<= max ? 0 : -1 }, { "3", 3 <= max ? 0 : -1 }, { "4",4 <= max ? 0: -1  },
                { "5", 5 <= max ? 0 : -1 }, {"6",  6 <= max ? 0 : -1}, {"7", 7<= max ? 0 : -1}
                
            });
        }        
        else
        {
            roomUIManger.startBtn.gameObject.SetActive(false);
            roomUIManger.readyBtn.gameObject.SetActive(true);            
        }
    }
    public void SetRoomInfoChange()
    {
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
        roomUIManger.roomPasswordText.text = (string)roomProperties["Password"];
        roomUIManger.SetPasswordUI(hasPassword);
    }
    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        RoomInfoUpdate(); // 변경된 방 속성을 UI에 반영
    }

    public override void OnLeftLobby()
    {
        Debug.Log("로비 퇴장했습니다.");
    }
    public void StartGame()
    {
        if(PhotonNetwork.IsMasterClient)
        {
            //인 게임으로 씬이동
            //PhotonNetwork.LoadLevel("InGameScene");
        }
    }
    public override void OnMasterClientSwitched(Player newMasterClient)
    {        
        foreach (var player in players)
        {            
            if (PhotonNetwork.IsMasterClient == true)
            {
                roomUIManger.startBtn.interactable = true;
                roomUIManger.readyBtn.interactable = false;
                return;
            }
        }
    }
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        //유저 입장시 유저 UI업데이트.
        UpdatePlayerUIList();
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        //유저 이탈시 해당 유저가 있던 부분의 UI업데이트
        UpdatePlayerUIList();
    }
    public void UpdatePlayerUIList()
    {
        for(int i = 0; i < players.Length; i++)
        {

        }
    }
    public void OutRoomBtn()
    {
        if (PhotonNetwork.InRoom)
        {
            Debug.Log("일단 방은 맞습니다.");
            StartCoroutine(LoadJoinLobby("LobbyScene"));
        }
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
        Debug.Log("옵니까?");
        yield break; 
    }
    public override void OnLeftRoom()
    {
        Debug.Log("확실히 방탈출");
        SceneCont.Instance.Oper.allowSceneActivation = true;
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
