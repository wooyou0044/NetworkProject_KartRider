using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;

/// <summary>
/// 플레이어 오브젝트 스크립트
/// 플레이어의 이미지, 닉네임, 액터넘버 (카트정보)등.. 표시 됨
/// </summary>
public class PlayerPanel : MonoBehaviourPun
{
    [Header("Player 정보")]

    public TMP_Text playerText;
    public TMP_Text PlayerNameText;
    public Image playerIcon;

    [Header("준비 완료 이미지")]
    public Image readyImage;

    private string targetTag = "Player";
    private int currentIndex = 0;

    [SerializeField] private List<GameObject> myTaggedObjects;
    [SerializeField] private RoomManager roomManager;
    [SerializeField] private CharacterList characterList;
    [SerializeField] private Button rightBtn;
    [SerializeField] private Button leftBtn;
    [SerializeField] private Button characterSelectBtn;
    private void Start()
    {
        roomManager = GameObject.FindObjectOfType<RoomManager>();
        characterList = GameObject.FindObjectOfType<CharacterList>();
        myTaggedObjects = new List<GameObject>();

        
        if (photonView.IsMine)
        {
            //이후에 들어온 사람도 확인을 해야하기 때문에 RpcTarget.AllBuffered사용
            GetComponent<PhotonView>().RPC("SetOwnInfo", RpcTarget.AllBuffered, PhotonNetwork.LocalPlayer);            
        }
        rightBtn.onClick.AddListener(CharacterChangeNextBtn);
        leftBtn.onClick.AddListener(PreviousCharacterBtn);
    }

    /// <summary>
    /// 플레이어 판넬의 생성은 RoomManager스크립트에서 PhotonNetwork.Instantiate로 생성
    /// 만들어 지면 PhotonView.IsMine으로 만든 사람이 PunRPC를 모두에게 뿌림
    /// 슬롯을 돌면서 빈 슬롯을 먼저 찾은 뒤 자신의 정보를 저장함,
    /// 버튼에 StartBtnClickTrigger()메서드를 저장한뒤 슬롯의 자식으로 위치를 옮김
    /// 저장이 완료 되면 위치 값과 크기를 조정함
    /// </summary>
    [PunRPC]
    public void SetOwnInfo(Player player)
    {
        if (roomManager == null)
        {
            roomManager = GameObject.Find("RoomManager").GetComponent<RoomManager>();
        }
        PlayerNameText.text = photonView.Controller.NickName;
        for (int i = 0; i < roomManager.playerSlots.Length; i++)
        {
            if (roomManager.playerSlots[i].playerPanel == null)
            {
                roomManager.playerSlots[i].playerPanel = GetComponent<PlayerPanel>();
                roomManager.playerSlots[i].actorNumber = player.ActorNumber;
                roomManager.roomUIManger.startBtn.onClick.AddListener(roomManager.playerSlots[i].playerPanel.StartBtnClickTrigger);
                roomManager.playerSlots[i].isReady = false;
                roomManager.playerSlots[i].playerPanel.characterSelectBtn = roomManager.roomUIManger.characterSelectBtn.GetComponent<Button>();
                roomManager.playerSlots[i].playerPanel.rightBtn = roomManager.roomUIManger.characterRightBtn.GetComponent<Button>();
                roomManager.playerSlots[i].playerPanel.leftBtn = roomManager.roomUIManger.characterLeftBtn.GetComponent<Button>();
                transform.SetParent(roomManager.playerSlots[i].transform);
                roomManager.UpdateAllPlayersReady();
                break;
            }
        }
        GetComponent<RectTransform>().anchorMin = Vector3.zero;
        GetComponent<RectTransform>().anchorMax = Vector3.one;
        GetComponent<RectTransform>().localPosition = Vector3.zero;
        GetComponent<PhotonView>().RPC("PlayerCharacterLoad", RpcTarget.AllBuffered);
    }
    /// <summary>
    /// 스타트 버튼
    /// 마스터 클라이언트가 아닐경우에만 활성화 
    /// 마스터는 따로 룸매니저에서 할당 됨
    /// </summary>
    public void StartBtnClickTrigger()
    {
        if (photonView.IsMine && !PhotonNetwork.IsMasterClient)
        {
            //이후에 들어온 사람도 확인을 해야하기 때문에 RpcTarget.AllBuffered사용
            GetComponent<PhotonView>().RPC("SetReady", RpcTarget.AllBuffered);
        }
    }
    /// <summary>
    /// StartBtnClickTrigger을 누르면 준비 완료 상태 이미지를 띄움
    /// 다시 누르면 이미지를 감춤
    /// 자신의 위치의 자신의 것만 확인하고 모두에게 정보를 알려줘야함.
    /// </summary>
    [PunRPC]
    public void SetReady()
    {
        //부모객체에서 찾기
        Transform parentTransform = transform.parent;
        if (roomManager == null)
        {
            roomManager = GameObject.Find("RoomManager").GetComponent<RoomManager>();
        }
        if (parentTransform != null)
        {
            //부모의 컴포넌트 가져오기
            PlayerSlot parentSlot = parentTransform.GetComponent<PlayerSlot>();
            if (parentSlot != null)
            {
                if (!readyImage.gameObject.activeSelf)
                {
                    parentSlot.isReady = true;
                    readyImage.gameObject.SetActive(true);
                    roomManager.UpdateAllPlayersReady();
                }
                else
                {
                    parentSlot.isReady = false;
                    roomManager.UpdateAllPlayersReady();
                    readyImage.gameObject.SetActive(false);
                }
            }
        }
    }

    //public void CharacterSelectBtn()
    //{
    //    if(photonView.IsMine)
    //    {
    //        .RPC("PlayerCharacterLoad", RpcTarget.AllBuffered);
    //    }
    //}
    [PunRPC]
    public void PlayerCharacterLoad()
    {
        myTaggedObjects.Clear();        
        PhotonView[] allPhotonViews = GameObject.FindObjectsOfType<PhotonView>();
        foreach (PhotonView pv in allPhotonViews)
        {
            if (pv.gameObject.CompareTag(targetTag)) // 내가 생성했고 태그가 일치하는 경우
            {
                pv.gameObject.gameObject.SetActive(false);
                if (pv.IsMine)
                {
                    myTaggedObjects.Add(pv.gameObject);
                    Debug.Log(pv.gameObject.name);
                }
            }
        }
        for (int i = 0; i < myTaggedObjects.Count; i++)
        {
            myTaggedObjects[0].gameObject.SetActive(true);
            if (i == 1)
            {
                myTaggedObjects[1].transform.rotation = Quaternion.Euler(0, 0, 0);
            }
        }
        CharacterChangeNextBtn();
        PreviousCharacterBtn();
    }

    public void CharacterChangeNextBtn()
    {
        myTaggedObjects[currentIndex].gameObject.SetActive(false);
        currentIndex = (currentIndex + 1) % myTaggedObjects.Count;
        Debug.Log(myTaggedObjects.Count + "몇개임?");
        myTaggedObjects[currentIndex].gameObject.SetActive(true);
        Debug.Log(myTaggedObjects.Count + "여기서 터지는거임?");
        Debug.Log(characterSelectBtn);
        characterSelectBtn.onClick.AddListener(() =>
        {
            Debug.Log("들어왔는지?");
            if (photonView.IsMine)
            {
                Debug.Log(myTaggedObjects[currentIndex].name + "선택 됨");                
                GetComponent<PhotonView>().RPC("CharacterSelect", RpcTarget.AllBuffered, PhotonNetwork.LocalPlayer.ActorNumber, myTaggedObjects[currentIndex].name);
                characterSelectBtn.onClick.RemoveAllListeners();
            }
        });    
    }
    public void PreviousCharacterBtn()
    {
        myTaggedObjects[currentIndex].gameObject.SetActive(false);
        currentIndex = (currentIndex - 1 + myTaggedObjects.Count) % myTaggedObjects.Count; // 첫 번째
        myTaggedObjects[currentIndex].gameObject.SetActive(true);
        characterSelectBtn.onClick.AddListener(() =>
        {
            if (photonView.IsMine)
            {

                GetComponent<PhotonView>().RPC("CharacterSelect", RpcTarget.AllBuffered, PhotonNetwork.LocalPlayer.ActorNumber, myTaggedObjects[currentIndex].name);
                characterSelectBtn.onClick.RemoveAllListeners();

            }
        });
    }    
    [PunRPC]
    public void CharacterSelect(int palyerActorNum, string characterName)
    {
        for (int i = 0; i < roomManager.playerSlots.Length; i++)
        {
            if (palyerActorNum == roomManager.playerSlots[i].actorNumber)
            {
                Debug.Log("들어옴?");
                roomManager.playerSlots[i].playerPanel.playerText.text = characterName;
            }
        }
    }
}