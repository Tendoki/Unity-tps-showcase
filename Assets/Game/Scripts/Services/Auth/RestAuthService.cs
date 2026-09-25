using Cysharp.Threading.Tasks;
using Game.Scripts.Services.Auth.Dtos;
using UnityEngine;
using UnityEngine.Networking;

namespace Game.Scripts.Services.Auth
{
    public class RestAuthService : IPlayerAuthService
    {
        private const string GuestAuthPath = "/api/auth/guest";
        private const string AccessTokenKey = "RestAccessToken";
        private readonly RestApiClient _apiClient;

        public bool IsInitialized { get; private set; }
        public bool IsSignedIn { get; private set; }
        public string UserId { get; private set; }
        public string AccessToken { get; private set; }

        public RestAuthService(RestApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public UniTask<bool> InitializeAsync()
        {
            AccessToken = PlayerPrefs.GetString(AccessTokenKey);
            UserId = null;
            IsSignedIn = false;
            IsInitialized = true;
            return UniTask.FromResult(true);
        }

        public async UniTask<bool> SignInGuestAsync()
        {
            if (IsInitialized == false)
            {
                Debug.LogError("REST guest auth failed. RestAuthService is not initialized.");
                return false;
            }

            if (IsSignedIn)
                return true;

            if (await TryRestoreSessionAsync())
                return true;

            return await LoginGuestAsync();
        }

        private async UniTask<bool> TryRestoreSessionAsync()
        {
            if (string.IsNullOrWhiteSpace(AccessToken))
                return false;

            Debug.Log("REST guest session token found. Validating it on the server.");

            try
            {
                PlayerDto session = await _apiClient.SendAsync<PlayerDto>(
                    UnityWebRequest.kHttpVerbGET,
                    "/api/players/me",
                    accessToken: AccessToken);

                if (session == null || session.id <= 0)
                    throw new RestRequestException("REST restored session returned an invalid player.");

                UserId = session.id.ToString();
                IsSignedIn = true;
                return true;
            }
            catch (RestRequestException exception) when (exception.IsUnauthorized)
            {
                ClearRestoredSession();
                Debug.Log("REST guest session token was rejected. Starting a new guest session.");
                return false;
            }
        }

        private void ClearRestoredSession()
        {
            AccessToken = null;
            UserId = null;
            IsSignedIn = false;
            PlayerPrefs.DeleteKey(AccessTokenKey);
            PlayerPrefs.Save();
        }

        private async UniTask<bool> LoginGuestAsync()
        {
            GuestAuthResponseDto response = await _apiClient.SendAsync<GuestAuthResponseDto>(
                UnityWebRequest.kHttpVerbPOST, GuestAuthPath);

            if (response == null || string.IsNullOrWhiteSpace(response.accessToken))
            {
                throw new RestRequestException("REST guest auth returned an invalid session.");
            }

            UserId = response.id.ToString();
            AccessToken = response.accessToken;
            IsSignedIn = true;

            PlayerPrefs.SetString(AccessTokenKey, AccessToken);
            PlayerPrefs.Save();

            Debug.Log($"REST guest auth complete. UserId: {UserId}");
            return true;
        }
    }
}
