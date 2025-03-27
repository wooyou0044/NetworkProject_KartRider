using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TitleUI : MonoBehaviour
{
    [Header("�α��� �ĳ� ����")]
    public GameObject loginPanel;
    public TMP_InputField loginEmailField;
    public TMP_InputField loginpasswordField;
    public Button loingBtn;
    public Button signUpBtn;

    [Header("ȸ������ �ǳ� ����")]
    public GameObject signUpPanel;
    public TMP_InputField signUpEmailField;
    public TMP_InputField signUpPasswordField;
    public Button signUpLoingToBtn;
    public Button signUpCompleteBtn;

    [Header("�г��� ���� �ű� �����ڸ�")]
    public GameObject createNickNamePanel;
    public TMP_InputField createNickNameField;

    [Header("�޼��� ����")]
    public TMP_Text errorMessage;
    public TMP_Text successMessage;

    [Header("�ε���")]
    public Slider lodingBar;

    private void Start()
    {
        //���� ���� �� �α��� �г��̿��� �ǳ� �� �޼��� ��Ȱ��
        InitializeLogin();
    }

    /// <summary>
    /// �α��� �г��̿��� �ǳ� �� �޼��� ��Ȱ��
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
    /// �޼��� ǥ�� �� �����
    /// </summary>
    /// <param name="messageText"> ǥ���� �ؽ�Ʈ ������Ʈ </param>
    /// <param name="message"> ǥ���� �޼��� ���� </param>
    /// <param name="isActive"> Ȱ��ȭ ���� </param>
    /// <example>
    /// �޼��� ��� ����
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

    //��� �޼��� �����
    public void HideMessages()
    {
        errorMessage.gameObject.SetActive(false);
        successMessage.gameObject.SetActive(false);
    }

    /// <summary>
    /// Ư�� ��ǲ �ʵ��� �ؽ�Ʈ �ʱ�ȭ
    /// </summary>
    /// <param name="field1"> �ʱ�ȭ�� ù ��° ��ǲ �ʵ�</param>
    /// <param name="field2"> �ʱ�ȭ�� �� ��° ��ǲ �ʵ� (����)</param>
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
    /// �α��� �г� Ȱ��ȭ ���� ����
    /// </summary>
    /// <param name="isActive">Ȱ��ȭ ����</param>
    public void ToggleCreateNickNamePanel(bool isActive)
    {        
        createNickNamePanel.SetActive(isActive);
    }
    /// <summary>
    ///  ȸ�����Կ��� ��ư Ȱ��ȭ ���� ����
    /// </summary>
    /// <param name="isInteractable"> ��ư Ȱ��ȭ ����</param>
    public void SetLogInButtonsInteractable(bool isInteractable)
    {
        loingBtn.interactable = isInteractable;
        signUpBtn.interactable = isInteractable;
    }

    /// <summary>
    ///  ȸ�����Կ��� ��ư Ȱ��ȭ ���� ����
    /// </summary>
    /// <param name="isInteractable"> ��ư Ȱ��ȭ ����</param>
    public void SetsignUpInteractable(bool isInteractable)
    {
        signUpCompleteBtn.interactable = isInteractable; 
        signUpLoingToBtn.interactable = isInteractable;
    }
    /// <summary>
    /// ȸ������ �г� Ȱ��ȭ ���� ����
    /// </summary>
    /// <param name="isActive">Ȱ��ȭ ����</param>
    public void ToggleSignUpPanel(bool isActive)
    {
        signUpPanel.SetActive(isActive);
    }
    /// <summary>
    /// �α��� ȭ�鿡�� ȸ���������� �̵�
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
    /// ȸ������ ȭ�鿡�� �α��� ȭ������ �̵�
    /// </summary>
    public void LoginToButtonCon()
    {
        signUpEmailField.text = "";
        signUpPasswordField.text = "";
        loginPanel.SetActive(true);
        signUpPanel.SetActive(false);
    }
    /// <summary>
    /// �г��� ���� ��� ��ư ����
    /// </summary>
    public void CreateCancelBottonCon()
    {
        errorMessage.text = "";
        createNickNameField.text = "";
        createNickNamePanel.gameObject.SetActive(false);
        loginPanel.gameObject.SetActive(true);
    }
}
