using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class TempCarScript : MonoBehaviour
{
    private Transform _playerParent;
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
    
    void Start()
    {
        _playerParent = GameObject.Find("Players").transform;
        virtualCamera = FindObjectOfType<CinemachineVirtualCamera>();

        if (_photonView.IsMine)
        {
            virtualCamera.LookAt = transform;
            virtualCamera.Follow = transform;
        }
        
        transform.parent = _playerParent;
    }

    void Update()
    {
        if (!_photonView.IsMine)
        {
            return;
        }
        
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
