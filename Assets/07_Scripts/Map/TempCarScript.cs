using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Photon.Pun;
using UnityEngine;

public class TempCarScript : MonoBehaviour
{
    private Transform _tr;
    private PhotonView _photonView;
    private float speed = 10f;
    private float rotate = 50f;

    public CinemachineVirtualCamera virtualCamera;

    void Awake()
    {
        _tr = gameObject.transform;
        _photonView = GetComponent<PhotonView>();        
    }
    
    // Start is called before the first frame update
    void Start()
    {
        virtualCamera = FindObjectOfType<CinemachineVirtualCamera>();

        if (_photonView.IsMine)
        {
            virtualCamera.LookAt = transform;
            virtualCamera.Follow = transform;
        }        
    }

    // Update is called once per frame
    void Update()
    {
        float h = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");

        if (h != 0 || y != 0)
        {
            Vector3 movePos =  new Vector3(-y * speed * Time.deltaTime, 0, 0);
            _tr.Translate(movePos);
            _tr.Rotate(Vector3.up * (rotate * h * Time.deltaTime));
        }
    }
}
