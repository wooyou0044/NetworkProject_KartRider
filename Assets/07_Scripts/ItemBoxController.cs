using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;

public class ItemBoxController : MonoBehaviour
{
    [SerializeField] GameObject destroyParticle;
    public ItemManager[] items;

    ItemType type;
    ItemManager curItem;
    KartInventory inventory;

    int count;

    // ���� ���� OnTriggerEnter �̺�Ʈ�� �߰����� �̺�Ʈ ����
    [HideInInspector]
    public UnityEvent<GameObject> onTouchItemBox;
    
    void Start()
    {
        // ������ ����
        count = (int)ItemType.booster + 1;
        int rand = Random.Range(0, count);
        curItem = items[rand];

        //curItem = items[3];

        //int rand = Random.Range(0, 3);
        //curItem = items[2];

        //curItem = items[2];

        //count = (int)ItemType.booster + 1;
        //int rand = Random.Range(1, 3);
        //curItem = items[rand];
    }

    public void InactiveItemBox()
    {
        Instantiate(destroyParticle, gameObject.transform.position + new Vector3(0,1,0), gameObject.transform.rotation);
        gameObject.SetActive(false);
    }
    
    public IEnumerator RespawnItemBox(float respawnTime)
    {
        float time = 0;
        float increaseTime = 0.5f;
        
        yield return new WaitForSeconds(respawnTime);

        Vector3 initialScale = new Vector3(25, 25, 25);
        Vector3 toIncrease = new Vector3(50, 50, 50);
        
        gameObject.SetActive(true);
        gameObject.transform.localScale = initialScale;        

        while (time < increaseTime)
        {
            time += Time.deltaTime;
            gameObject.transform.localScale = Vector3.Lerp(initialScale, toIncrease, time/increaseTime);
            yield return new WaitForFixedUpdate();
        }

        gameObject.transform.localScale = toIncrease;
    }    

    void OnTriggerEnter(Collider other)
    {
        PhotonView pv = other.gameObject.GetPhotonView();

        if (pv == null || !pv.IsMine)
        {
            return;
        }
        
        // �ڽ� ȹ���� īƮ �κ��丮�� ������
        inventory = other.GetComponent<KartInventory>();
        inventory.isItemCreate = true;
        inventory.AddInventory(curItem);
        
        onTouchItemBox.Invoke(gameObject);
    }
}
