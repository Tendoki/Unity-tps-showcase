using UnityEngine;

namespace Game.Scripts.Development
{
    public class ProgressDebugView : MonoBehaviour
    {
        [SerializeField] private ProgressDebugController controller;

        private void Awake()
        {
            if (controller == null)
            {
                controller = FindAnyObjectByType<ProgressDebugController>();
            }
        }

        public void LogCurrent()
        {
            controller.LogCurrent();
        }

        public void AddCoins()
        {
            controller.AddCoins();
        }

        public void LevelUp()
        {
            controller.LevelUp();
        }

        public void ApplyNickname()
        {
            controller.ApplyNickname();
        }

        public void Save()
        {
            controller.Save();
        }

        public void TestLocalNotification()
        {
            controller.TestLocalNotification();
        }

        public void Crash()
        {
            controller.Crash();
        }
    }
}
