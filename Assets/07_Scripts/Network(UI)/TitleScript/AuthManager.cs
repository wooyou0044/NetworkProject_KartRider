using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using System;

public class AuthManager : MonoBehaviour
{
    public TitleUI titleUI;
    public ServerConnect serverCon;

    //���۰� ���ÿ� ���̾�̽��� ��� ������ �����ͺ��̽� ������
    //���� ���Ӽ��� ���̾�̽� ������ ������
    private void Awake()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            var dependStatus = task.Result;
            if (dependStatus == DependencyStatus.Available)
            {
                FirebaseDBManager.Instance.Auth = FirebaseAuth.DefaultInstance;
                FirebaseDBManager.Instance.DbRef = FirebaseDatabase.DefaultInstance.RootReference;
            }
            else
            {
                Debug.LogError("���̾�̽� ����" + dependStatus + "��� �Ұ�");
            }
        });
    }
    
    //������������ �����ϴ� ��ư �׽�Ʈ ���� ����ϰ� ������ ����
    public void DeletuserProfile()
    {//�������Ͽ� ������ ���� �г��� �ʱ�ȭ�ϱ�
        var nickNameTask = FirebaseDBManager.Instance.DbRef.Child("users")
            .Child(FirebaseDBManager.Instance.User.UserId)
            .Child("isLoggedIn").SetValueAsync(false);
    }

    /// <summary>
    /// �α��� ������ �Է� �޾� �α��� �ڷ�ƾ ����
    /// �α��� ���� �Ǵ� ������ ���� ��ư�� ��Ȱ��ȭ
    /// </summary>
    public void Login()
    {

        string email = titleUI.loginEmailField.text;
        string password = titleUI.loginpasswordField.text;
        titleUI.SetLogInButtonsInteractable(false);
        StartCoroutine(LoginCoroutine(email, password));
    }
    IEnumerator LoginCoroutine(string email, string password)
    {
        // Firebase ������ ���� �̸��ϰ� ��й�ȣ �α��� ��û ó��
        var loginTask = FirebaseDBManager.Instance.Auth.SignInWithEmailAndPasswordAsync(email, password);
        // �α��� ��û�� �Ϸ�� �� ���� ���
        yield return new WaitUntil(predicate: () => loginTask.IsCompleted);
        if (loginTask.Exception != null)
        {
            //�α��� ���� ���� ó��
            LoginError(loginTask.Exception);
            titleUI.SetLogInButtonsInteractable(true);
            yield break;
        }

        FirebaseDBManager.Instance.User = loginTask.Result.User;

        var userLoginTask = FirebaseDBManager.Instance.DbRef.Child("users")
            .Child(FirebaseDBManager.Instance.User.UserId)
            .Child("isLoggedIn").GetValueAsync();
        yield return new WaitUntil(() => userLoginTask.IsCompleted);
        if (userLoginTask.Exception != null)
        {
            titleUI.ShowMessage(titleUI.errorMessage, "�α��� ���� Ȯ�� ���� �ٽ� �α����ϼ���.", true);
            yield return new WaitForSeconds(1f);
            titleUI.SetLogInButtonsInteractable(true);
            titleUI.InitializeLogin();//�ٽ� �α��� �ϴ� �� ó�� Ÿ��Ʋ â �ʱ�ȭ
            yield break;            
        }
        bool? isLoggedIn = userLoginTask.Result.Value as bool?;
        if (isLoggedIn.HasValue && isLoggedIn.Value)
        {
            // �̹� �α��� ���¶�� �α��� ����
            titleUI.SetLogInButtonsInteractable(true);
            titleUI.ShowMessage(titleUI.errorMessage, "�̹� �α��ε� �����Դϴ�. \n�ٸ� ��⿡�� �α׾ƿ� �� �ٽ� �õ��ϼ���.", true);
            yield break; // �α��� �õ��� �ߴ�
        }
        else
        {
            var userRef = FirebaseDBManager.Instance.DbRef.Child("users")
                .Child(FirebaseDBManager.Instance.User.UserId)
                .Child("isLoggedIn").SetValueAsync(true);

            yield return new WaitUntil(() => userRef.IsCompleted);
            if (userRef.Exception != null)
            {
                titleUI.ShowMessage(titleUI.errorMessage, "�α��� ���� Ȯ�� ���� �ٽ� �α����ϼ���.", true);
                yield return new WaitForSeconds(1f);
                titleUI.SetLogInButtonsInteractable(true);
                yield break;
            }
            // �α��� ���� ó��
            LoginSuccess(loginTask.Result.User);
        }
    }
    private void LoginError(AggregateException exception)
    {
        // Firebase �α��� ���� �� �߻��� ���� ó��, �޼��� ���
        FirebaseException firebasEx = exception.GetBaseException() as FirebaseException;
        AuthError errorCode = (AuthError)firebasEx.ErrorCode;
        string message = "";
        switch (errorCode)
        {
            case AuthError.MissingEmail:
                message = "�̸����� �Է��� �ּ���.";
                break;
            case AuthError.MissingPassword:
                message = "�н����带 �Է��� �ּ���.";
                break;
            default:
                message = "�̸��� Ȥ�� ��й�ȣ�� �߸� �Է��ϼ̰ų�\n ��ϵ��� ���� �̸��� �Դϴ�.";
                break;
        }
        titleUI.SetLogInButtonsInteractable(true);
        titleUI.ShowMessage(titleUI.errorMessage, message, true);
    }

    void LoginSuccess(FirebaseUser user)
    {
        //�α��� ���� �� ���� ���� ���� �� ó��        
        titleUI.HideMessages();
        titleUI.ToggleLoginPanel(false);  
        StartCoroutine(PostLogin(user));
    }
    IEnumerator PostLogin(FirebaseUser user)
    {
        //�α��� ���� �� �г��� Ȯ��, �г����� ���ٸ� ������ �� ���� ���        
        titleUI.ResetField(titleUI.loginEmailField, titleUI.loginpasswordField);
        titleUI.ToggleCreateNickNamePanel(true);
        yield return new WaitUntil(predicate: () => !string.IsNullOrEmpty(user.DisplayName));
        titleUI.ToggleCreateNickNamePanel(false);//�г����� �ִٸ� ���
        titleUI.ShowMessage(titleUI.successMessage, "�α��� ����!", true);
        titleUI.SetLogInButtonsInteractable(true);
        serverCon.ConnectToServer(); //���� ���� �õ�

        SceneCont.Instance.Oper = SceneCont.Instance.SceneAsync("LobbyScene");
        SceneCont.Instance.Oper.allowSceneActivation = false;
        titleUI.lodingBar.gameObject.SetActive(true);

        while (SceneCont.Instance.Oper.isDone == false)
        {
            if (SceneCont.Instance.Oper.progress < 0.9f)
            {
                titleUI.lodingBar.value = SceneCont.Instance.Oper.progress;
                yield return new WaitUntil(predicate: () => serverCon.Connect());
                if (!serverCon.Connect())
                {
                    //Ŀ��Ʈ ������
                    titleUI.ShowMessage(titleUI.errorMessage, "���� ���� ���� �ٽ� �α������ּ���.", true);
                    yield return new WaitForSeconds(2f);
                    titleUI.InitializeLogin();//�ٽ� �α��� �ϴ� �� ó�� Ÿ��Ʋ â �ʱ�ȭ
                    yield break;
                }
            }
            else
            {
                //������ ���� ���� �����
                titleUI.lodingBar.value = 1f;
                break;
            }
        }
        SceneCont.Instance.Oper.allowSceneActivation = true;
    }

    public void CreateNickNameBottenCon()
    {
        //�г��� ���� ��ư Ŭ�� �� �г��� �ߺ� �˻� �ڷ�ƾ
        string nickName = titleUI.createNickNameField.text;
        StartCoroutine(CreateNickNameCor(nickName));
    }
    IEnumerator CreateNickNameCor(string nickName)
    {
        if (string.IsNullOrEmpty(nickName))
        {
            titleUI.ShowMessage(titleUI.errorMessage, "�г����� �����ϼ���!", true);
            yield break;
        }
        
        /// <summary>
        /// Firebase Realtime Database���� �г��� �ߺ� ���θ� Ȯ��
        /// </summary>
        /// <remarks>
        /// - �ֻ��� DbRef ��ο��� "users"�� �̵��մϴ�. �ش� ��ΰ� ������ �ڵ����� ����
        /// - "users" ��ο� �ִ� �����͸� "UserNickName" �ʵ尪�� �������� ����
        ///   Firebase �����ʹ� JSON ������ ����ǹǷ�, �ʵ尪�� �������� ����
        /// - EqualTo(nickName)�� ����Ͽ� "UserNickName" �ʵ尪�� nickName�� ������ �����͸� ���͸�
        /// - GetValueAsync()�� ȣ���Ͽ� �����͸� �񵿱�� �����ɴϴ�. �۾� ����� Task�� ��ȯ
        /// </remarks>
        /// <param name="nickName">�ߺ� ���θ� Ȯ���� �г���</param>
        /// <returns>Task�� ���� ���͸��� �����͸� ��ȯ�մϴ�.</returns>
        var nickNameCheckingTask = FirebaseDBManager.Instance.DbRef.Child("users")
            .OrderByChild("userNickName")
            .EqualTo(nickName).GetValueAsync();

        yield return new WaitUntil(predicate: () => nickNameCheckingTask.IsCompleted);
        if (nickNameCheckingTask.Exception != null)
        {//�г��� ��ȿ�� �˻� ���� �޼���: ����� �� ���� �г����Դϴ�.
            Debug.Log("�ߺ��г��Ӱ˻� ����!");
            titleUI.ShowMessage(titleUI.errorMessage, "����� �� ���� �г����Դϴ�.", true);
            yield break;
        }

        /// <summary>
        /// �־��� �г����� Firebase Realtime Database�� �̹� �����ϴ��� Ȯ��
        /// </summary>
        /// <remarks>
        /// - �г��� �˻��۾��� ����� DataSnapshot���� ������
        /// - Exists �Ӽ��� ����Ͽ� �г����� �̹� ��������� Ȯ��
        /// - �г����� ������ ���, ���� �޽����� ǥ���ϰ� ���� ó���� �ߴ�
        /// </remarks>
        /// <param name="nickName">�ߺ� ���θ� Ȯ���� �г���</param>
        DataSnapshot nickNameSnapshot = nickNameCheckingTask.Result;
        if (nickNameSnapshot.Exists)
        {//�޼���: �̹� ������� �г����Դϴ�.
            titleUI.ShowMessage(titleUI.errorMessage, "�̹� ������� �г��� �Դϴ�!", true);
            yield break;
        }

        /// <summary>
        /// ���� ������� �г����� Firebase Realtiem Database�� ����
        /// </summary>
        /// <remarks>
        /// - users/UserId/UserNickName ��η� �̵�
        /// - �ش� ����� "UserNickName" �ʵ忡 �г��� ���� ����
        /// - SetValueAsync() �޼��带 ȣ���Ͽ� �����͸� �񵿱�� �����ϰ� ������ �Ϸ� �Ǹ� Task�� ��ȯ
        /// </remarks>
        /// <param name="nickName"> ������ �г��� </param>            
        var nickNameTask = FirebaseDBManager.Instance.DbRef.Child("users")
            .Child(FirebaseDBManager.Instance.User.UserId)
            .Child("userNickName")
            .SetValueAsync(nickName);

        yield return new WaitUntil(predicate: () => nickNameTask.IsCompleted);
        if (nickNameTask.Exception != null)
        {
            //���̾�̽� �г��� ���� ���� �޼���: ����� �� ���� �г����Դϴ�
            Debug.Log("���̾�̽� �������!");
            titleUI.errorMessage.text = "����� �� ���� �г����Դϴ�.";
            yield break;
        }

        Debug.Log("���� ���� ����");
        FirebaseDBManager.Instance.User.UpdateUserProfileAsync(new UserProfile { DisplayName = nickName });
        titleUI.HideMessages();
        titleUI.ShowMessage(titleUI.successMessage, "�г��� ���� ����!", true);
        titleUI.ToggleCreateNickNamePanel(false);
    }


    /// <summary>
    /// ȸ�� ���� ��ư ���� ��
    /// ����ڰ� �Է��� �̸��� �� ��й�ȣ�� ������ �񵿱������� ȸ�� ���� ������ ����
    /// ������� �̸��� �� ��й�ȣ�� UI �ʵ忡�� ������
    /// �̸��� �� ��й�ȣ�� ���ڷ� �Ͽ� SignUpCoroutine �ڷ�ƾ�� ����
    /// </summary>

    public void Signup()
    {
        string email = titleUI.signUpEmailField.text;
        string password = titleUI.signUpPasswordField.text;
        StartCoroutine(SignUpCoroutine(email, password));
    }
    /// <summary>
    /// ȸ������ �ڷ�ƾ
    /// ���� �̸��ϰ� �н����带 ���̾�̽� ��� ���������� ���� ��.
    /// </summary>
    /// <param name="email">�̸��� �ʵ�� ���� �̸��� ����</param>
    /// <param name="password">�н����� �ʵ�� ���� �н����� ����</param>
    IEnumerator SignUpCoroutine(string email, string password)
    {        
        var signUpTask = FirebaseDBManager.Instance.Auth.CreateUserWithEmailAndPasswordAsync(email, password);
        yield return new WaitUntil(predicate: () => signUpTask.IsCompleted);
        
        if (signUpTask.Exception != null)
        {
            SignUpError(signUpTask.Exception);
            yield break;
        }

        SignUpSuccess(signUpTask.Result.User);
    }
    /// <summary>
    /// ȸ������ ��ȿ���˻� �޼���
    /// ȸ�����Խõ��� �� ���� ���� ������ ó����
    /// </summary>
    private void SignUpError(AggregateException exception)
    {
        FirebaseException firebaseEx = exception.GetBaseException() as FirebaseException;
        AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

        string message = "";
        switch (errorCode)
        {
            case AuthError.MissingEmail:
                message = "�̸����� �Է��� �ּ���.";
                break;
            case AuthError.MissingPassword:
                message = "�н����带 �Է��� �ּ���.";
                break;
            case AuthError.WeakPassword:
                message = "�н������ �ּ� 6�� �̻� �Է��� �ּ���.";
                break;
            case AuthError.EmailAlreadyInUse:
                message = "�̹� �����ϴ� �̸��� �Դϴ�.";
                break;
            default:
                message = "���� ���� ����! �����ڿ��� �����ϼ���.";
                break;
        }
        titleUI.ShowMessage(titleUI.errorMessage, message, true);
    }
    /// <summary>
    /// ȸ������ ���� �޼���
    /// ȸ�����Կ� �����ϸ� ������ ������ ó���ϴ� �ڷ�ƾ ����
    /// </summary>
    /// <param name="user">���������� �޾Ƽ� ���̾�̽��� �����ϴ� ������ �� </param>
    private void SignUpSuccess(FirebaseUser user)
    {
        FirebaseDBManager.Instance.User = user;//���� ���� �����ϰ�               
        titleUI.HideMessages();
        titleUI.ShowMessage(titleUI.successMessage, "���� ������.", true);
        //��ư ��Ȱ��ȭ
        titleUI.SetsignUpInteractable(false);
        StartCoroutine(FinishSignUp());
    }
    /// <summary>
    /// ȸ������ ������ ������ �Ǿ��ٸ� ���������� ���� ���̾�̽��� ������ �α��� ������ ������
    /// ���� ���� 5�� �̻��� �ð��� ������ �Ǹ� ������ ����
    /// ���� �߻� �� ������� ���� ������ �����ϰ� �ٽ� ������ ��
    /// ������ ���ٸ� ȸ������ �Ϸ�
    /// </summary>
    IEnumerator FinishSignUp()
    {        
        var setPrfileTask = FirebaseDBManager.Instance.DbRef.Child("users")
            .Child(FirebaseDBManager.Instance.User.UserId).Child("isLoggedIn")
            .SetValueAsync(false);
        
        float timer = 5f;
        float elapsedTime = 0;
        bool toggle = true;
        WaitForSeconds wait = new WaitForSeconds(1f);
        while (!setPrfileTask.IsCompleted)
        {            
            elapsedTime += 1f;
            if (elapsedTime >= timer)
            {
                titleUI.ShowMessage(titleUI.errorMessage, "���� ������ ���� ���� �����ڿ��� �����ϼ���.", true);
                yield break;
            }
            string message = toggle ? "���� ������." : "���� ������..";
            titleUI.ShowMessage(titleUI.successMessage, message, true);
            toggle = !toggle;
            yield return wait;            
        }
        yield return new WaitUntil(() => setPrfileTask.IsCompleted);
        if(setPrfileTask.Exception != null)
        {
            titleUI.ShowMessage(titleUI.errorMessage, "���� ������ ���� ���� �����ڿ��� �����ϼ���.", true);
            yield return new WaitForSeconds(2);
            yield break;
        }
        titleUI.ToggleSignUpPanel(false);
        //ȸ������ �Ϸ� �޼���
        titleUI.ShowMessage(titleUI.successMessage, "ȸ������ �Ϸ�!", true);
        yield return new WaitForSeconds(1f); //1�� ���
        titleUI.SetsignUpInteractable(true); //��ư ��� Ǯ��
        titleUI.HideMessages(); //�޼��� ������
                                //�ؽ�Ʈ �ʱ�ȭ �ϰ�        
        titleUI.LoginToButtonCon(); //ȸ������ �Ϸ� �� �α��� ȭ������ �̵�
    }
}
