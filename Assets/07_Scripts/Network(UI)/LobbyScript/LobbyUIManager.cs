using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Realtime;

public class LobbyUIManager : MonoBehaviour
{
    [SerializeField]public LobbyManager lobbyManager;

    [Header("방 만들기 판넬")]
    [SerializeField]public GameObject createRoomPanel;
    [SerializeField]public TMP_InputField roomNameInputField;
    [SerializeField]public TMP_InputField roomPasswordInputField;

    [Header("대기방 입장 판넬")]
    [SerializeField]public GameObject roomNumberJoinPanel;
    [SerializeField] public TMP_InputField roomNumberInputField;

    [Header("룸 옵션")]
    [SerializeField]public GameObject roomListPanel;
    [SerializeField]public Button roomPrefab;

    [Header("룸 옵션")]
    [SerializeField] public GameObject roomJoinFaildePanel;
    [SerializeField] public TMP_Text roomJoinFaildeText;
    private void Start()
    {
        createRoomPanel.SetActive(false);
        roomNumberJoinPanel.SetActive(false);
    }

    public void CreateRoomPanleCon()
    {
        createRoomPanel.SetActive(true);
    }
    public void RoomNumberJoinPanelCon()
    {
        roomNumberJoinPanel.SetActive(true);
    }
    public void CreateRoomPanleCancelCon()
    {
        createRoomPanel.SetActive(false);
    }
    public void RoomNumberJoinPanelCancelCon()
    {
        roomNumberJoinPanel.SetActive(false);
    }
    public void RoomJoinFaildeText(string message)
    {
        // UI 패널 또는 팝업 활성화
        roomJoinFaildePanel.SetActive(true);
        roomJoinFaildeText.text = message; // Feedback 텍스트 설정
    }
    public void RoomJoinFaildeBtn()
    {
        roomJoinFaildePanel.SetActive(false);
    }
    public void AddRoomToList(RoomInfo roomInfo)
    {
        // 방 리스트 프리팹 생성
        var roomEntry = Instantiate(roomPrefab, roomListPanel.transform);
        var roomEntryScript = roomEntry.GetComponent<RoomEntry>();
        var roomButton = roomEntry.GetComponent<Button>();

        if (roomEntryScript != null)
        {
            roomEntryScript.SetRoomInfo(roomInfo);
        }
        roomEntry.onClick.AddListener(() => lobbyManager.JoinRoom(roomInfo.Name));
    }
}