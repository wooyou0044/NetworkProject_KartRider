using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using System.Threading.Tasks;
using Firebase.Database;
using OpenCover.Framework.Model;
using System;
using UnityEngine.Rendering;

public class AuthManager : MonoBehaviour
{
    public TitleUI titleUI;
    public ServerConnect serverCon;

    private bool nickNameCheck = false;

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
                Debug.LogError("파이어베이스 오류" + dependStatus + "사용 불가");
            }
        });
    }
    
    /// <summary>
    /// 로그인 정보를 입력 받아 로그인 코루틴 실행
    /// </summary>
    public void Login()
    {
        string email = titleUI.loginEmailField.text;
        string password = titleUI.loginpasswordField.text;
        StartCoroutine(LoginCoroutine(email, password));        
    }

    IEnumerator LoginCoroutine(string email, string password)
    {
        // Firebase 인증을 통해 이메일과 비밀번호 로그인 요청 처리
        var loginTask = FirebaseDBManager.Instance.Auth.SignInWithEmailAndPasswordAsync(email, password);
        // 로그인 요청이 완료될 때 까지 대기
        yield return new WaitUntil(predicate: () => loginTask.IsCompleted);

        if (loginTask.Exception != null)
        {
            //로그인 실패 에러 처리
            LoginError(loginTask.Exception);
            yield break;
        }
        // 로그인 성공 처리
        LoginSuccess(loginTask.Result.User);
        
    }
    private void LoginError(AggregateException exception)
    {
        // Firebase 로그인 실패 시 발생한 에러 처리, 메세지 출력
        FirebaseException firebasEx = exception.GetBaseException() as FirebaseException;
        AuthError errorCode = (AuthError)firebasEx.ErrorCode;
        string message = "";        
        switch (errorCode)
        {
            case AuthError.MissingEmail:
                message = "MissingEmail";
                break;
            case AuthError.MissingPassword:
                message = "MissingPassword";
                break;
            default:
                message = "Email or Password or UserNotFound";
                break;
        }
        titleUI.ShowMessage(titleUI.errorMessage, message, true);
    }

    void LoginSuccess(FirebaseUser user)
    {
        //로그인 성공 시 유저 정보 저장 및 처리
        FirebaseDBManager.Instance.User = user;
        titleUI.HideMessages();
        titleUI.ToggleLoginPanel(false);  
        StartCoroutine(PostLogin(user));        
    }
    IEnumerator PostLogin(FirebaseUser user)
    {
        //로그인 성공 후 닉네임 확인, 닉네임이 없다면 생성될 때 까지 대기
        //닉네임이 있다면 통과
        titleUI.ResetField(titleUI.loginEmailField, titleUI.loginpasswordField);
        titleUI.ToggleCreateNickNamePanel(true);        
        yield return new WaitUntil(predicate: () => !string.IsNullOrEmpty(user.DisplayName));
        titleUI.ToggleCreateNickNamePanel(false);
        titleUI.ShowMessage(titleUI.successMessage, "Login Successful!", true);
        serverCon.ConnectToServer();
    }
    public void CreateNickNameBottenCon()
    {
        //닉네임 생성 버튼 클릭 시 닉네임 중복 검사 코루틴
        string nickName = titleUI.createNickNameField.text;
        StartCoroutine(CreateNickNameCor(nickName));
    }
    IEnumerator CreateNickNameCor(string nickName)
    {
        if (string.IsNullOrEmpty(nickName))
        {//닉네임 생성창이 비어있다면 메세지: "닉네임을 생성하세요"
            titleUI.ShowMessage(titleUI.errorMessage, "CreateNickName!", true);
            yield break;
        }
        
        /// <summary>
        /// Firebase Realtime Database에서 닉네임 중복 여부를 확인
        /// </summary>
        /// <remarks>
        /// - 최상위 DbRef 경로에서 "users"로 이동합니다. 해당 경로가 없으면 자동으로 생성
        /// - "users" 경로에 있는 데이터를 "UserNickName" 필드값을 기준으로 정렬
        ///   Firebase 데이터는 JSON 구조로 저장되므로, 필드값을 기준으로 정렬
        /// - EqualTo(nickName)을 사용하여 "UserNickName" 필드값이 nickName과 동일한 데이터를 필터링
        /// - GetValueAsync()를 호출하여 데이터를 비동기로 가져옵니다. 작업 결과는 Task로 반환
        /// </remarks>
        /// <param name="nickName">중복 여부를 확인할 닉네임</param>
        /// <returns>Task를 통해 필터링된 데이터를 반환합니다.</returns>
        var nickNameCheckingTask = FirebaseDBManager.Instance.DbRef.Child("users")
            .OrderByChild("UserNickName")
            .EqualTo(nickName).GetValueAsync();

        yield return new WaitUntil(predicate: () => nickNameCheckingTask.IsCompleted);
        if (nickNameCheckingTask.Exception != null)
        {//닉네임 유효성 검사 실패 메세지: 사용할 수 없는 닉네임입니다.
            Debug.Log("중복닉네임검사 오류!");
            titleUI.ShowMessage(titleUI.errorMessage, "This username is not available", true);
            yield break;
        }

        /// <summary>
        /// 주어진 닉네임이 Firebase Realtime Database에 이미 존재하는지 확인
        /// </summary>
        /// <remarks>
        /// - 닉네임 검사작업의 결과를 DataSnapshot으로 가져옴
        /// - Exists 속성을 사용하여 닉네임이 이미 사용중인지 확인
        /// - 닉네임이 존재할 경우, 오류 메시지를 표시하고 이후 처리를 중단
        /// </remarks>
        /// <param name="nickName">중복 여부를 확인할 닉네임</param>
        DataSnapshot nickNameSnapshot = nickNameCheckingTask.Result;
        if (nickNameSnapshot.Exists)
        {//메세지: 이미 사용중인 닉네임입니다.
            titleUI.ShowMessage(titleUI.errorMessage, "Nickname is already in use!", true);
            yield break;
        }

        /// <summary>
        /// 현재 사용자의 닉네임을 Firebase Realtiem Database에 저장
        /// </summary>
        /// <remarks>
        /// - users/UserId/UserNickName 경로로 이동
        /// - 해당 경로의 "UserNickName" 필드에 닉네임 값을 저장
        /// - SetValueAsync() 메서드를 호출하여 데이터를 비동기로 저장하고 저장이 완료 되면 Task로 반환
        /// </remarks>
        /// <param name="nickName"> 저장할 닉네임 </param>            
        var nickNameTask = FirebaseDBManager.Instance.DbRef.Child("users")
            .Child(FirebaseDBManager.Instance.User.UserId)
            .Child("UserNickName")
            .SetValueAsync(nickName);

        yield return new WaitUntil(predicate: () => nickNameTask.IsCompleted);
        if (nickNameTask.Exception != null)
        {
            //파이어베이스 닉네임 저장 실패 메세지: 사용할 수 없는 닉네임입니다
            Debug.Log("파이어베이스 저장실패!");
            titleUI.errorMessage.text = "This username is not available";
            yield break;
        }

        Debug.Log("유저 정보 저장");
        FirebaseDBManager.Instance.User.UpdateUserProfileAsync(new UserProfile { DisplayName = nickName });
        titleUI.HideMessages();
        titleUI.ShowMessage(titleUI.successMessage, "Creation complete!", true);
        titleUI.ToggleCreateNickNamePanel(false);
    }


    //유저프로파일 삭제하는 버튼 테스트 때만 사용하기 서버 커넥트 연결할 때 삭제하기
    public void DeletuserProfile()
    {//프로파일에 저장한 유저 닉네임 초기화하기        
        var nickNameTask = FirebaseDBManager.Instance.DbRef.Child("users").Child(FirebaseDBManager.Instance.User.UserId).Child("UserNickName").SetValueAsync(null);
        UserProfile userProfile = new UserProfile { DisplayName = null };
        var user = FirebaseDBManager.Instance.User.UpdateUserProfileAsync(userProfile);
    }
    public void Signup()
    {
        string email = titleUI.signUpEmailField.text;
        string password = titleUI.signUpPasswordField.text;
        StartCoroutine(SignUpCoroutine(email, password));
    }
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
    private void SignUpError(AggregateException exception)
    {
        FirebaseException firebaseEx = exception.GetBaseException() as FirebaseException;
        AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

        string message = "";
        switch (errorCode)
        {
            case AuthError.MissingEmail:
                message = "MissingEmail";
                break;
            case AuthError.MissingPassword:
                message = "MissingPassword";
                break;
            case AuthError.WeakPassword:
                message = "Kindly use at least 6 characters.";
                break;
            case AuthError.EmailAlreadyInUse:
                message = "This email is already in use.";
                break;
            default:
                message = "Please contact the administrator";
                break;
        }
        titleUI.ShowMessage(titleUI.errorMessage, message, true);
    }
    private void SignUpSuccess(FirebaseUser user)
    {
        FirebaseDBManager.Instance.User = user;//유저 정보 연결하고
        titleUI.HideMessages();
        titleUI.ShowMessage(titleUI.successMessage, "Please Wait", true);
        //버튼 비활성화
        titleUI.SetButtonsInteractable(false);

        StartCoroutine(FinishSignUp());
    }
    IEnumerator FinishSignUp()
    {
        yield return new WaitForSeconds(1); //1초 대기
        titleUI.ToggleSignUpPanel(false);
        //회원가입 완료 메세지
        titleUI.ShowMessage(titleUI.successMessage, "SignUp Successful!", true);
        yield return new WaitForSeconds(1f); //1초 대기
        titleUI.SetButtonsInteractable(true);
        titleUI.HideMessages();
        titleUI.ResetField(titleUI.signUpEmailField, titleUI.signUpPasswordField);
        titleUI.ToggleLoginPanel(true);
    }
}
