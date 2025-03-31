using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KartInventory : MonoBehaviour
{
    [SerializeField] ItemManager boostItem;

    public ItemManager[] haveItem { get; private set; }

    int size = 2;
    public int itemNum { get; private set; }

    public int boostCount { get; set; }
    public bool isItemCreate { get; set; }

    Sprite removeItemImage;

    void Awake()
    {

    }

    void Start()
    {
        haveItem = new ItemManager[size];
        itemNum = 0;
        isItemCreate = false;
    }

    void Update()
    {
    }

    public void AddInventory(ItemManager getItem)
    {
        if(itemNum < size)
        {
            haveItem[itemNum] = getItem;
            itemNum++;
        }
    }

    public void AddBoostItem()
    {
        if (itemNum < size)
        {
            haveItem[itemNum] = boostItem;
            itemNum++;
        }
    }

    public void RemoveItem()
    {
        if(itemNum > 0)
        {
            itemNum--;
            removeItemImage = haveItem[itemNum].itemImage;
            if(itemNum == 1)
            {
                haveItem[itemNum - 1] = haveItem[itemNum];
            }
            haveItem[itemNum] = null;
        }
    }

    public Sprite GetItemImage()
    {
        if(removeItemImage != null)
        {
            return removeItemImage;
        }
        return null;
    }

    public ItemType GetUsedItemType()
    {
        return haveItem[0].itemType;
    }
}
