using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Infrastructure
{
    public class SceneLoader
    {
        public async UniTask Load(string nextScene)
        {
            if (SceneManager.GetActiveScene().name == nextScene)
                return;

            AsyncOperation waitNextScene = SceneManager.LoadSceneAsync(nextScene);
            await waitNextScene.ToUniTask();
        }
    }
}
