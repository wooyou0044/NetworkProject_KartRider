using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;


public class RoomManager : MonoBehaviourPunCallbacks
{
    [SerializeField] public RoomUIManager roomUIManger;
    private Player[] players = PhotonNetwork.PlayerList;
    private void Start()
    {        
        foreach (var player in players)
        {
            //이미지로 띄워주면 됨
            //배열의 0 번부터 차례대로
            Debug.Log("방 안의 사람들 목록"+ player.NickName);
        }
        
        if (PhotonNetwork.IsMasterClient == false)
        {
            roomUIManger.startBtn.interactable =false;
            roomUIManger.readyBtn.interactable = true;
            //만약 마스터가 아니라면 스타트 버튼이 아니라 준비완료 버튼으로 띄우기
        }
    }
    public override void OnLeftLobby()
    {
        Debug.Log("로비 퇴장했습니다.");
    }
    public void StartGame()
    {
        if(PhotonNetwork.IsMasterClient == true)
        {
            //인 게임으로 씬이동
            PhotonNetwork.LoadLevel("InGameScene");
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
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        //유저 이탈시 해당 유저가 있던 부분의 UI업데이트
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
                Debug.Log(SceneCont.Instance.Oper.progress);
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
    public void JoinTestBtn()
    {
        Debug.Log(PhotonNetwork.IsConnected + "마스터");
        Debug.Log(PhotonNetwork.InLobby + "로비");
        Debug.Log(PhotonNetwork.InRoom + "룸");
    }
}
