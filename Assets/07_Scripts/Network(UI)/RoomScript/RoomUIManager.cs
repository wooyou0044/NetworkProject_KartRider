using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RoomUIManager : MonoBehaviour
{
    [Header("�� �������̼�")]
    [SerializeField] public Button roomTitleChangeBtn;
    [SerializeField] public TMP_Text roomNumberText;
    [SerializeField] public TMP_Text roomNameText;
    [SerializeField] public TMP_Text roomPasswordText;
    [SerializeField] public TMP_Text roomMapNameText;
    [SerializeField] public Image roomMapeImg;
       
    [Header("�� �̸� ����")]
    [SerializeField] public GameObject roomInfoChangePanel;
    [SerializeField] public TMP_InputField roomNameChangeField;
    [SerializeField] public TMP_InputField roomPasswordChangeField;

    [Header("�� ��й�ȣ ������")]
    [SerializeField] public Image passwordGroup;

    [Header("��(Ʈ������) ��ư")]
    [SerializeField] public Button MapChangeBtn;    

    [Header("�غ� ��ư")]
    [SerializeField] public Button startBtn;
    [SerializeField] public Button readyBtn;
    [SerializeField] public Button readyCanCelBtn;

    [Header("�� ������ ��ư")]
    [SerializeField] public Button exitBtn;

    [Header("īƮ �ǳ�")]
    [SerializeField] public GameObject kartPanel;
    [SerializeField] public Button kartRightBtn;
    [SerializeField] public Button kartLeftBtn;
    [SerializeField] public Button kartSelectBtn;
    [SerializeField] public Image kartImg;
    [SerializeField] public Button kartPanelBtn;

    [Header("ĳ���� �ǳ�")]
    [SerializeField] public GameObject characterPanel;
    [SerializeField] public Button characterRightBtn;
    [SerializeField] public Button characterLeftBtn;
    [SerializeField] public Button characterSelectBtn;
    [SerializeField] public Image characterImg;
    [SerializeField] public Button characterPanelBtn;

    public void SetPasswordUI(bool hasPassword)
    {
        passwordGroup.gameObject.SetActive(hasPassword);
    }

    public void RoomInfoChangePanelBtn()
    {
        roomInfoChangePanel.gameObject.SetActive(true);
    }
    public void RoominfoChangeCancelBtn()
    {
        roomInfoChangePanel.gameObject.SetActive(false);
    }
    public void SetRoominfoChangeBtn()
    {
        
    }



}
