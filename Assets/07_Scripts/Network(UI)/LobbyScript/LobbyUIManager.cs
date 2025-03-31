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
    public void InitializeLobby()
    {//�κ� �ʱ�ȭ
        createRoomPanel.SetActive(false);
        roomNumberJoinPanel.SetActive(false);        
        roomJoinFaildePanel.SetActive(false);
        lockRoomPanel.SetActive(false);
    }
    public void CreateRoomPanleCon()
    {//�� ���� ��ư Ŭ���� Ȱ��ȭ
        createRoomPanel.SetActive(true);
    }
    public void RoomNumberJoinPanelCon()
    {//���� ���� Ŭ���� Ȱ��ȭ
        roomNumberJoinPanel.SetActive(true);
    }
    public void CreateRoomPanleCancelCon()
    {//�� ����� ��� Ŭ���� ��Ȱ��ȭ
        createRoomPanel.SetActive(false);
    }
    public void RoomNumberJoinPanelCancelCon()
    {//���� ���� ��� Ŭ���� ��Ȱ��ȭ
        roomNumberJoinPanel.SetActive(false);
    }
    public void LockRoomPasswrodPanelActive(bool active)
    {//��й�ȣ�� �ɸ� �� ����� 
        lockRoomPanel.SetActive(active);
    }
    public void RoomJoinFaildeText(string message)
    {//UI �г� �Ǵ� �˾� Ȱ��ȭ
        roomJoinFaildePanel.SetActive(true);
        roomJoinFaildeText.text = message; //�ؽ�Ʈ ����
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

