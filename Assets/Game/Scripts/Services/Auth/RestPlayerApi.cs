using CodeBase.Data;
using Cysharp.Threading.Tasks;
using Game.Scripts.Services.Auth.Dtos;
using UnityEngine;
using UnityEngine.Networking;

namespace Game.Scripts.Services.Auth
{
    public class RestPlayerApi
    {
        private const string CurrentPlayerPath = "/api/players/me";
        private const string SavePlayerProgressPath = "/api/players/me/save";

        private readonly RestAuthService _authService;
        private readonly RestApiClient _apiClient;

        public RestPlayerApi(RestAuthService authService, RestApiClient apiClient)
        {
            _authService = authService;
            _apiClient = apiClient;
        }

        public async UniTask<PlayerDto> GetCurrentPlayerAsync()
        {
            if (string.IsNullOrWhiteSpace(_authService.AccessToken))
            {
                throw new RestRequestException("REST get player requires an access token.", 401);
            }

            PlayerDto player = await _apiClient.SendAsync<PlayerDto>(
                UnityWebRequest.kHttpVerbGET, CurrentPlayerPath, accessToken: _authService.AccessToken);

            if (player == null)
            {
                throw new RestRequestException("REST get player returned an invalid profile.");
            }

            Debug.Log($"REST player loaded. UserId: {player.id}");
            return player;
        }

        public async UniTask<PlayerDto> SaveProgressAsync(PlayerProgress progress)
        {
            if (progress == null)
            {
                throw new RestRequestException("REST save progress requires player progress.");
            }

            if (string.IsNullOrWhiteSpace(_authService.AccessToken))
            {
                throw new RestRequestException("REST save progress requires an access token.", 401);
            }

            PlayerDto player = await _apiClient.SendAsync<PlayerDto>(
                UnityWebRequest.kHttpVerbPOST, SavePlayerProgressPath,
                new SavePlayerProgressRequestDto(progress), _authService.AccessToken);

            if (player == null)
            {
                throw new RestRequestException("REST save progress returned an invalid profile.");
            }

            Debug.Log($"REST progress saved. UserId: {player.id}");
            return player;
        }

        public async UniTask<PlayerDto> UpdateNicknameAsync(string nickname)
        {
            if (string.IsNullOrWhiteSpace(_authService.AccessToken))
            {
                throw new RestRequestException("REST update nickname requires an access token.", 401);
            }

            PlayerDto player = await _apiClient.SendAsync<PlayerDto>(
                "PATCH", CurrentPlayerPath, new UpdateNicknameRequestDto(nickname), _authService.AccessToken);

            if (player == null)
            {
                throw new RestRequestException("REST update nickname returned an invalid profile.");
            }

            Debug.Log($"REST nickname updated. UserId: {player.id}");
            return player;
        }
    }
}
