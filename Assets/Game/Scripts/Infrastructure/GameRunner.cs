using Game.Scripts.Logic;
using UnityEngine;

namespace Game.Infrastructure
{
  public class GameRunner : MonoBehaviour
  {
    public GameBootstrapper BootstrapperPrefab;
    
    private void Awake()
    {
      var bootstrapper = FindFirstObjectByType<GameBootstrapper>();
      
      if(bootstrapper != null) return;
      
      Instantiate(BootstrapperPrefab);
    }
  }
}
