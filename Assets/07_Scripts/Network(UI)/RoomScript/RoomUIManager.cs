using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

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

    //방에 비밀번호가 걸리면 켜지는 이미지
    public void SetPasswordUI(bool hasPassword)
    {
        passwordGroup.gameObject.SetActive(hasPassword);
    }
    //룸안에서 방 제목과 방 패스워드를 변경할 수 있는 판넬을 켬
    //방장만 설정 가능
    public void RoomInfoChangePanelBtn()
    {
        if(PhotonNetwork.IsMasterClient)
        {
            roomInfoChangePanel.gameObject.SetActive(true);
        }
    }
    //룸안에서 방 제목과 방 패스워드를 변경할 수 있는 판넬을 끔
    public void RoominfoChangeCancelBtn()
    {
        roomInfoChangePanel.gameObject.SetActive(false);
    }
    //룸 안에서 맵 트랙을 바꿀 수 있는 판넬을 켬
    //방장만 설정 가능
    public void TrackSelectPanelBtn()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            trackSelectPanel.gameObject.SetActive(true);
        }
    }
    //룸 안에서 맵 트랙을 바꿀 수 있는 판넬을 끔
    public void TrackSelectBtn()
    {
        trackSelectPanel.gameObject.SetActive(false);
    }
    //방장에게 알려주는 메세지 게임 시작을 눌렀을 때 상황에 따라 다른 메세지 출력
    public void progressMessage(string massage)
    {
        progressMessageText.text = massage;
    }
}
