using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TitleUI : MonoBehaviour
{
    [Header("로그인 파넬 모음")]
    public GameObject loginPanel;
    public TMP_InputField loginEmailField;
    public TMP_InputField loginpasswordField;    

    [Header("회원가입 판넬 모음")]
    public GameObject signUpPanel;
    public TMP_InputField signUpEmailField;
    public TMP_InputField signUpPasswordField;
    public Button signUpLoingToBtn;
    public Button signUpCompleteBtn;

    [Header("닉네임 생성 신규 가입자만")]
    public GameObject createNickNamePanel;
    public TMP_InputField createNickNameField;

    [Header("메세지 모음")]
    public TMP_Text errorMessage;
    public TMP_Text successMessage;

    [Header("로딩바")]
    public Slider lodingBar;

    private void Start()
    {
        //최초 시작 시 로그인 패널이외의 판넬 및 메세지 비활성
        InitializeLogin();
    }

    /// <summary>
    /// 로그인 패널이외의 판넬 및 메세지 비활성
    /// </summary>
    public void InitializeLogin()
    {
        loginPanel.SetActive(true);
        signUpPanel.SetActive(false);
        createNickNamePanel.SetActive(false);
        errorMessage.gameObject.SetActive(false);
        successMessage.gameObject.SetActive(false);
        lodingBar.gameObject.SetActive(false);
    }

    /// <summary>
    /// 메세지 표시 및 숨기기
    /// </summary>
    /// <param name="messageText"> 표시할 텍스트 오브젝트 </param>
    /// <param name="message"> 표시할 메세지 내용 </param>
    /// <param name="isActive"> 활성화 여부 </param>
    /// <example>
    /// 메세지 사용 예시
    /// <code> 
    /// titleUI.ShowMessage(titleUI.successMessage, "Login Successful!", true); 
    /// </code>>
    /// </example>
    public void ShowMessage(TMP_Text messageText, string message, bool isActive)
    {
        messageText.gameObject.SetActive(isActive);
        if (isActive)
        {
            messageText.text = message;
        }
    }

    public void HideMessages()
    {//모든 메세지 숨기기
        errorMessage.gameObject.SetActive(false);
        successMessage.gameObject.SetActive(false);
    }

    /// <summary>
    /// 특정 인풋 필드의 텍스트 초기화
    /// </summary>
    /// <param name="field1"> 초기화할 첫 번째 인풋 필드</param>
    /// <param name="field2"> 초기화할 두 번째 인풋 필드 (선택)</param>
    public void ResetField(TMP_InputField field1, TMP_InputField field2)
    {
        field1.text = "";
        if (field2 != null) 
        {
            field2.text = "";
        }
    }
    public void ToggleLoginPanel(bool isActive)
    {
        loginPanel.SetActive(isActive);
    }

    /// <summary>
    /// 로그인 패널 활성화 상태 설정
    /// </summary>
    /// <param name="isActive">활성화 여부</param>
    public void ToggleCreateNickNamePanel(bool isActive)
    {        
        createNickNamePanel.SetActive(isActive);
    }
    /// <summary>
    /// 특정 버튼 활성화 여부 설정
    /// 일단 사용은 회원가입 때만 사용함
    /// </summary>
    /// <param name="isInteractable"> 버튼 활성화 여부</param>
    public void SetButtonsInteractable(bool isInteractable)
    {
        signUpCompleteBtn.interactable = isInteractable; 
        signUpLoingToBtn.interactable = isInteractable;
    }
    /// <summary>
    /// 회원가입 패널 활성화 상태 설정
    /// </summary>
    /// <param name="isActive">활성화 여부</param>
    public void ToggleSignUpPanel(bool isActive)
    {
        signUpPanel.SetActive(isActive);
    }
    /// <summary>
    /// 로그인 화면에서 회원가입으로 이동
    /// </summary>
    public void SignUpButtonCon()
    {
        loginEmailField.text = "";
        loginpasswordField.text = "";
        errorMessage.gameObject.SetActive(false);
        loginPanel.SetActive(false);
        signUpPanel.SetActive(true);
    }

    /// <summary>
    /// 회원가입 화면에서 로그인 화면으로 이동
    /// </summary>
    public void LoginToButtonCon()
    {
        signUpEmailField.text = "";
        signUpPasswordField.text = "";
        loginPanel.SetActive(true);
        signUpPanel.SetActive(false);
    }
    /// <summary>
    /// 닉네임 생성 취소 버튼 동작
    /// </summary>
    public void CreateCancelBottonCon()
    {
        errorMessage.text = "";
        createNickNameField.text = "";
        createNickNamePanel.gameObject.SetActive(false);
        loginPanel.gameObject.SetActive(true);
    }
}
