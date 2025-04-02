using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterList : MonoBehaviour
{
    public CharacterSo[] characters;
    public GameObject kartPrefab;
    public RawImage characterImage;
    //public Camera cam;
    public List<GameObject> characterListPrefab;
    public int currentIndex = 0;
    public Button characterSelectBtnButton;
    private void Awake()
    {
        characters = Resources.LoadAll<CharacterSo>("Character");
        characterListPrefab = new List<GameObject>();

        SetCharacterResources();
        SelectedCharacter(characterListPrefab[currentIndex]);
        characterSelectBtnButton.onClick.AddListener(() =>
        {
            SelectedCharacter(characterListPrefab[currentIndex]);
        });
    }

    public void SetCharacterResources()
    {
        //GameObject kart = PhotonNetwork.Instantiate(kartPrefab.name, Vector3.zero, Quaternion.identity);
        // kart에 붙어 있는 Controller 가져오기
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
        characterSelectBtnButton.onClick.RemoveAllListeners();
        characterSelectBtnButton.onClick.AddListener(() =>
        {
            SelectedCharacter(characterListPrefab[currentIndex]);
        });
    }
    public void PreviousCharacterBtn()
    {
        characterListPrefab[currentIndex].gameObject.SetActive(false);
        currentIndex = (currentIndex - 1 + characterListPrefab.Count) % characterListPrefab.Count;
        characterListPrefab[currentIndex].gameObject.SetActive(true);
        characterSelectBtnButton.onClick.RemoveAllListeners();
        characterSelectBtnButton.onClick.AddListener(() =>
        {
            SelectedCharacter(characterListPrefab[currentIndex]);
        });
    }
    public void SelectedCharacter(GameObject gameObject)
    {
        Debug.Log(gameObject.name + "선택한 오브젝트 이름");
    }
}
