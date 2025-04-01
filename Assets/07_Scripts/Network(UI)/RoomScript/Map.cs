using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Map : MonoBehaviour
{
    public Image mapImage;
    public TMP_Text mapName;
    public Button mapButton;
    private RoomUIManager roomUIManager;

    private void Start()
    {
        roomUIManager = GameObject.Find("RoomUIManger").GetComponent<RoomUIManager>();
        mapButton = gameObject.GetComponent<Button>();
        mapImage = gameObject.GetComponent<Image>();
        mapName = gameObject.GetComponentInChildren<TMP_Text>();
        mapButton.onClick.AddListener(() => MapClickTrigger());
    }

    public void MapClickTrigger()
    { 
        roomUIManager.SelectMapName.text = mapName.text;
        roomUIManager.selectMapImage.sprite = mapImage.sprite;
        roomUIManager.roomMapNameText.text = mapName.text;
        roomUIManager.MapChangeBtn.image.sprite = mapImage.sprite;
    }
}
