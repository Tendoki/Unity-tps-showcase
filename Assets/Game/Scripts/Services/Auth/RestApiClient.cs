using CodeBase.Data;
using Cysharp.Threading.Tasks;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Game.Scripts.Services.Auth
{
    public class RestApiClient
    {
        private const int TimeoutSeconds = 10;
        private readonly string _baseUrl;

        public RestApiClient(string baseUrl)
        {
            _baseUrl = baseUrl.TrimEnd('/');
        }

        public async UniTask<TResponse> SendAsync<TResponse>(
            string method,
            string path,
            object body = null,
            string accessToken = null) where TResponse : class
        {
            using var request = new UnityWebRequest($"{_baseUrl}{path}", method)
            {
                downloadHandler = new DownloadHandlerBuffer(),
                timeout = TimeoutSeconds,
            };

            if (!string.IsNullOrWhiteSpace(accessToken))
                request.SetRequestHeader("Authorization", $"Bearer {accessToken}");

            if (body != null)
            {
                request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body.ToJson()));
                request.SetRequestHeader("Content-Type", "application/json");
            }

            try
            {
                await request.SendWebRequest().ToUniTask();
            }
            catch (UnityWebRequestException exception)
            {
                string message = request.responseCode == 401
                    ? "REST session was rejected by the server."
                    : request.responseCode == 0
                        ? "REST server is unavailable or the request timed out."
                        : $"REST server returned HTTP {request.responseCode}.";

                throw new RestRequestException(message, request.responseCode, exception);
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                throw new RestRequestException(
                    $"REST request failed: {request.error}", request.responseCode);
            }

            Debug.Log($"REST {method} {path} complete. Status: {request.responseCode}");
            TResponse response = request.downloadHandler.text.ToDeserialized<TResponse>();

            if (response == null)
                throw new RestRequestException("REST server returned an empty response.", request.responseCode);

            return response;
        }
    }
}
