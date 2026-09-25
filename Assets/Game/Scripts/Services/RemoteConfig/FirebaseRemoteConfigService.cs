using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Firebase.RemoteConfig;
using UnityEngine;

namespace Game.Scripts.Services.RemoteConfig
{
    public class FirebaseRemoteConfigService : IRemoteConfigService
    {
        private const string DebugBonusCoinsKey = "debug_bonus_coins";
        private const int DefaultDebugBonusCoins = 10;

        public int DebugBonusCoins { get; private set; } = DefaultDebugBonusCoins;

        public async UniTask InitializeAsync()
        {
            try
            {
                FirebaseRemoteConfig remoteConfig = FirebaseRemoteConfig.DefaultInstance;

                await remoteConfig.SetDefaultsAsync(new Dictionary<string, object>
                {
                    [DebugBonusCoinsKey] = DefaultDebugBonusCoins,
                }).AsUniTask();

                await remoteConfig.FetchAsync(TimeSpan.Zero).AsUniTask();
                await remoteConfig.ActivateAsync().AsUniTask();

                DebugBonusCoins = (int)remoteConfig.GetValue(DebugBonusCoinsKey).LongValue;

                Debug.Log($"Remote Config loaded. {DebugBonusCoinsKey}: {DebugBonusCoins}");
            }
            catch (Exception exception)
            {
                Debug.LogWarning(
                    $"Remote Config failed. Use default {DebugBonusCoinsKey}: {DebugBonusCoins}. Message: {exception.Message}");
            }
        }
    }
}
