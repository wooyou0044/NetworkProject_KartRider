using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.WSA;

public partial class TestCHMKart : MonoBehaviour
{
    [SerializeField] Transform backThrowPos;
    [SerializeField] Transform frontBarricadePos;
    [SerializeField] float shieldDuration;
    [SerializeField] float exitWaterFlyTime;
    [SerializeField] GameObject waterBombParticle;

    GameObject damage;

    float originAngularDrag;
    Vector3 playerPastPos;

    public bool isUsingShield { get; set; }
    bool isOneUsedShield;
    bool isExitWaterFly;

    GameObject waterFlyObject;

    ItemNetController itemNetCtrl;

    public bool isKartRotating { get; set; }

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
                //ThrowBanana();
                //_photonView.RPC("ThrowBanana", RpcTarget.Others);
                Vector3 spawnPos = backThrowPos.position;
                Quaternion spawnRot = backThrowPos.rotation;

                _photonView.RPC("ThrowBanana", RpcTarget.All, spawnPos, spawnRot);
                break;
            case ItemType.shield:
                isUsingShield = true;
                SetActiveShield(isUsingShield);
                kartBodyCtrl.SetShieldEffectActive(true);
                StartCoroutine(OffShield());
                break;
            case ItemType.barricade:
                // 임시로 => 1등 앞에 생겨야 함
                //MakeBarricade();
                itemNetCtrl.RequestBarricade(_photonView.ViewID);
                break;
            case ItemType.waterFly:
                // 임시로
                //StuckInWaterFly();
                itemNetCtrl.RequestWaterFly(_photonView.ViewID, rankManager.GetRank(), exitWaterFlyTime);
                break;
        }
    }

    //void ThrowBanana()
    //{
    //    GameObject banana = Resources.Load<GameObject>("Items/Banana");
    //    GameObject bananaPrefab = Instantiate(banana, backThrowPos.position, Quaternion.identity);
    //    Vector3 backwardDir = -transform.forward;
    //    bananaPrefab.transform.position += backwardDir + Vector3.up;
    //}

    [PunRPC]
    void ThrowBanana(Vector3 position, Quaternion rotation)
    {
        GameObject banana = Resources.Load<GameObject>("Items/Banana");
        GameObject bananaPrefab = Instantiate(banana, position, rotation);
        Vector3 backwardDir = -transform.forward;
        bananaPrefab.transform.position += backwardDir + Vector3.up;

        itemNetCtrl.RegisterItem(bananaPrefab);
    }

    IEnumerator ThreadBanana(float duration)
    {
        isKartRotating = true;
        isRacingStart = false;
        rigid.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rigid.centerOfMass = new Vector3(0, -2.0f, 0);
        originAngularDrag = rigid.angularDrag;
        rigid.angularDrag = 0.05f;
        float timer = 0f;
        while(timer < duration)
        {
            //Debug.Log(timer);
            rigid.AddTorque(Vector3.up * 700f, ForceMode.Impulse);
            timer += Time.deltaTime;
            yield return null;
        }
        rigid.angularVelocity = Vector3.zero;
        yield return new WaitForSeconds(1f);
        isRacingStart = true;
        rigid.angularDrag = originAngularDrag;
        isKartRotating = false;
    }

    void DamageItem(ItemType type)
    {
        switch(type)
        {
            case ItemType.banana:
                StartCoroutine(ThreadBanana(3f));
                break;
        }
    }

    [PunRPC]
    void SetShieldState(bool isActive)
    {
        isUsingShield = isActive;
    }

    void SetActiveShield(bool isActive)
    {
        if(_photonView.IsMine)
        {
            _photonView.RPC("SetShieldState", RpcTarget.All, isActive);
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
        SetActiveShield(isUsingShield);
    }

    [PunRPC]
    void MakeBarricade()
    {
        GameObject barricade = Resources.Load<GameObject>("Items/Barricade");
        GameObject barricadePrefab = Instantiate(barricade, frontBarricadePos.position, Quaternion.identity);

        // ItemNetController에 바리케이드 등록
        itemNetCtrl.RegisterItem(barricadePrefab);

        Vector3 forwardDir = transform.forward;
        barricadePrefab.transform.position += forwardDir * 7 + Vector3.up;
        Vector3 direction = transform.position - barricadePrefab.transform.position;
        direction.y = 0;
        barricadePrefab.transform.rotation = Quaternion.LookRotation(direction);
    }

    public void MakeDisableBarricade(GameObject disableObject)
    {
        itemNetCtrl.RequestDisableItem(disableObject);
    }

    [PunRPC]
    public void StuckInWaterFly()
    {
        playerPastPos = transform.position;
        GameObject waterFly = Resources.Load<GameObject>("Items/WaterFly");
        GameObject waterFlyPrefab = Instantiate(waterFly, transform.position, Quaternion.identity);
        waterFlyPrefab.transform.position += new Vector3(0, 5, 0);

        itemNetCtrl.RegisterItem(waterFlyPrefab);

        waterFlyObject = waterFlyPrefab;

        gameObject.transform.parent = waterFlyPrefab.transform;
        transform.localPosition = Vector3.zero;
        rigid.isKinematic = true;
        isRacingStart = false;
    }

    [PunRPC]
    void ExitInWaterFly()
    {
        //GameObject waterFly = itemNetCtrl.GetLastItem();
        if(waterFlyObject == null)
        {
            return;
        }

        gameObject.transform.parent = _playerParent;
        transform.position = playerPastPos;
        rigid.isKinematic = false;
        isRacingStart = true;

        Instantiate(waterBombParticle, waterFlyObject.transform.position, Quaternion.identity);

        itemNetCtrl.RequestDisableItem(waterFlyObject);

        waterFlyObject = null;
    }

    //void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    //{
    //    if(stream.IsWriting)
    //    {
    //        stream.SendNext(isUsingShield);
    //    }
    //    else
    //    {
    //        isUsingShield = (bool)stream.ReceiveNext();
    //    }
    //}

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
            itemNetCtrl.RequestDisableItem(other.gameObject);
            //other.gameObject.SetActive(false);
        }
    }
}
