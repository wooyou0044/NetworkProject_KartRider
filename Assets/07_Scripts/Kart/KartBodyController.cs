using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KartBodyController : MonoBehaviour
{
    [SerializeField] GameObject[] lampTrail;
    [SerializeField] Transform[] boostEffTransform;
    [SerializeField] GameObject boostEffect;
    [SerializeField] GameObject collisionSpark;
    [SerializeField] Transform collisionTransform;
    [SerializeField] Transform[] driftSparkWheelPos;
    [SerializeField] Transform shieldEffectBodyPos;
    [SerializeField] Transform confettiPos;
    [SerializeField] GameObject driftSpark;
    [SerializeField] GameObject windEffect;
    [SerializeField] GameObject shieldEffect;
    [SerializeField] GameObject confettiEffect;

    GameObject spark;
    GameObject wind;
    public GameObject shield { get; set; }
    GameObject[] driftSparkObject;
    GameObject confetti;

    int input;

    void Start()
    {
        driftSparkObject = new GameObject[driftSparkWheelPos.Length];

        foreach (Transform parentTrans in boostEffTransform)
        {
            GameObject boost = Instantiate(boostEffect, parentTrans);
            boost.transform.eulerAngles += new Vector3(0, 0, -90);
        }
        spark = Instantiate(collisionSpark, collisionTransform);
        wind = Instantiate(windEffect, collisionTransform);
        wind.transform.eulerAngles += new Vector3(0, -90, 0);
        for(int i=0; i< driftSparkWheelPos.Length; i++)
        {
            driftSparkObject[i] = Instantiate(driftSpark, driftSparkWheelPos[i]);
            driftSparkObject[i].SetActive(false);
        }
        shield = Instantiate(shieldEffect, shieldEffectBodyPos);
        //shield.transform.position -= new Vector3(0, 0.05f, 0);
        confetti = Instantiate(confettiEffect, confettiPos);
        confetti.transform.eulerAngles += new Vector3(0, 90, 0);

        SetLampTrailActive(false);
        SetBoostEffectActive(false);
        SetCollisonSparkActive(false);
        SetBoostWindEffectActive(false);
        SetShieldEffectActive(false);
        SetConfettiEffectActive(false);
    }

    void Update()
    {
        
    }

    public void SetLampTrailActive(bool isActive)
    {
        foreach(GameObject lampObject in lampTrail)
        {
            lampObject.GetComponent<TrailRenderer>().enabled = isActive;
        }
    }

    public void SetBoostEffectActive(bool isActive)
    {
        foreach(Transform transform in boostEffTransform)
        {
            transform.GetChild(0).gameObject.SetActive(isActive);
        }
    }

    public void SetCollisonSparkActive(bool isActive)
    {
        spark.SetActive(isActive);
        if (isActive == true)
        {
            StartCoroutine(SparkOff());
        }
    }

    IEnumerator SparkOff()
    {
        yield return new WaitForSeconds(1f);
        SetCollisonSparkActive(false);
    }

    public void SetDriftSparkActive(bool isActive, float steerInput)
    {
        // 왼쪽 방향키 + Shift 키 눌렀을 때 
        if(steerInput < 0)
        {
            input = 1;
        }
        // 오른쪽 방향키 + Shift 키 눌렀을 때
        else if(steerInput > 0)
        {
            input = 0;
        }

        driftSparkObject[input].SetActive(isActive);

        if (isActive == false)
        {
            if (driftSparkObject[0].activeSelf == true)
            {
                driftSparkObject[0].SetActive(false);
            }
            if (driftSparkObject[1].activeSelf == true)
            {
                driftSparkObject[1].SetActive(false);
            }
        }
    }

    public void SetBoostWindEffectActive(bool isActive)
    {
        wind.SetActive(isActive);
    }

    public void SetShieldEffectActive(bool isActive)
    {
        shield.SetActive(isActive);
    }

    public void SetConfettiEffectActive(bool isActive)
    {
        confetti.SetActive(isActive);  
    }
}
