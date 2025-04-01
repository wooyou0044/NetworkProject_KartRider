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
    private LobbyUIManager lobbyUiMgr;
    private List<RoomInfo> roomInfos = new List<RoomInfo>();
    private Dictionary<string, RoomEntry> roomEntryMap = new Dictionary<string, RoomEntry>();
    private Queue<string> availableRoomNumbers = new Queue<string>(); // 방 번호 관리 Queue
    private HashSet<string> usedRoomNumbers = new HashSet<string>(); // 사용 중인 방 번호 추적
    private Coroutine roomListUpdateCor;
    /// <summary>
    /// 스타트를 코루틴으로 하여 연결수립을 대기
    /// 타이틀 씬에서 넘어오면서 씬 변경에 대한 안정성을 보장 받기 위한 작업
    /// 네트워크에서 연결이 완료 되면 로비 조인을 시도함
    /// </summary>
    /// <returns></returns>
    IEnumerator Start()
    {
        yield return new WaitUntil(() => PhotonNetwork.IsConnectedAndReady);
        PhotonNetwork.JoinLobby();
        lobbyUiMgr.ClickOffPanelActive(false);//로비에 들어오면 일단 끔
        InitializeRoomNumber(); //방 번호 초기화
    }

    /// <summary>
    /// 방이름은 증복설정을 가능하게 할 것이기 때문에 입장시 고유 번호가 필요
    /// 최대한 겹치지않는 방 번호를 위해 랜덤으로 100개의 숫자를 만듬
    /// </summary>
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
    /// <summary>
    /// 만약 번호가 사용중이라면 방 생성 전에 오류를 띄워줌
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// 들어가고자하는 방에 매핑시키기 위한 메소드
    /// </summary>
    /// <param name="roomName">룸 이름 매칭</param>
    public void JoinRoom(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
    }

    /// <summary>
    /// 룸 생성 버튼과 연결 버튼 클릭 시 
    /// 인풋 필드에 작성한 이름으로 방을 생성함
    /// 패스워드 널 또는 빈칸을 확인, 있으면 적용 없으면 적용하지 않음
    /// 룸 넘버를 받아와서 적용 만약 번호가 없다면 방생성을 막음
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

        CreateRoom(roomName, password, roomNumber); //포톤에서 지원하는 메서드가 아닌 만든 매서드
    }

    /// <summary>
    /// 랜덤한 방 입장 버튼과 연결, 방이 없으면 랜덤한 방을 생성함
    /// </summary>
    public void JoinRandomRoomBtn()
    {
        lobbyUiMgr.ClickOffPanelActive(true);
        PhotonNetwork.JoinRandomRoom();
    }

    /// <summary>
    /// RoomNumberJoinBtn과 연결되어 있음
    /// 방넘버에 맞는 방에 들어감
    /// 이름으로 하지 않은 이유는 이름은 중복생성이 가능하기 때문에 
    /// 원하는 방에 입장하기 위해서 고유한 태그가 필요 했고 그 태그를 랜덤한 6자리 숫자로 연결함
    /// </summary>
    public void RoomNumberJoinBtn()
    {
        lobbyUiMgr.ClickOffPanelActive(true);
        PhotonNetwork.JoinRoom(lobbyUiMgr.roomNumberInputField.text);
    }

    /// <summary>
    /// 룸 생성 메서드
    /// 포톤네트워크에서 지원해주는 CreateRoom을 사용하기 전에 Hashtable을 활용하여 RoomOptions을 커스텀 할 수 있음
    /// 포톤네트워크에서 지원하는 CustomRoomProperties, CustomRoomPropertiesForLobby를 활용하여
    /// 옵션을 추가 및 수정하는 것도 가능함
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
            MaxPlayers = 8, //최대 입장 플레이어 제한
            EmptyRoomTtl = 0, //방에 사람이 없다면 바로 방을 삭제하도록 지정
            CustomRoomProperties = custom,
            CustomRoomPropertiesForLobby = new string[] { "RoomName", "Password", "RoomNumber","Map","IsGameStart" }
        };

        PhotonNetwork.CreateRoom(roomNumber, roomOptions, TypedLobby.Default); //로비는 한 개밖에 없으니 로비타입은 기본
    }

    /// <summary>
    /// 빠른 입장 클릭 시 실행
    /// 방에 들어가지 못했다면 랜덤한 방을 만들도록 설정함
    /// CreateRoom과 다르게 랜덤으로 생성한 방은 비밀번호 설정을 하지 않음 대신 룸에 들어가서 설정을 통해 변경이 가능하도록 설정할 것임
    /// 입장에 실패 했을 때 내가 방을 만들면 유저입장에서 오류라고 인식하지 못할 것이기 때문에 OnJoinRandomFailed 포톤에서 제공되는 메서드에서 적용
    /// </summary>
    /// <param name="returnCode"> 룸 입장 실패에 따른 에러코드 : 따로 정의하지 않음 </param>
    /// <param name="message"> 룸 입장 실패 관련 메세지 : 따로 정의하지 않음 </param>
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
            { "Password", "" },
            { "RoomNumber", roomNumber },
            { "Map", "default"},
            { "IsGameStart", false }
        };

        RoomOptions roomOptions = new RoomOptions
        {
            MaxPlayers = 8, //최대 입장 플레이어 제한
            EmptyRoomTtl = 0, //방에 사람이 없다면 바로 방을 삭제하도록 지정
            CustomRoomProperties = custom,
            CustomRoomPropertiesForLobby = new string[] { "RoomName", "Password", "RoomNumber", "Map", "IsGameStart" }
        };

        PhotonNetwork.JoinRandomOrCreateRoom(custom, 8, MatchmakingMode.FillRoom, null, null, roomNumber, roomOptions, null);
    }

    /// <summary>
    /// OnJoinRoom이 실행 되면 룸에 들어가기 위한 (씬전환) 코루틴 실행
    /// </summary>
    public override void OnJoinedRoom()
    {
        StartCoroutine(LoadJoinRoom("RoomScene"));
    }

    /// <summary>
    /// 모든 씬 전환은 네트워크 지연관련 오류를 최소화 하기 위해 로딩 시간을 따로 추가해서 만듬
    /// </summary>
    IEnumerator LoadJoinRoom(string sceneName)
    {
        SceneCont.Instance.Oper = SceneCont.Instance.SceneAsync(sceneName);
        SceneCont.Instance.Oper.allowSceneActivation = false;
        while (SceneCont.Instance.Oper.isDone == false)
        {
            if (SceneCont.Instance.Oper.progress < 0.9f)
            {
                //로비에서 룸으로 이동에 프로그래스바를 만들 필요가 있을까..?
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
    /// 로비입장이 완료 되었다면 룸리스트를 업데이트 하고
    /// 업데이트 코루틴 시작
    /// </summary>
    public override void OnJoinedLobby()
    {
        roomListUpdateCor = StartCoroutine(RoomListUpdateCor());
    }

    /// <summary>
    /// 로비에서 나갈 때 룸리스트 업데이트 코루틴 멈추기
    /// 룸 이동시 어차피 씬 전환으로 인해 파괴가 될 것이지만.... 흠
    /// </summary>
    public override void OnLeftLobby()
    {
        StopAllCoroutines();
    }

    /// <summary>
    /// 로비에서 보이는 룸을 업데이트 함
    /// 포톤에서 업데이트 되는 룸들을 내가 만든 리스트에 담아서 사용
    /// </summary>
    /// <param name="roomList"></param>
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        //RoomListUpdate(roomList); //변화가 감지되면 자동으로 업데이트(최적화 문제로 빼겠습니다.)
        roomInfos = roomList; //룸에 변화가 감지되면 roomInfos에 새로 업데이트
    }

    /// <summary>
    /// 리셋 버튼을 누르면 유저 임의로 로비Room 업데이트
    /// </summary>
    public void RoomListUpdateBtn()
    {
        RoomListUpdate(roomInfos);
    }

    /// <summary>
    ///15초 마다 룸 리스트 자동으로 업데이트 되도록 설계함
    ///시간은 바꿔도 됩니다.
    /// </summary>
    /// <example waitForSec = new WaitForSeconds(s);> s에 원하는 시간초 넣으면 됨 </example>
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
    /// 포톤에서 OnRoomListUpdate가 호출 되면 전체적인 룸 리스트 업데이트를 진행함
    /// 방이 새로 생성 되거나 없어지거나 상태가 변화되면 자동으로 실행 되도록 만듬
    /// </summary>
    /// <param name="roomList">OnRoomListUpdate에서 룸 리스트를 받아와서 처리함</param>
    public void RoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (RoomInfo roomInfo in roomList)
        {
            if (roomInfo.RemovedFromList) //방 삭제 처리
            {
                RemoveRoomEntry(roomInfo.Name);
            }
            else
            {
                if (!roomEntryMap.ContainsKey(roomInfo.Name)) //새로운 방 추가
                {
                    AddRoomToList(roomInfo);
                }
                else
                {
                    UpdateRoomEntry(roomInfo); //기존 방 업데이트
                }
            }
        }
    }
    /// <summary>
    /// 방 오브젝트를 삭제를 담당하는 메서드
    /// 삭제가 완료 되면 리스트와 딕셔너리에서 제거함
    /// </summary>
    private void RemoveRoomEntry(string roomName)
    {
        if (roomEntryMap.ContainsKey(roomName))
        {
            RoomEntry entryToRemove = roomEntryMap[roomName];

            // UI 오브젝트 삭제
            Destroy(entryToRemove.gameObject);

            // 리스트와 매핑에서 제거
            roomEntryMap.Remove(roomName);
        }
    }
    /// <summary>
    /// 룸 설정이 변화 되었다면 해당 방의 설정을 바뀐 값으로 다시 셋팅함
    /// </summary>
    private void UpdateRoomEntry(RoomInfo roomInfo)
    {
        if (roomEntryMap.ContainsKey(roomInfo.Name))
        {
            RoomEntry existingEntry = roomEntryMap[roomInfo.Name];
            existingEntry.SetRoomInfo(roomInfo); //최신 RoomInfo로 업데이트
        }
    }
    /// <summary>
    /// 로비에 룸 오브젝트를 생성하는 메서드
    /// 로비에 룸 오브젝트를 생성하고 해당 값들을 룸엔트리.SetRoomInfo의 메소드를 통해 해당 값을들 셋팅함
    /// 오브젝트가 생성이 되었다면 오브젝트를 버튼에 연결함
    /// 버튼 연결을 통해 해당 방을 클릭하면 그 방으로 이동하도록 하는 역할을 함
    /// 방에 무언가 속성값이 있다면?(게임이 시작 되었거나, 비밀번호가 있거나 등... 옵션 추가가능 RoomEntry 스크립트에서 할 것)
    /// </summary>
    /// <param name="roomInfo">포톤의 룸인포를 받아서 처리함 </param>
    public void AddRoomToList(RoomInfo roomInfo)
    {
        var roomEntry = Instantiate(lobbyUiMgr.roomPrefab, lobbyUiMgr.roomListPanel.transform); //룸 오브젝트 생성
        var roomEntryScript = roomEntry.GetComponent<RoomEntry>(); //생성된 룸에 RoomEntry 스크립트 컴포넌트 담기
        roomEntryMap.Add(roomInfo.Name, roomEntryScript);//방정보와 진짜오브젝트와 맵핑(딕셔너리만 있어도 되긴함)
        
        if (roomEntryScript != null)
        {
            roomEntryScript.SetRoomInfo(roomInfo); //RoomInfo에서 정보 설정
            
            roomEntry.onClick.AddListener(() =>
            {
                //FirstOrDefault 를 통해 roomInfos안의 roomInfo들을 보고 이름이 맞는 것들 중
                //있다면 해당 객체를 반환해주고 없다면 null을 반환해 줌
                //roomInfo = roomInfos.FirstOrDefault(r => r.CustomProperties["RoomNumber"].ToString() == roomEntryScript.roomNumberText.text);
                if (roomEntryScript.IsPasswrod(roomInfo))
                {
                    ShowPasswordPrompt(roomInfo.Name, roomEntryScript.roomPasswordText.text);
                }
                else if (roomEntryScript.IsGameStarted(roomInfo))
                {
                    RoomListUpdate(roomInfos);
                    lobbyUiMgr.RoomJoinFaildeText("게임이 이미 진행 중입니다.");
                }
                else if (roomEntryScript.IsRoomFull(roomInfo))
                {
                    RoomListUpdate(roomInfos);
                    lobbyUiMgr.RoomJoinFaildeText("방이 가득 찼습니다.");
                }
                else if (roomInfo == null)
                {
                    RoomListUpdate(roomInfos);
                    lobbyUiMgr.RoomJoinFaildeText("방이 없습니다.");
                }
                else
                {
                    JoinRoom(roomInfo.Name);
                }
            });
        }
    }
    /// <summary>
    /// 방에 비밀번호가 있을 경우 실행될 메서드
    /// 방과 패스워드가 맞다면 입장, 입장 실패시 오류메세지
    /// </summary>
    /// <param name="roomName">방이름</param>
    /// <param name="correctPassword">패스워드</param>
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
                RoomListUpdate(roomInfos);
                lobbyUiMgr.RoomJoinFaildeText("입력한 비밀번호가 일치하지 않습니다.");
            }
        });
    }    
}