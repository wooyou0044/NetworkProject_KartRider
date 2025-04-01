using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LobbyUIManager : MonoBehaviour
{
    [SerializeField] public LobbyManager lobbyManager;

    [Header("방 만들기 판넬")]
    [SerializeField] public GameObject createRoomPanel;
    [SerializeField] public TMP_InputField roomNameInputField;
    [SerializeField] public TMP_InputField roomPasswordInputField;

    [Header("대기방 입장 판넬")]
    [SerializeField] public GameObject roomNumberJoinPanel;
    [SerializeField] public TMP_InputField roomNumberInputField;

    [Header("룸 리스트 옵션")]
    [SerializeField] public GameObject roomListPanel;
    [SerializeField] public Button roomPrefab;

    [Header("룸 연결 옵션")]
    [SerializeField] public GameObject roomJoinFaildePanel;
    [SerializeField] public TMP_Text roomJoinFaildeText;

    [Header("비밀방 옵션")]
    [SerializeField] public GameObject lockRoomPanel;
    [SerializeField] public TMP_InputField lockRoomPasswordInputField;
    [SerializeField] public Button lockRoomConnectBtn;

    [SerializeField] public Button roomReSetBtn;
    private void Start()
    {
        InitializeLobby();
    }
    //로비 초기화
    public void InitializeLobby()
    {
        createRoomPanel.SetActive(false);
        roomNumberJoinPanel.SetActive(false);        
        roomJoinFaildePanel.SetActive(false);
        lockRoomPanel.SetActive(false);
    }
    //방 생성 버튼 클릭시 활성화
    public void CreateRoomPanleCon()
    {
        createRoomPanel.SetActive(true);
    }
    //대기방 입장 클릭시 활성화
    public void RoomNumberJoinPanelCon()
    {
        roomNumberJoinPanel.SetActive(true);
    }
    //방 만들기 취소 클릭시 비활성화
    public void CreateRoomPanleCancelCon()
    {
        createRoomPanel.SetActive(false);
    }
    //대기방 입장 취소 클릭시 비활성화
    public void RoomNumberJoinPanelCancelCon()
    {
        roomNumberJoinPanel.SetActive(false);
    }
    //비밀번호가 걸린 방 입장시 
    public void LockRoomPasswrodPanelActive(bool active)
    {
        lockRoomPanel.SetActive(active);
    }
    //UI 패널 또는 팝업 활성화
    public void RoomJoinFaildeText(string message)
    {
        roomJoinFaildePanel.SetActive(true);
        roomJoinFaildeText.text = message; //텍스트 처리
    }
    //룸 입장 실패시 뜨는 판넬 비활성화 버튼
    public void RoomJoinFaildeBtn()
    {
        roomJoinFaildePanel.SetActive(false);
    }
    //비밀번호 입력창 비활성화
    public void LockRoomPanelCancelBtn()
    {
        lockRoomPanel.SetActive(false);
    }
}

