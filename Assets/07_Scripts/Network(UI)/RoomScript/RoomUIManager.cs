using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RoomUIManager : MonoBehaviour
{
    [Header("방 인포메이션")]
    [SerializeField] public Button roomTitleChangeBtn;
    [SerializeField] public TMP_Text roomNumberText;
    [SerializeField] public TMP_Text roomNameText;
    [SerializeField] public TMP_Text roomPasswordText;
    [SerializeField] public TMP_Text roomMapNameText;
    [SerializeField] public Image roomMapeImg;
       
    [Header("방 이름 변경")]
    [SerializeField] public GameObject roomInfoChangePanel;
    [SerializeField] public TMP_InputField roomNameChangeField;
    [SerializeField] public TMP_InputField roomPasswordChangeField;

    [Header("방 비밀번호 아이콘")]
    [SerializeField] public Image passwordGroup;

    [Header("맵(트랙변경) 버튼")]
    [SerializeField] public Button MapChangeBtn;    

    [Header("준비 버튼")]
    [SerializeField] public Button startBtn;
    [SerializeField] public Button readyBtn;
    [SerializeField] public Button readyCanCelBtn;

    [Header("방 나가기 버튼")]
    [SerializeField] public Button exitBtn;

    [Header("카트 판넬")]
    [SerializeField] public GameObject kartPanel;
    [SerializeField] public Button kartRightBtn;
    [SerializeField] public Button kartLeftBtn;
    [SerializeField] public Button kartSelectBtn;
    [SerializeField] public Image kartImg;
    [SerializeField] public Button kartPanelBtn;

    [Header("캐릭터 판넬")]
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
