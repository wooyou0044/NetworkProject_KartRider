using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Realtime;

public class LobbyUIManager : MonoBehaviour
{
    public LobbyManager lobbyManager;

    [Header("방 만들기 판넬")]
    public GameObject createRoomPanle;
    public TMP_InputField roomNameInputField;
    public TMP_InputField roomPasswordInputField;

    [Header("대기방 입장 판넬")]
    public GameObject roomNumberJoinPanel;
    public TMP_InputField roomNumberInputField;

    [Header("룸 옵션")]
    public GameObject roomListPanel;
    public GameObject roomPrefab;
    public GameObject roomNamePanel;
    public Sprite gameStageLevel;
    public TMP_Text roomNameText;
    public TMP_Text roomLevelText;
    public Sprite lockImg;
    public Sprite trackImg;
    private void Start()
    {
        createRoomPanle.SetActive(false);
        roomNumberJoinPanel.SetActive(false);
    }

    private void OnEnable()
    {
        LobbyManager.OnRoomListUpdateEvent += UpdateRoomList;
    }

    private void OnDisable()
    {
        LobbyManager.OnRoomListUpdateEvent -= UpdateRoomList;
    }

    private void UpdateRoomList(List<RoomInfo> roomList)
    {
        foreach (Transform child in roomListPanel.transform)
        {
            Destroy(child.gameObject); // 기존 목록 삭제
        }

        foreach (RoomInfo room in roomList)
        {
            var roomEntry = Instantiate(roomPrefab, roomListPanel.transform);

            // Room Prefab의 텍스트 및 이미지 설정
            roomNameText.text = room.Name;
            //TMP_Text roomLevelText = roomEntry.transform.Find("RoomLevel").GetComponent<TMP_Text>();
            //SpriteRenderer lockImage = roomEntry.transform.Find("LockImage").GetComponent<SpriteRenderer>();

            //roomNameText.text = room.Name;
            //roomLevelText.text = "Level: " + room.CustomProperties["Level"];
            //lockImage.sprite = room.IsOpen ? trackImg : lockImg;
        }
    }

    public void CreateRoomPanleCon()
    {
        createRoomPanle.SetActive(true);
    }
    public void RoomNumberJoinPanelCon()
    {
        roomNumberJoinPanel.SetActive(true);
    }
    public void CreateRoomPanleCancelCon()
    {
        createRoomPanle.SetActive(false);
    }
    public void RoomNumberJoinPanelCancelCon()
    {
        roomNumberJoinPanel.SetActive(false);
    }
    
    
}

