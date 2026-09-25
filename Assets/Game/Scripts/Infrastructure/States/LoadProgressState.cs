using CodeBase.Data;
using Cysharp.Threading.Tasks;
using Game.Infrastructure.StateMachines;
using Game.Scripts.Services.PersistentProgress;
using Game.Scripts.Services.SaveLoad;

namespace Game.Scripts.Infrastructure.States
{
  public class LoadProgressState : AsyncBaseState
  {
    private readonly IPersistentProgressService _progressService;
    private readonly ISaveLoadService _saveLoadProgress;

    public LoadProgressState(IPersistentProgressService progressService, ISaveLoadService saveLoadProgress)
    {
      _progressService = progressService;
      _saveLoadProgress = saveLoadProgress;
    }

    public override async UniTask Enter()
    {
      await LoadProgressOrInitNew();
    }
 
    private async UniTask LoadProgressOrInitNew()
    {
      _progressService.Progress =
        await _saveLoadProgress.LoadProgress()
        ?? NewProgress();
    }

    private PlayerProgress NewProgress()
    {
      return new PlayerProgress(
        coins: 0,
        level: 1,
        nickname: "Guest");
    }
  }
}
