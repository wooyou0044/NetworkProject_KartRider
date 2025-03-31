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

    //시작과 동시에 파이어베이스의 어스의 정보와 데이터베이스 정보를
    //만든 게임서버 파이어베이스 정보와 연결함
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
    
    //유저프로파일 삭제하는 버튼 테스트 때만 사용하고 삭제할 예정
    public void DeletuserProfile()
    {//프로파일에 저장한 유저 닉네임 초기화하기
        var nickNameTask = FirebaseDBManager.Instance.DbRef.Child("users")
            .Child(FirebaseDBManager.Instance.User.UserId)
            .Child("isLoggedIn").SetValueAsync(false);
    }

    /// <summary>
    /// 로그인 정보를 입력 받아 로그인 코루틴 실행
    /// 로그인 성공 또는 실패전 까지 버튼을 비활성화
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
        // Firebase 인증을 통해 이메일과 비밀번호 로그인 요청 처리
        var loginTask = FirebaseDBManager.Instance.Auth.SignInWithEmailAndPasswordAsync(email, password);
        // 로그인 요청이 완료될 때 까지 대기
        yield return new WaitUntil(predicate: () => loginTask.IsCompleted);
        if (loginTask.Exception != null)
        {
            //로그인 실패 에러 처리
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
            titleUI.ShowMessage(titleUI.errorMessage, "로그인 상태 확인 실패 다시 로그인하세요.", true);
            yield return new WaitForSeconds(1f);
            titleUI.SetLogInButtonsInteractable(true);
            titleUI.InitializeLogin();//다시 로그인 하는 것 처럼 타이틀 창 초기화
            yield break;            
        }
        bool? isLoggedIn = userLoginTask.Result.Value as bool?;
        if (isLoggedIn.HasValue && isLoggedIn.Value)
        {
            // 이미 로그인 상태라면 로그인 차단
            titleUI.SetLogInButtonsInteractable(true);
            titleUI.ShowMessage(titleUI.errorMessage, "이미 로그인된 계정입니다. \n다른 기기에서 로그아웃 후 다시 시도하세요.", true);
            yield break; // 로그인 시도를 중단
        }
        else
        {
            var userRef = FirebaseDBManager.Instance.DbRef.Child("users")
                .Child(FirebaseDBManager.Instance.User.UserId)
                .Child("isLoggedIn").SetValueAsync(true);

            yield return new WaitUntil(() => userRef.IsCompleted);
            if (userRef.Exception != null)
            {
                titleUI.ShowMessage(titleUI.errorMessage, "로그인 상태 확인 실패 다시 로그인하세요.", true);
                yield return new WaitForSeconds(1f);
                titleUI.SetLogInButtonsInteractable(true);
                yield break;
            }
            // 로그인 성공 처리
            LoginSuccess(loginTask.Result.User);
        }
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
                message = "이메일을 입력해 주세요.";
                break;
            case AuthError.MissingPassword:
                message = "패스워드를 입력해 주세요.";
                break;
            default:
                message = "이메일 혹은 비밀번호를 잘못 입력하셨거나\n 등록되지 않은 이메일 입니다.";
                break;
        }
        titleUI.SetLogInButtonsInteractable(true);
        titleUI.ShowMessage(titleUI.errorMessage, message, true);
    }

    void LoginSuccess(FirebaseUser user)
    {
        //로그인 성공 시 유저 정보 저장 및 처리        
        titleUI.HideMessages();
        titleUI.ToggleLoginPanel(false);  
        StartCoroutine(PostLogin(user));
    }
    IEnumerator PostLogin(FirebaseUser user)
    {
        //로그인 성공 후 닉네임 확인, 닉네임이 없다면 생성될 때 까지 대기        
        titleUI.ResetField(titleUI.loginEmailField, titleUI.loginpasswordField);
        titleUI.ToggleCreateNickNamePanel(true);
        yield return new WaitUntil(predicate: () => !string.IsNullOrEmpty(user.DisplayName));
        titleUI.ToggleCreateNickNamePanel(false);//닉네임이 있다면 통과
        titleUI.ShowMessage(titleUI.successMessage, "로그인 성공!", true);
        titleUI.SetLogInButtonsInteractable(true);
        serverCon.ConnectToServer(); //서버 연결 시도

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
                    //커넥트 오류시
                    titleUI.ShowMessage(titleUI.errorMessage, "서버 접속 실패 다시 로그인해주세요.", true);
                    yield return new WaitForSeconds(2f);
                    titleUI.InitializeLogin();//다시 로그인 하는 것 처럼 타이틀 창 초기화
                    yield break;
                }
            }
            else
            {
                //마스터 서버 접속 대기중
                titleUI.lodingBar.value = 1f;
                break;
            }
        }
        SceneCont.Instance.Oper.allowSceneActivation = true;
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
        {
            titleUI.ShowMessage(titleUI.errorMessage, "닉네임을 생성하세요!", true);
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
            .OrderByChild("userNickName")
            .EqualTo(nickName).GetValueAsync();

        yield return new WaitUntil(predicate: () => nickNameCheckingTask.IsCompleted);
        if (nickNameCheckingTask.Exception != null)
        {//닉네임 유효성 검사 실패 메세지: 사용할 수 없는 닉네임입니다.
            Debug.Log("중복닉네임검사 오류!");
            titleUI.ShowMessage(titleUI.errorMessage, "사용할 수 없는 닉네임입니다.", true);
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
            titleUI.ShowMessage(titleUI.errorMessage, "이미 사용중인 닉네임 입니다!", true);
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
            .Child("userNickName")
            .SetValueAsync(nickName);

        yield return new WaitUntil(predicate: () => nickNameTask.IsCompleted);
        if (nickNameTask.Exception != null)
        {
            //파이어베이스 닉네임 저장 실패 메세지: 사용할 수 없는 닉네임입니다
            Debug.Log("파이어베이스 저장실패!");
            titleUI.errorMessage.text = "사용할 수 없는 닉네임입니다.";
            yield break;
        }

        Debug.Log("유저 정보 저장");
        FirebaseDBManager.Instance.User.UpdateUserProfileAsync(new UserProfile { DisplayName = nickName });
        titleUI.HideMessages();
        titleUI.ShowMessage(titleUI.successMessage, "닉네임 생성 성공!", true);
        titleUI.ToggleCreateNickNamePanel(false);
    }


    /// <summary>
    /// 회원 가입 버튼 연결 됨
    /// 사용자가 입력한 이메일 및 비밀번호를 가져와 비동기적으로 회원 가입 절차를 실행
    /// 사용자의 이메일 및 비밀번호를 UI 필드에서 가져옴
    /// 이메일 및 비밀번호를 인자로 하여 SignUpCoroutine 코루틴을 실행
    /// </summary>

    public void Signup()
    {
        string email = titleUI.signUpEmailField.text;
        string password = titleUI.signUpPasswordField.text;
        StartCoroutine(SignUpCoroutine(email, password));
    }
    /// <summary>
    /// 회원가입 코루틴
    /// 들어온 이메일과 패스워드를 파이어베이스 어스에 유저정보를 저장 함.
    /// </summary>
    /// <param name="email">이메일 필드로 받은 이메일 정보</param>
    /// <param name="password">패스워드 필드로 받은 패스워드 정보</param>
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
    /// 회원가입 유효성검사 메서드
    /// 회원가입시도할 때 에러 관련 내용을 처리함
    /// </summary>
    private void SignUpError(AggregateException exception)
    {
        FirebaseException firebaseEx = exception.GetBaseException() as FirebaseException;
        AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

        string message = "";
        switch (errorCode)
        {
            case AuthError.MissingEmail:
                message = "이메일을 입력해 주세요.";
                break;
            case AuthError.MissingPassword:
                message = "패스워드를 입력해 주세요.";
                break;
            case AuthError.WeakPassword:
                message = "패스워드는 최소 6자 이상 입력해 주세요.";
                break;
            case AuthError.EmailAlreadyInUse:
                message = "이미 존재하는 이메일 입니다.";
                break;
            default:
                message = "계정 생성 실패! 관리자에게 문의하세요.";
                break;
        }
        titleUI.ShowMessage(titleUI.errorMessage, message, true);
    }
    /// <summary>
    /// 회원가입 성공 메서드
    /// 회원가입에 성공하면 마지막 로직을 처리하는 코루틴 실행
    /// </summary>
    /// <param name="user">유저정보를 받아서 파이어베이스에 연결하는 역할을 함 </param>
    private void SignUpSuccess(FirebaseUser user)
    {
        FirebaseDBManager.Instance.User = user;//유저 정보 연결하고               
        titleUI.HideMessages();
        titleUI.ShowMessage(titleUI.successMessage, "계정 생성중.", true);
        //버튼 비활성화
        titleUI.SetsignUpInteractable(false);
        StartCoroutine(FinishSignUp());
    }
    /// <summary>
    /// 회원가입 성공이 보장이 되었다면 유저정보를 토대로 파이어베이스에 유저의 로그인 공간을 생성함
    /// 연결 도중 5초 이상의 시간이 지나게 되면 오류로 간주
    /// 오류 발생 시 만들어진 유저 정보를 삭제하고 다시 만들어야 됨
    /// 오류가 없다면 회원가입 완료
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
                titleUI.ShowMessage(titleUI.errorMessage, "유저 데이터 생성 실패 관리자에게 문의하세요.", true);
                yield break;
            }
            string message = toggle ? "계정 생성중." : "계정 생성중..";
            titleUI.ShowMessage(titleUI.successMessage, message, true);
            toggle = !toggle;
            yield return wait;            
        }
        yield return new WaitUntil(() => setPrfileTask.IsCompleted);
        if(setPrfileTask.Exception != null)
        {
            titleUI.ShowMessage(titleUI.errorMessage, "유저 데이터 생성 실패 관리자에게 문의하세요.", true);
            yield return new WaitForSeconds(2);
            yield break;
        }
        titleUI.ToggleSignUpPanel(false);
        //회원가입 완료 메세지
        titleUI.ShowMessage(titleUI.successMessage, "회원가입 완료!", true);
        yield return new WaitForSeconds(1f); //1초 대기
        titleUI.SetsignUpInteractable(true); //버튼 잠금 풀고
        titleUI.HideMessages(); //메세지 가리고
                                //텍스트 초기화 하고        
        titleUI.LoginToButtonCon(); //회원가입 완료 후 로그인 화면으로 이동
    }
}
