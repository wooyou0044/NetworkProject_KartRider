using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

public class RoomUIManager : MonoBehaviour
{
    [Header("방 인포메이션")]
    public Button roomTitleChangeBtn;
    public TMP_Text roomNumberText;
    public TMP_Text roomNameText;
    public TMP_Text roomPasswordText;
    public TMP_Text roomMapNameText;
    public Image roomMapeImg;
       
    [Header("방 이름 변경")]
    public GameObject roomInfoChangePanel;
    public TMP_InputField roomNameChangeField;
    public TMP_InputField roomPasswordChangeField;

    [Header("준비 버튼")]
    public Button startBtn;
    public TMP_Text startBtnText;

    [Header("방 비밀번호 아이콘")]
    public Image passwordGroup;

    [Header("맵(트랙변경) 버튼")]
    public Button MapChangeBtn;

    [Header("방 나가기 버튼")]
    public Button exitBtn;

    [Header("카트 판넬")]
    public GameObject kartPanel;
    public Button kartRightBtn;
    public Button kartLeftBtn;
    public Button kartSelectBtn;
    public Image kartImg;
    public Button kartPanelBtn;

    [Header("캐릭터 판넬")]
    public GameObject characterPanel;
    public Button characterRightBtn;
    public Button characterLeftBtn;
    public Button characterSelectBtn;
    public Image characterImg;
    public Button characterPanelBtn;

    [Header("트랙 선택 판넬")]
    public GameObject trackSelectPanel;
    public MapList mapListPanel;
    public Image selectMapImage;
    public TMP_Text SelectMapName;
    public Button trackSelectBtn;

    [Header("안내메세지 판넬")]
    public GameObject progressMessagePanel;
    public TMP_Text progressMessageText;

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
    
    public void TrackSelectPanelBtn()
    {
        trackSelectPanel.gameObject.SetActive(true);
    }   
    public void TrackSelectBtn()
    {
        trackSelectPanel.gameObject.SetActive(false);
    }

    public void progressMessage(string massage)
    {
        progressMessageText.text = massage;
    }
}
