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
    public void InitializeLobby()
    {//로비 초기화
        createRoomPanel.SetActive(false);
        roomNumberJoinPanel.SetActive(false);        
        roomJoinFaildePanel.SetActive(false);
        lockRoomPanel.SetActive(false);
    }
    public void CreateRoomPanleCon()
    {//방 생성 버튼 클릭시 활성화
        createRoomPanel.SetActive(true);
    }
    public void RoomNumberJoinPanelCon()
    {//대기방 입장 클릭시 활성화
        roomNumberJoinPanel.SetActive(true);
    }
    public void CreateRoomPanleCancelCon()
    {//방 만들기 취소 클릭시 비활성화
        createRoomPanel.SetActive(false);
    }
    public void RoomNumberJoinPanelCancelCon()
    {//대기방 입장 취소 클릭시 비활성화
        roomNumberJoinPanel.SetActive(false);
    }
    public void LockRoomPasswrodPanelActive(bool active)
    {//비밀번호가 걸린 방 입장시 
        lockRoomPanel.SetActive(active);
    }
    public void RoomJoinFaildeText(string message)
    {//UI 패널 또는 팝업 활성화
        roomJoinFaildePanel.SetActive(true);
        roomJoinFaildeText.text = message; //텍스트 설정
    }
    public void RoomJoinFaildeBtn()
    {
        roomJoinFaildePanel.SetActive(false);
    }
    public void LockRoomPanelCancelBtn()
    {
        lockRoomPanel.SetActive(false);
    }
    public void ClearRoomList()
    {
        foreach (Transform child in roomListPanel.transform)
        {
            Destroy(child.gameObject);
        }
    }
}

