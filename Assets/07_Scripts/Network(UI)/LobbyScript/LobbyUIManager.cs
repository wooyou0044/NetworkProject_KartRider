using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LobbyUIManager : MonoBehaviour
{
    public LobbyManager lobbyManager;

    [Header("방 만들기 판넬")]
    public GameObject createRoomPanel;
    public TMP_InputField roomNameInputField;
    public TMP_InputField roomPasswordInputField;

    [Header("대기방 입장 판넬")]
    public GameObject roomNumberJoinPanel;
    public TMP_InputField roomNumberInputField;

    [Header("룸 리스트 옵션")]
    public GameObject roomListPanel;
    public Button roomPrefab;

    [Header("룸 연결 옵션")]
    public GameObject roomJoinFaildePanel;
    public TMP_Text roomJoinFaildeText;

    [Header("비밀방 옵션")]
    public GameObject lockRoomPanel;
    public TMP_InputField lockRoomPasswordInputField;
    public Button lockRoomConnectBtn;

    [Header("룸 리스트 리셋 버튼")]
    public Button roomReSetBtn;

    [Header("클릭 방지 판넬(임시)")]
    public GameObject clickOffPanel;
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
        ClickOffPanelActive(false);
        createRoomPanel.SetActive(false);
    }
    //대기방 입장 취소 클릭시 비활성화
    public void RoomNumberJoinPanelCancelCon()
    {
        ClickOffPanelActive(false);
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
    //버튼 클릭을 막는 판넬 오브젝트
    public void ClickOffPanelActive(bool active)
    {
        clickOffPanel.SetActive(active);
    }
}

