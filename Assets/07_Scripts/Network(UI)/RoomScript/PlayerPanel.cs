using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;
using UnityEngine.ProBuilder.Shapes;
using UnityEngine.TextCore.Text;

/// <summary>
/// �÷��̾� ������Ʈ ��ũ��Ʈ
/// �÷��̾��� �̹���, �г���, ���ͳѹ� (īƮ����)��.. ǥ�� ��
/// </summary>
public class PlayerPanel : MonoBehaviourPun
{
    [Header("Player ����")]
    public TMP_Text PlayerNameText;
    public Image playerIcon;

    [Header("�غ� �Ϸ� �̹���")]
    public Image readyImage;

    [SerializeField] private RoomManager roomManager;
    [SerializeField] private CharacterList characterList;
    private void Start()
    {
        roomManager = GameObject.FindObjectOfType<RoomManager>();
        characterList = GameObject.FindObjectOfType<CharacterList>();
        if (photonView.IsMine)
        {
            //���Ŀ� ���� ����� Ȯ���� �ؾ��ϱ� ������ RpcTarget.AllBuffered���
            GetComponent<PhotonView>().RPC("SetOwnInfo", RpcTarget.AllBuffered, PhotonNetwork.LocalPlayer);
            OnClickGameObjSelectBtn();
        }
    }

    /// <summary>
    /// �÷��̾� �ǳ��� ������ RoomManager��ũ��Ʈ���� PhotonNetwork.Instantiate�� ����
    /// ����� ���� PhotonView.IsMine���� ���� ����� PunRPC�� ��ο��� �Ѹ�
    /// ������ ���鼭 �� ������ ���� ã�� �� �ڽ��� ������ ������,
    /// ��ư�� StartBtnClickTrigger()�޼��带 �����ѵ� ������ �ڽ����� ��ġ�� �ű�
    /// ������ �Ϸ� �Ǹ� ��ġ ���� ũ�⸦ ������
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
                roomManager.roomUIManger.characterSelectBtn.onClick.AddListener(roomManager.playerSlots[i].playerPanel.OnClickGameObjSelectBtn);
                transform.SetParent(roomManager.playerSlots[i].transform);
                roomManager.UpdateAllPlayersReady();
                break;
            }
        }
        GetComponent<RectTransform>().anchorMin = Vector3.zero;
        GetComponent<RectTransform>().anchorMax = Vector3.one;
        GetComponent<RectTransform>().localPosition = Vector3.zero;
    }
    /// <summary>
    /// ��ŸƮ ��ư
    /// ������ Ŭ���̾�Ʈ�� �ƴҰ�쿡�� Ȱ��ȭ 
    /// �����ʹ� ���� ��Ŵ������� �Ҵ� ��
    /// </summary>
    public void StartBtnClickTrigger()
    {
        if (photonView.IsMine && !PhotonNetwork.IsMasterClient)
        {
            //���Ŀ� ���� ����� Ȯ���� �ؾ��ϱ� ������ RpcTarget.AllBuffered���
            GetComponent<PhotonView>().RPC("SetReady", RpcTarget.AllBuffered);
        }
    }
    /// <summary>
    /// StartBtnClickTrigger�� ������ �غ� �Ϸ� ���� �̹����� ���
    /// �ٽ� ������ �̹����� ����
    /// �ڽ��� ��ġ�� �ڽ��� �͸� Ȯ���ϰ� ��ο��� ������ �˷������.
    /// </summary>
    [PunRPC]
    public void SetReady()
    {
        //�θ�ü���� ã��
        Transform parentTransform = transform.parent;
        if (roomManager == null)
        {
            roomManager = GameObject.Find("RoomManager").GetComponent<RoomManager>();
        }
        if (parentTransform != null)
        {
            //�θ��� ������Ʈ ��������
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
    public void OnClickGameObjSelectBtn()
    {
        if (photonView.IsMine)
        {            
            GetComponent<PhotonView>().RPC("GameObjSelect", RpcTarget.AllBuffered, characterList.SelectedCharacter().name);
        }
    }
    [PunRPC]
    public void GameObjSelect(string characterName)
    {
        CharacterSo[] characters = Resources.LoadAll<CharacterSo>("Character");
        CharacterSo selectedChar = null;
        foreach (var charac in characters)
        {
            if (charac.name.Equals(characterName))
            {
                selectedChar = charac;
            }
        } 
            
        Transform parentTransform = transform.parent;
        if (parentTransform != null)
        {
            PlayerSlot parentSlot = parentTransform.GetComponent<PlayerSlot>();
            if (parentSlot != null)
            {
                Debug.Log(parentSlot + "�� ����");
                parentSlot.playerPanel.playerIcon.sprite = selectedChar.characterIcon;   
            }
        }
    }
}
