using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public partial class TestCHMKart : MonoBehaviour
{
    [SerializeField] Transform backThrowPos;

    GameObject damage;

    float originAngularDrag;

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
        }
    }

    void ThrowBanana()
    {
        GameObject banana = Instantiate(inventory.haveItem[0].itemObject, backThrowPos.position, Quaternion.identity);
        Vector3 backwardDir = -transform.forward;
        banana.transform.position += backwardDir * 2 + Vector3.up;
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

    void OnTriggerEnter(Collider other)
    {
        if(other == null)
        {
            return;
        }
        if(other.CompareTag("ItemBox"))
        {
            ItemController ctrl = other.GetComponent<ItemController>();
            DamageItem(ctrl.item.itemType);
            other.gameObject.SetActive(false);
        }
    }
}
