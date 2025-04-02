using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class CharacterList : MonoBehaviourPun
{
    public CharacterSo[] characters;
    public GameObject kartPrefab;
    public RawImage characterImage;
    public Camera cam;
    private PhotonView pv;
    private int currentIndex = 0;
    public TestCHMKart testCHMKart;

    private void Awake()
    {
        
        pv = GetComponent<PhotonView>();
        characters = Resources.LoadAll<CharacterSo>("Character");

        DefaultPool pool = PhotonNetwork.PrefabPool as DefaultPool;
        if (pool != null)
        {
            //pool.ResourceCache.Add(kartPrefab.name, kartPrefab);
            foreach (var character in characters)
            {
                pool.ResourceCache.Add(character.characterName, character.characterPrefab);
            }
        }
        SetCharacterResources();
    }

    public void SetCharacterResources()
    {
        //GameObject kart = PhotonNetwork.Instantiate(kartPrefab.name, Vector3.zero, Quaternion.identity);
        // kart에 붙어 있는 Controller 가져오기
        //testCHMKart = kart.GetComponent<TestCHMKart>();

        foreach (var character in characters)
        {
            var createCharacter = PhotonNetwork.Instantiate(character.characterName, Vector3.zero, Quaternion.Euler(-90, -90, 0));
        }

    }

    //public void CharacterChangeNextBtn()
    //{
    //    characters[currentIndex].characterPrefab.gameObject.SetActive(false);
    //    currentIndex = (currentIndex + 1) % characters.Length;
    //    characters[currentIndex].characterPrefab.gameObject.SetActive(true);
    //}
    //public void PreviousCharacterBtn()
    //{
    //    characters[currentIndex].characterPrefab.gameObject.SetActive(false);
    //    currentIndex = (currentIndex - 1 + characters.Length) % characters.Length; // 첫 번째
    //    characters[currentIndex].characterPrefab.gameObject.SetActive(true);
    //}
}
