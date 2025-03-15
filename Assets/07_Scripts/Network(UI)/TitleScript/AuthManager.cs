using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using System.Threading.Tasks;
using Firebase.Database;
using OpenCover.Framework.Model;

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
    
    public void Login()
    {
        StartCoroutine(LoginCoroutine(titleUI.loginEmailField.text, titleUI.loginpasswordField.text));        
    }

    IEnumerator LoginCoroutine(string email, string password)
    {
        var loginTask = FirebaseDBManager.Instance.Auth.SignInWithEmailAndPasswordAsync(email, password);

        yield return new WaitUntil(predicate: () => loginTask.IsCompleted);

        if (loginTask.Exception != null)
        {
            FirebaseException firebasEx = loginTask.Exception.GetBaseException() as FirebaseException;
            AuthError errorCode = (AuthError)firebasEx.ErrorCode;
            string message = "";
            titleUI.errorMessage.gameObject.SetActive(true);
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
            titleUI.errorMessage.text = message;
            yield break;
        }
        else
        {
            FirebaseDBManager.Instance.User = loginTask.Result.User;
            titleUI.errorMessage.gameObject.SetActive(false);
            titleUI.successMessage.gameObject.SetActive(true);
            titleUI.successMessage.text = "Login Successful!";
            titleUI.loginPanel.gameObject.SetActive(false);
            yield return new WaitForSeconds(0.5f); //잠깐 대기
            titleUI.successMessage.gameObject.SetActive(false);
            titleUI.loginEmailField.text = "";
            titleUI.loginpasswordField.text = "";

            //닉네임이 없다면 닉네임 부터 만들고
            titleUI.createNickNamePanel.gameObject.SetActive(true);
            yield return new WaitUntil(predicate: () => !string.IsNullOrEmpty(FirebaseDBManager.Instance.User.DisplayName));
            titleUI.createNickNamePanel.gameObject.SetActive(false);

            //닉네임이 있거나 만들었다면 접속 시도요청
            serverCon.ConnectToServer();
        }
    }
    public void CreateNickNameBottenCon()
    {
        StartCoroutine(CreateNickNameCor(titleUI.createNickNameField.text));
    }
    IEnumerator CreateNickNameCor(string nickName)
    {
        titleUI.errorMessage.gameObject.SetActive(true);
        if (nickName == "")
        {
            titleUI.errorMessage.text = "CreateNickName!";
        }
        else
        {
            /// <summary>
            /// Firebase Realtime Database에서 닉네임 중복 여부를 확인합니다.
            /// </summary>
            /// <remarks>
            /// - 최상위 DbRef 경로에서 "users"로 이동합니다. 해당 경로가 없으면 자동으로 생성됩니다.
            /// - "users" 경로에 있는 데이터를 "UserNickName" 필드값을 기준으로 정렬합니다.
            ///   Firebase 데이터는 JSON 구조로 저장되므로, 필드값을 기준으로 정렬이 가능합니다.
            /// - EqualTo(nickName)을 사용하여 "UserNickName" 필드값이 nickName과 동일한 데이터를 필터링합니다.
            /// - GetValueAsync()를 호출하여 데이터를 비동기로 가져옵니다. 작업 결과는 Task로 반환됩니다.
            /// </remarks>
            /// <param name="nickName">중복 여부를 확인할 닉네임</param>
            /// <returns>Task를 통해 필터링된 데이터를 반환합니다.</returns>
            var nickNameCheckingTast = FirebaseDBManager.Instance.DbRef.Child("users")
                .OrderByChild("UserNickName")
                .EqualTo(nickName).GetValueAsync();

            yield return new WaitUntil(predicate: () => nickNameCheckingTast.IsCompleted);
            if (nickNameCheckingTast.Exception != null)
            {//닉네임 유효성 검사 실패 메세지: 사용할 수 없는 닉네임입니다.
                Debug.Log("중복닉네임검사 오류!");                
                titleUI.errorMessage.text = "This username is not available";
                yield break;
            }

            /// <summary>
            /// 주어진 닉네임이 Firebase Realtime Database에 이미 존재하는지 확인합니다.
            /// </summary>
            /// <remarks>
            /// - 닉네임 검사작업의 결과를 DataSnapshot으로 가져옵니다.
            /// - Exists 속성을 사용하여 닉네임이 이미 사용중인지 확인합니다.
            /// - 닉네임이 존재할 경우, 오류 메시지를 표시하고 이후 처리를 중단합니다.
            /// </remarks>
            /// <param name="nickName">중복 여부를 확인할 닉네임</param>
            DataSnapshot nickNameSnapshot = nickNameCheckingTast.Result;
            if (nickNameSnapshot.Exists)
            {//메세지: 이미 사용중인 닉네임입니다.
                titleUI.errorMessage.text = "Nickname is already in use!";
                yield break;
            }

            /// <summary>
            /// 현재 사용자의 닉네임을 Firebase Realtiem Database에 저장합니다.
            /// </summary>
            /// <remarks>
            /// - users/UserId/UserNickName 경로로 이동합니다.
            /// - 해당 경로의 "UserNickName" 필드에 닉네임 값을 저장합니다.
            /// - SetValueAsync() 메서드를 호출하여 데이터를 비동기로 저장하고 저장이 완료 되면 Task로 반환합니다.
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
            {//닉네임 생성 완료!
                UserProfile userProfile = new UserProfile { DisplayName = nickName };
                var profileTask = FirebaseDBManager.Instance.User.UpdateUserProfileAsync(userProfile);
                titleUI.errorMessage.gameObject.SetActive(false);
                titleUI.successMessage.gameObject.SetActive(true);
                titleUI.successMessage.text = "Creation complete!";
                yield break;
            }
        }
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
        StartCoroutine(SignUpCoroutine(titleUI.signUpEmailField.text, titleUI.signUpPasswordField.text));
    }
    IEnumerator SignUpCoroutine(string email, string password)
    {        
        var signUpTask = FirebaseDBManager.Instance.Auth.CreateUserWithEmailAndPasswordAsync(email, password);
        yield return new WaitUntil(predicate: () => signUpTask.IsCompleted);
        titleUI.loginPanel.gameObject.SetActive(false);
        titleUI.signUpPanel.gameObject.SetActive(true);
        if (signUpTask.Exception != null)
        {
            FirebaseException firebaseEx = signUpTask.Exception.GetBaseException() as FirebaseException;

            AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

            string message = "";
            titleUI.errorMessage.gameObject.SetActive(true);
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
            titleUI.errorMessage.text = message;
            yield break;
        }
        else
        {
            FirebaseDBManager.Instance.User = signUpTask.Result.User;//유저 정보 연결하고
            titleUI.signUpCompleteBtn.interactable = false; //잠시 버튼 비활성화
            titleUI.signUpLoingToBtn.interactable = false; //잠시 버튼 비활성화
            titleUI.successMessage.gameObject.SetActive(true);//메세지 키고
            titleUI.successMessage.text = "Please Wait"; //잠시 기다리세요...(유저 정보 연결 웨잇언틸 돌리는게 맞음)
            yield return new WaitForSeconds(1); //1초 대기
            titleUI.signUpPanel.gameObject.SetActive(false); //회원가입창 끄고
            titleUI.successMessage.text = "SignUp Successful!";//계정 생성 완료
            yield return new WaitForSeconds(1f); //잠깐 대기
            titleUI.signUpCompleteBtn.interactable = true; //버튼 활성화
            titleUI.signUpLoingToBtn.interactable = true; //버튼 활성화
            titleUI.successMessage.gameObject.SetActive(false); //메세지 끄고
            titleUI.signUpEmailField.text = "";
            titleUI.signUpPasswordField.text = "";
            titleUI.loginPanel.gameObject.SetActive(true); // 로그인 판넬 키기
            yield break;
        }
    }
}
