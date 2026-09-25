using Cysharp.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using Game.Scripts.Services.Analytics;
using Game.Scripts.Services.Auth;
using UnityEngine;

namespace Game.Scripts.Services.Firebase
{
    public class FirebaseAuthService : IPlayerAuthService
    {
        private readonly IAnalyticsService _analyticsService;
        private FirebaseAuth _auth;

        public bool IsInitialized { get; private set; }
        public bool IsSignedIn { get; private set; }
        public string UserId { get; private set; }

        public FirebaseAuthService(IAnalyticsService analyticsService)
        {
            _analyticsService = analyticsService;
        }

        public UniTask<bool> InitializeAsync()
        {
            try
            {
                _auth = FirebaseAuth.DefaultInstance;
                IsInitialized = true;
                return UniTask.FromResult(true);
            }
            catch (FirebaseException exception)
            {
                Debug.LogError($"Firebase initialization failed. ErrorCode: {exception.ErrorCode}. Message: {exception.Message}");
                return UniTask.FromResult(false);
            }
            catch (System.Exception exception)
            {
                Debug.LogError($"Firebase initialization failed. Message: {exception.Message}");
                return UniTask.FromResult(false);
            }
        }

        public async UniTask<bool> SignInGuestAsync()
        {
            if (IsInitialized == false || _auth == null)
            {
                Debug.LogError("Firebase sign-in failed. FirebaseAuth is not initialized.");
                return false;
            }

            try
            {
                FirebaseUser user = _auth.CurrentUser;

                if (user == null)
                {
                    AuthResult authResult = await _auth.SignInAnonymouslyAsync().AsUniTask();
                    user = authResult.User;
                    Debug.Log($"Firebase anonymous sign-in complete. UserId: {user.UserId}");
                    _analyticsService.LogAnonymousSignIn(user.UserId);
                }
                else
                {
                    Debug.Log($"Firebase user restored. UserId: {user.UserId}");
                    _analyticsService.LogFirebaseUserRestored(user.UserId);
                }

                UserId = user.UserId;
                IsSignedIn = true;

                return true;
            }
            catch (FirebaseException exception)
            {
                Debug.LogError($"Firebase sign-in failed. ErrorCode: {exception.ErrorCode}. Message: {exception.Message}");
                return false;
            }
            catch (System.Exception exception)
            {
                Debug.LogError($"Firebase sign-in failed. Message: {exception.Message}");
                return false;
            }
        }
    }
}
