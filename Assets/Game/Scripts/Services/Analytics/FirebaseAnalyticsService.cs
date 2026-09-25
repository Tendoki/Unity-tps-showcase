using Firebase.Analytics;
using UnityEngine;

namespace Game.Scripts.Services.Analytics
{
    public class FirebaseAnalyticsService : IAnalyticsService
    {
        private const string AnonymousSignInEvent = "auth_anonymous_sign_in";
        private const string FirebaseUserRestoredEvent = "auth_user_restored";
        private const string ProgressLoadedEvent = "progress_loaded";
        private const string ProgressSavedEvent = "progress_saved";
        private const string CoinsChangedEvent = "coins_changed";
        private const string LevelChangedEvent = "level_changed";
        private const string NicknameChangedEvent = "nickname_changed";

        private const string SourceParameter = "source";
        private const string TargetParameter = "target";
        private const string CoinsParameter = "coins";
        private const string LevelParameter = "level";
        private const string NicknameLengthParameter = "nickname_length";

        public void LogAnonymousSignIn(string userId)
        {
            FirebaseAnalytics.SetUserId(userId);
            FirebaseAnalytics.LogEvent(AnonymousSignInEvent);
            Debug.Log($"Analytics event: {AnonymousSignInEvent}");
        }

        public void LogFirebaseUserRestored(string userId)
        {
            FirebaseAnalytics.SetUserId(userId);
            FirebaseAnalytics.LogEvent(FirebaseUserRestoredEvent);
            Debug.Log($"Analytics event: {FirebaseUserRestoredEvent}");
        }

        public void LogProgressLoaded(string source)
        {
            FirebaseAnalytics.LogEvent(ProgressLoadedEvent, SourceParameter, source);
            Debug.Log($"Analytics event: {ProgressLoadedEvent}, source: {source}");
        }

        public void LogProgressSaved(string target)
        {
            FirebaseAnalytics.LogEvent(ProgressSavedEvent, TargetParameter, target);
            Debug.Log($"Analytics event: {ProgressSavedEvent}, target: {target}");
        }

        public void LogCoinsChanged(int coins)
        {
            FirebaseAnalytics.LogEvent(CoinsChangedEvent, CoinsParameter, coins);
            Debug.Log($"Analytics event: {CoinsChangedEvent}, coins: {coins}");
        }

        public void LogLevelChanged(int level)
        {
            FirebaseAnalytics.LogEvent(LevelChangedEvent, LevelParameter, level);
            Debug.Log($"Analytics event: {LevelChangedEvent}, level: {level}");
        }

        public void LogNicknameChanged(string nickname)
        {
            FirebaseAnalytics.LogEvent(NicknameChangedEvent, NicknameLengthParameter, nickname?.Length ?? 0);
            Debug.Log($"Analytics event: {NicknameChangedEvent}");
        }
    }
}
