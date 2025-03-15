using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;

public class TitleUI : MonoBehaviour
{
    [Header("로그인 파넬 모음")]
    public GameObject loginPanel;
    public TMP_InputField loginEmailField;
    public TMP_InputField loginpasswordField;
    public Button loginBtn;
    public Button signUpBtn;

    [Header("회원가입 판넬 모음")]
    public GameObject signUpPanel;
    public TMP_InputField signUpEmailField;
    public TMP_InputField signUpPasswordField;
    public Button signUpLoingToBtn;
    public Button signUpCompleteBtn;

    [Header("닉네임 생성 신규 가입자만")]
    public GameObject createNickNamePanel;
    public TMP_InputField createNickNameField;
    public Button createNickNameBtn;
    public Button createNickNameCancelBtn;

    [Header("메세지 모음")]
    public TMP_Text errorMessage;
    public TMP_Text successMessage;

    private void Start()
    {
        loginPanel.SetActive(true);
        signUpPanel.SetActive(false);
        createNickNamePanel.SetActive(false);
        errorMessage.gameObject.SetActive(false);
        successMessage.gameObject.SetActive(false);
    }

    public void SignUpButtonCon()
    {
        loginEmailField.text = "";
        loginpasswordField.text = "";
        errorMessage.gameObject.SetActive(false);
        loginPanel.SetActive(false);
        signUpPanel.SetActive(true);
    }

    public void LoginToButtonCon()
    {
        signUpEmailField.text = "";
        signUpPasswordField.text = "";
        loginPanel.SetActive(true);
        signUpPanel.SetActive(false);
    }
    public void CreateCancelBottenCon()
    {
        errorMessage.text = "";
        createNickNameField.text = "";
        createNickNamePanel.gameObject.SetActive(false);
        loginPanel.gameObject.SetActive(true);
    }
}
