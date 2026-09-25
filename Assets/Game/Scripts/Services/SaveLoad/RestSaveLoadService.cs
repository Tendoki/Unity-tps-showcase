using CodeBase.Data;
using Cysharp.Threading.Tasks;
using Game.Scripts.Services.Auth;
using Game.Scripts.Services.Auth.Dtos;
using Game.Scripts.Services.PersistentProgress;

namespace Game.Scripts.Services.SaveLoad
{
    public class RestSaveLoadService : ISaveLoadService
    {
        private readonly RestPlayerApi _playerApi;
        private readonly IPersistentProgressService _progressService;

        public RestSaveLoadService(
            RestPlayerApi playerApi,
            IPersistentProgressService progressService)
        {
            _playerApi = playerApi;
            _progressService = progressService;
        }

        public async UniTask SaveProgress()
        {
            PlayerProgress progress = _progressService.Progress;

            if (progress == null)
                return;

            await _playerApi.SaveProgressAsync(progress);
        }

        public async UniTask<PlayerProgress> LoadProgress()
        {
            PlayerDto player = await _playerApi.GetCurrentPlayerAsync();
            return player?.ToPlayerProgress();
        }

        public async UniTask UpdateNickname(string nickname)
        {
            PlayerDto player = await _playerApi.UpdateNicknameAsync(nickname);

            if (player == null || _progressService.Progress == null)
                return;

            _progressService.Progress.Nickname = player.nickname;
        }
    }
}
