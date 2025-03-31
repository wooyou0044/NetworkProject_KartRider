using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LobbyUIManager : MonoBehaviour
{
    [SerializeField] public LobbyManager lobbyManager;

    [Header("�� ����� �ǳ�")]
    [SerializeField] public GameObject createRoomPanel;
    [SerializeField] public TMP_InputField roomNameInputField;
    [SerializeField] public TMP_InputField roomPasswordInputField;

    [Header("���� ���� �ǳ�")]
    [SerializeField] public GameObject roomNumberJoinPanel;
    [SerializeField] public TMP_InputField roomNumberInputField;

    [Header("�� ����Ʈ �ɼ�")]
    [SerializeField] public GameObject roomListPanel;
    [SerializeField] public Button roomPrefab;

    [Header("�� ���� �ɼ�")]
    [SerializeField] public GameObject roomJoinFaildePanel;
    [SerializeField] public TMP_Text roomJoinFaildeText;

    [Header("��й� �ɼ�")]
    [SerializeField] public GameObject lockRoomPanel;
    [SerializeField] public TMP_InputField lockRoomPasswordInputField;
    [SerializeField] public Button lockRoomConnectBtn;

    [SerializeField] public Button roomReSetBtn;
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
        createRoomPanel.SetActive(true);
    }
    //���� ���� Ŭ���� Ȱ��ȭ
    public void RoomNumberJoinPanelCon()
    {
        roomNumberJoinPanel.SetActive(true);
    }
    //�� ����� ��� Ŭ���� ��Ȱ��ȭ
    public void CreateRoomPanleCancelCon()
    {
        createRoomPanel.SetActive(false);
    }
    //���� ���� ��� Ŭ���� ��Ȱ��ȭ
    public void RoomNumberJoinPanelCancelCon()
    {
        roomNumberJoinPanel.SetActive(false);
    }
    //��й�ȣ�� �ɸ� �� ����� 
    public void LockRoomPasswrodPanelActive(bool active)
    {
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
        roomJoinFaildePanel.SetActive(false);
    }
    //��й�ȣ �Է�â ��Ȱ��ȭ
    public void LockRoomPanelCancelBtn()
    {
        lockRoomPanel.SetActive(false);
    }
}

