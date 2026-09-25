using System;
using System.Collections.Generic;
using CodeBase.Data;
using Cysharp.Threading.Tasks;
using Firebase.Firestore;
using Game.Scripts.Services.Analytics;
using Game.Scripts.Services.Auth;
using Game.Scripts.Services.PersistentProgress;
using UnityEngine;

namespace Game.Scripts.Services.SaveLoad
{
    public class FirestoreSaveLoadService : ISaveLoadService
    {
        private const string ProgressKey = "Progress";
        private const string PlayersCollection = "players";
        private const string CoinsField = "coins";
        private const string LevelField = "level";
        private const string NicknameField = "nickname";
        private const string UpdatedAtField = "updatedAt";
        private const string FirestoreSource = "firestore";
        private const string LocalCacheSource = "local_cache";

        private readonly IPlayerAuthService _playerAuthService;
        private readonly IPersistentProgressService _progressService;
        private readonly IAnalyticsService _analyticsService;

        public FirestoreSaveLoadService(
            IPlayerAuthService playerAuthService,
            IPersistentProgressService progressService,
            IAnalyticsService analyticsService)
        {
            _playerAuthService = playerAuthService;
            _progressService = progressService;
            _analyticsService = analyticsService;
        }

        public async UniTask SaveProgress()
        {
            PlayerProgress progress = _progressService.Progress;

            if (progress == null)
                return;

            SaveLocalProgress(progress);
            _analyticsService.LogProgressSaved(LocalCacheSource);

            if (CanUseFirestore() == false)
                return;

            SaveProgressToFirestore(progress).Forget();
            await UniTask.CompletedTask;
        }

        private async UniTask SaveProgressToFirestore(PlayerProgress progress)
        {
            try
            {
                await PlayerDocument().SetAsync(ToFirestoreData(progress)).AsUniTask();
                Debug.Log($"Player progress saved to Firestore. UserId: {_playerAuthService.UserId}");
                _analyticsService.LogProgressSaved(FirestoreSource);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Failed to save progress to Firestore. Local cache was updated. {exception.Message}");
            }
        }

        public async UniTask<PlayerProgress> LoadProgress()
        {
            if (CanUseFirestore() == false)
                return LoadLocalProgressWithAnalytics();

            try
            {
                DocumentSnapshot snapshot = await PlayerDocument().GetSnapshotAsync().AsUniTask();

                if (snapshot.Exists == false)
                    return LoadLocalProgressWithAnalytics();

                PlayerProgress progress = FromFirestoreData(snapshot.ToDictionary());
                SaveLocalProgress(progress);
                _analyticsService.LogProgressLoaded(FirestoreSource);

                Debug.Log($"Player progress loaded from Firestore. UserId: {_playerAuthService.UserId}");
                return progress;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Failed to load progress from Firestore. Trying local cache. {exception.Message}");
                return LoadLocalProgressWithAnalytics();
            }
        }

        public async UniTask UpdateNickname(string nickname)
        {
            PlayerProgress progress = _progressService.Progress;

            if (progress == null)
                return;

            if (CanUseFirestore() == false)
            {
                progress.Nickname = nickname;
                SaveLocalProgress(progress);
                _analyticsService.LogProgressSaved(LocalCacheSource);
                return;
            }

            try
            {
                await PlayerDocument().SetAsync(
                    new Dictionary<string, object>
                    {
                        [NicknameField] = nickname,
                        [UpdatedAtField] = FieldValue.ServerTimestamp,
                    },
                    SetOptions.MergeAll).AsUniTask();

                progress.Nickname = nickname;
                SaveLocalProgress(progress);
                _analyticsService.LogProgressSaved(FirestoreSource);
                Debug.Log($"Player nickname saved to Firestore. UserId: {_playerAuthService.UserId}");
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Failed to save nickname to Firestore. {exception.Message}");
            }
        }

        private bool CanUseFirestore() =>
            _playerAuthService.IsSignedIn &&
            string.IsNullOrEmpty(_playerAuthService.UserId) == false;

        private DocumentReference PlayerDocument() =>
            FirebaseFirestore.DefaultInstance
                .Collection(PlayersCollection)
                .Document(_playerAuthService.UserId);

        private static Dictionary<string, object> ToFirestoreData(PlayerProgress progress) =>
            new Dictionary<string, object>
            {
                [CoinsField] = progress.Coins,
                [LevelField] = progress.Level,
                [NicknameField] = progress.Nickname,
                [UpdatedAtField] = FieldValue.ServerTimestamp,
            };

        private static PlayerProgress FromFirestoreData(Dictionary<string, object> data) =>
            new PlayerProgress(
                coins: ReadInt(data, CoinsField),
                level: ReadInt(data, LevelField, defaultValue: 1),
                nickname: ReadString(data, NicknameField, defaultValue: "Guest"));

        private static int ReadInt(
            IReadOnlyDictionary<string, object> data,
            string fieldName,
            int defaultValue = 0)
        {
            if (data.TryGetValue(fieldName, out object value) == false || value == null)
                return defaultValue;

            return Convert.ToInt32(value);
        }

        private static string ReadString(
            IReadOnlyDictionary<string, object> data,
            string fieldName,
            string defaultValue)
        {
            if (data.TryGetValue(fieldName, out object value) == false || value == null)
                return defaultValue;

            return value.ToString();
        }

        private static void SaveLocalProgress(PlayerProgress progress)
        {
            PlayerPrefs.SetString(ProgressKey, progress.ToJson());
            PlayerPrefs.Save();
            Debug.Log($"Player progress saved to Local.");
        }

        private static PlayerProgress LoadLocalProgress()
        {
            string progressJson = PlayerPrefs.GetString(ProgressKey);

            if (string.IsNullOrEmpty(progressJson))
                return null;

            return progressJson.ToDeserialized<PlayerProgress>();
        }

        private PlayerProgress LoadLocalProgressWithAnalytics()
        {
            PlayerProgress progress = LoadLocalProgress();

            if (progress != null)
                _analyticsService.LogProgressLoaded(LocalCacheSource);

            return progress;
        }
    }
}
