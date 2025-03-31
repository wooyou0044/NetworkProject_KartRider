using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemBoxController : MonoBehaviour
{
    [SerializeField] GameObject destroyParticle;
    public ItemManager[] items;

    ItemType type;
    ItemManager curItem;
    KartInventory inventory;

    int count;

    void Start()
    {
        // ������ ����
        count = (int)ItemType.booster + 1;
        int rand = Random.Range(0, count);
        curItem = items[rand];

        //curItem = items[3];

        //int rand = Random.Range(0, 3);
        //curItem = items[2];
    }

    public void GetItem()
    {
        Instantiate(destroyParticle, gameObject.transform.position + new Vector3(0,1,0), gameObject.transform.rotation);
        gameObject.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            // �ڽ� ������ �� ��ƼŬ
            GetItem();

            // �ڽ� ȹ���� īƮ �κ��丮�� ������
            inventory = other.GetComponent<KartInventory>();
            inventory.isItemCreate = true;
            inventory.AddInventory(curItem);
        }
    }
}
