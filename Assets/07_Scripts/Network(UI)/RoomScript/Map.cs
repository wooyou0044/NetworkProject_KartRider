using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 맵을 설정하고, 만들어진 맵의 정보를 토대로 해당 오브젝트를 셋팅
/// 해당 맵(버튼)을 클릭하면 방의 정보가 바뀌도록 설정
/// </summary>
public class Map : MonoBehaviour
{
    public Image mapImage;
    public TMP_Text mapName;
    public Button mapButton;
    private RoomUIManager roomUIManager;

    /// <summary>
    /// 버튼 셋팅하기
    /// </summary>
    private void Start()
    {
        roomUIManager = GameObject.Find("RoomUIManger").GetComponent<RoomUIManager>();
        mapButton = gameObject.GetComponent<Button>();
        mapImage = gameObject.GetComponent<Image>();
        mapName = gameObject.GetComponentInChildren<TMP_Text>();
        mapButton.onClick.AddListener(() => MapClickTrigger()); //버튼연결
    }
    /// <summary>
    /// 버튼에 연결되어 해당 맵을 클릭하면 방의 정보를 바꿈
    /// </summary>
    public void MapClickTrigger()
    { 
        roomUIManager.SelectMapName.text = mapName.text;
        roomUIManager.selectMapImage.sprite = mapImage.sprite;
        roomUIManager.roomMapNameText.text = mapName.text;
        roomUIManager.MapChangeBtn.image.sprite = mapImage.sprite;
    }
}
