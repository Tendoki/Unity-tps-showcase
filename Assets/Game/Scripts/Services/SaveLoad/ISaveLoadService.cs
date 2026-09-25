using CodeBase.Data;
using Cysharp.Threading.Tasks;
using Game.Services;

namespace Game.Scripts.Services.SaveLoad
{
  public interface ISaveLoadService : IService
  {
    UniTask SaveProgress();
    UniTask<PlayerProgress> LoadProgress();
    UniTask UpdateNickname(string nickname);
  }
}
