using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public partial class TestCHMKart : MonoBehaviour
{
    [SerializeField] Transform backThrowPos;
    [SerializeField] Transform frontBarricadePos;
    [SerializeField] float shieldDuration;

    GameObject damage;

    float originAngularDrag;

    bool isUsingShield;
    bool isOneUsedShield;

    private void HandleItemInput()
    {
        // LeftControl 키와 부스트 게이지 최대치 시 부스터 기본 발동
        if (Input.GetKeyDown(KeyCode.LeftControl) && inventory.itemNum > 0)
        {
            //StartBoost(boostDuration);
            MakeItemUseFunction(inventory.GetUsedItemType());
            inventory.RemoveItem();
            isItemUsed = true;
        }
        // 부스트 게이지 충전
        if (currentMotorInput != 0 || isDrifting)
        {
            ChargeBoostGauge();
        }

        if (boostGauge >= maxBoostGauge)
        {
            //isBoostCreate = true;
            inventory.isItemCreate = true;
            boostGauge = 0;
            chargeAmount = 0;

            //if (boostCount < 2)
            //{
            //    boostCount++;
            //}
            inventory.AddBoostItem();
        }
    }

    #region 카트 효과음
    void PlayDriftEffectSound()
    {
        audioSource.clip = driftAudioClip;
        audioSource.Play();
    }

    void PlayBoostEffectSound()
    {
        audioSource.clip = boostAudioClip;
        audioSource.loop = true;
        audioSource.Play();
    }

    #endregion

    void MakeItemUseFunction(ItemType type)
    {
        switch (type)
        {
            case ItemType.booster:
                StartBoost(boostDuration);
                break;
            case ItemType.banana:
                ThrowBanana();
                break;
            case ItemType.shield:
                isUsingShield = true;
                kartBodyCtrl.SetShieldEffectActive(true);
                StartCoroutine(OffShield());
                break;
            case ItemType.barricade:
                // 임시로 => 1등 앞에 생겨야 함
                MakeBarricade();
                break;
        }
    }

    void ThrowBanana()
    {
        //GameObject banana = Instantiate(inventory.haveItem[0].itemObject, backThrowPos.position, Quaternion.identity);
        //Instantiate(inventory.haveItem[0].itemObject, backThrowPos.position, Quaternion.identity);
        GameObject banana = Resources.Load<GameObject>("Items/Banana");
        GameObject bananaPrefab = Instantiate(banana, backThrowPos.position, Quaternion.identity);
        Vector3 backwardDir = -transform.forward;
        bananaPrefab.transform.position += backwardDir + Vector3.up;
    }

    IEnumerator ThreadBanana(float duration)
    {
        isRacingStart = false;
        originAngularDrag = rigid.angularDrag;
        rigid.angularDrag = 0.05f;
        float timer = 0f;
        while(timer < duration)
        {
            Debug.Log(timer);
            rigid.AddTorque(Vector3.up * 500f, ForceMode.Impulse);
            timer += Time.deltaTime;
            yield return null;
        }
        rigid.angularVelocity = Vector3.zero;
        yield return new WaitForSeconds(1f);
        isRacingStart = true;
        rigid.angularDrag = originAngularDrag;
    }

    void DamageItem(ItemType type)
    {
        switch(type)
        {
            case ItemType.banana:
                StartCoroutine(ThreadBanana(5f));
                break;
        }
    }

    IEnumerator OffShield()
    {
        if(isUsingShield == false)
        {
            yield break;
        }
        float timer = 0;
        while(timer < shieldDuration)
        {
            if (isUsingShield == false)
            {
                yield break;
            }
            timer += Time.deltaTime;
            yield return null;
        }
        isUsingShield = false;
    }

    public void MakeBarricade()
    {
        GameObject barricade = Resources.Load<GameObject>("Items/Barricade");
        GameObject barricadePrefab = Instantiate(barricade, frontBarricadePos.position, Quaternion.identity);
       
        Vector3 forwardDir = transform.forward;
        barricadePrefab.transform.position += forwardDir * 7 + Vector3.up;
        Vector3 direction = transform.position - barricadePrefab.transform.position;
        direction.y = 0;
        barricadePrefab.transform.rotation = Quaternion.LookRotation(direction);
    }

    void OnTriggerEnter(Collider other)
    {
        if(other == null)
        {
            return;
        }
        if(other.CompareTag("ItemBox"))
        {
            ItemController ctrl = other.GetComponent<ItemController>();
            if(isUsingShield == false)
            {
                DamageItem(ctrl.item.itemType);
            }
            else
            {
                isUsingShield = false;
            }
            other.gameObject.SetActive(false);
        }
    }
}
