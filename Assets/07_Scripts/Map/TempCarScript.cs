using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

/*
 ToDo 실제 카트에도 적용할 리스트
 - Rigidbody Component 게터 : public Rigidbody Rigidbody => _rigidbody;
 - PlayerParent : 플레이어 찾기 편하도록 _playerParent = GameObject.Find("Players").transform;
 - 포톤 뷰 관련 컴포넌트들 달아주기, 뷰, 트랜스폼 뷰, 리지드 바디 뷰
 */
public class TempCarScript : MonoBehaviour
{
    private Transform _playerParent;
    private Transform _tr;
    private PhotonView _photonView;
    private Rigidbody _rigidbody;
    
    private float speed = 10f;
    private float rotate = 50f;
    
    public CinemachineVirtualCamera virtualCamera;

    public Rigidbody Rigidbody => _rigidbody;


    void Awake()
    {
        _tr = gameObject.transform;
        _photonView = GetComponent<PhotonView>();
        _rigidbody = GetComponent<Rigidbody>();
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
