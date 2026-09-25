using Game.Scripts.Logic;
using UnityEngine;

namespace Game.Infrastructure
{
    public class GameBootstrapper : MonoBehaviour
    {
        [SerializeField] private LoadingCurtain loadingCurtainPrefab;
        [SerializeField] private PlayerDataBackend playerDataBackend = PlayerDataBackend.Firebase;

        private Game _game;

        private void Awake()
        {
            DontDestroyOnLoad(this);

            var loadingCurtain = Instantiate(loadingCurtainPrefab);
            _game = new Game(loadingCurtain, playerDataBackend);
        }

        private void Update()
        {
            _game?.StateMachine.Update();
        }
    }
}
