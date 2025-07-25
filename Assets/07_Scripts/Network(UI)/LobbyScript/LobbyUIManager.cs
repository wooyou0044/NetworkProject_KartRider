using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LobbyUIManager : MonoBehaviour
{
    public LobbyManager lobbyManager;

    [Header("�� ����� �ǳ�")]
    public GameObject createRoomPanel;
    public TMP_InputField roomNameInputField;
    public TMP_InputField roomPasswordInputField;

    [Header("���� ���� �ǳ�")]
    public GameObject roomNumberJoinPanel;
    public TMP_InputField roomNumberInputField;

    [Header("�� ����Ʈ �ɼ�")]
    public GameObject roomListPanel;
    public Button roomPrefab;

    [Header("�� ���� �ɼ�")]
    public GameObject roomJoinFaildePanel;
    public TMP_Text roomJoinFaildeText;

    [Header("��й� �ɼ�")]
    public GameObject lockRoomPanel;
    public TMP_InputField lockRoomPasswordInputField;
    public Button lockRoomConnectBtn;

    [Header("�� ����Ʈ ���� ��ư")]
    public Button roomReSetBtn;

    [Header("Ŭ�� ���� �ǳ�(�ӽ�)")]
    public GameObject clickOffPanel;
    private void Start()
    {
        InitializeLobby();
    }
    //�κ� �ʱ�ȭ
    public void InitializeLobby()
    {
        createRoomPanel.SetActive(false);
        roomNumberJoinPanel.SetActive(false);        
        roomJoinFaildePanel.SetActive(false);
        lockRoomPanel.SetActive(false);
    }
    //�� ���� ��ư Ŭ���� Ȱ��ȭ
    public void CreateRoomPanleCon()
    {
        ClickOffPanelActive(true);
        createRoomPanel.SetActive(true);
    }
    //���� ���� Ŭ���� Ȱ��ȭ
    public void RoomNumberJoinPanelCon()
    {
        ClickOffPanelActive(true);
        roomNumberJoinPanel.SetActive(true);
    }
    //�� ����� ��� Ŭ���� ��Ȱ��ȭ
    public void CreateRoomPanleCancelCon()
    {
        ClickOffPanelActive(false);
        roomNameInputField.text = "";
        roomPasswordInputField.text = "";
        createRoomPanel.SetActive(false);
    }
    //���� ���� ��� Ŭ���� ��Ȱ��ȭ
    public void RoomNumberJoinPanelCancelCon()
    {
        ClickOffPanelActive(false);
        roomNumberInputField.text = "";
        roomNumberJoinPanel.SetActive(false);
    }
    //��й�ȣ�� �ɸ� �� ����� 
    public void LockRoomPasswrodPanelActive(bool active)
    {
        ClickOffPanelActive(active);
        lockRoomPanel.SetActive(active);
    }
    //UI �г� �Ǵ� �˾� Ȱ��ȭ
    public void RoomJoinFaildeText(string message)
    {
        roomJoinFaildePanel.SetActive(true);
        roomJoinFaildeText.text = message; //�ؽ�Ʈ ó��
    }
    //�� ���� ���н� �ߴ� �ǳ� ��Ȱ��ȭ ��ư
    public void RoomJoinFaildeBtn()
    {
        ClickOffPanelActive(false);
        roomJoinFaildePanel.SetActive(false);
    }
    //��й�ȣ �Է�â ��Ȱ��ȭ
    public void LockRoomPanelCancelBtn()
    {
        ClickOffPanelActive(false);
        lockRoomPasswordInputField.text = "";
        lockRoomPanel.SetActive(false);
    }
    //��ư Ŭ���� ���� �ǳ� ������Ʈ
    public void ClickOffPanelActive(bool active)
    {
        clickOffPanel.SetActive(active);
    }
}

