using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LobbyUIManager : MonoBehaviour
{
    [Header("방 만들기 판넬")]
    public GameObject createRoomPanle;
    public TMP_InputField roomNameInputField;
    public TMP_InputField roomPasswordInputField;

    [Header("대기방 입장 판넬")]
    public GameObject roomNumberJoinPanel;
    public TMP_InputField roomNumberInputField;
    private void Start()
    {
        createRoomPanle.SetActive(false);
        roomNumberJoinPanel.SetActive(false);
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

