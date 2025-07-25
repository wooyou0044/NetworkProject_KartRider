using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType
{
    banana,
    shield,
    barricade,
    waterFly,
    booster,
}

[CreateAssetMenu(fileName = "NewItemData", menuName = "ItemData", order = 1)]
public class ItemManager : ScriptableObject
{
    public string itemName;
    public ItemType itemType;
    public GameObject itemObject;
    public Sprite itemImage;
}
