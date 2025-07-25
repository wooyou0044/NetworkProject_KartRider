using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterList : MonoBehaviour
{
    public CharacterSo[] characters;
    public GameObject kartPrefab;
    public RawImage characterImage;
    
    public List<GameObject> characterListPrefab;
    public int currentIndex = 0;
    private void Awake()
    {
        characters = Resources.LoadAll<CharacterSo>("Character");
        characterListPrefab = new List<GameObject>();

        SetCharacterResources();
        
    }

    public void SetCharacterResources()
    {
        //GameObject kart = PhotonNetwork.Instantiate(kartPrefab.name, Vector3.zero, Quaternion.identity);
        // kart�� �پ� �ִ� Controller ��������
        //testCHMKart = kart.GetComponent<TestCHMKart>();

        foreach (var character in characters)
        {
            var characterObj = Instantiate(character.characterPrefab, Vector3.zero, Quaternion.Euler(-90, -90, 0));
            characterObj.gameObject.SetActive(false);
            characterListPrefab.Add(characterObj);            
        }
        characterListPrefab[currentIndex].gameObject.SetActive(true);
    }

    public void CharacterChangeNextBtn()
    {
        characterListPrefab[currentIndex].gameObject.SetActive(false);
        currentIndex = (currentIndex + 1) % characterListPrefab.Count;
        characterListPrefab[currentIndex].gameObject.SetActive(true);
    }
    public void PreviousCharacterBtn()
    {
        characterListPrefab[currentIndex].gameObject.SetActive(false);
        currentIndex = (currentIndex - 1 + characterListPrefab.Count) % characterListPrefab.Count;
        characterListPrefab[currentIndex].gameObject.SetActive(true);
    }
    public CharacterSo SelectedCharacter()
    {
        if (SceneCont.Instance != null)
        {
            SceneCont.Instance.SelectedCharacter = characters[currentIndex];
        }
        Debug.Log(characterListPrefab[currentIndex].gameObject.name + "������ ������Ʈ �̸�");
        return characters[currentIndex];
    }

}
