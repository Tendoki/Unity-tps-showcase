using CodeBase.Data;
using Game.Services;

namespace Game.Scripts.Services.PersistentProgress
{
  public interface IPersistentProgressService : IService
  {
    PlayerProgress Progress { get; set; }
  }
}