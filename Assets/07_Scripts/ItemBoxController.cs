using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemBoxController : MonoBehaviour
{
    [SerializeField] GameObject destroyParticle;

    // ������ �����ϰ� ����

    public void GetItem()
    {
        Instantiate(destroyParticle, gameObject.transform.position + new Vector3(0,1,0), gameObject.transform.rotation);
        gameObject.SetActive(false);
    }
}
