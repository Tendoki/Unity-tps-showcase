using Cysharp.Threading.Tasks;
using Firebase.Messaging;
using UnityEngine;

namespace Game.Scripts.Services.Messaging
{
    public class FirebaseMessagingService : IMessagingService
    {
        public string Token { get; private set; }

        public async UniTask InitializeAsync()
        {
            FirebaseMessaging.TokenReceived += OnTokenReceived;
            FirebaseMessaging.MessageReceived += OnMessageReceived;

            Token = await FirebaseMessaging.GetTokenAsync().AsUniTask();

            Debug.Log($"FCM token: {Token}");
        }

        private void OnTokenReceived(object sender, TokenReceivedEventArgs args)
        {
            Token = args.Token;
            Debug.Log($"FCM token received: {Token}");
        }

        private void OnMessageReceived(object sender, MessageReceivedEventArgs args)
        {
            Debug.Log($"FCM message received. From: {args.Message.From}, MessageId: {args.Message.MessageId}");
        }
    }
}
