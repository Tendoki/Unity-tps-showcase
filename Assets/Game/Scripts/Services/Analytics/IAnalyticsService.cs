using Game.Services;

namespace Game.Scripts.Services.Analytics
{
    public interface IAnalyticsService : IService
    {
        void LogAnonymousSignIn(string userId);
        void LogFirebaseUserRestored(string userId);
        void LogProgressLoaded(string source);
        void LogProgressSaved(string target);
        void LogCoinsChanged(int coins);
        void LogLevelChanged(int level);
        void LogNicknameChanged(string nickname);
    }
}
