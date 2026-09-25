using System;
using CodeBase.Data;
using Game.Scripts.Services.PersistentProgress;
using Game.Services;
using TMPro;
using UnityEngine;

namespace Game.Scripts.UI.Elements
{
    public class ActorUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text coins;
        [SerializeField] private TMP_Text level;

        private IPersistentProgressService _progressService;

        private void Awake()
        {
            _progressService = AllServices.Container.Single<IPersistentProgressService>();
        }

        private void Start()
        {
            Refresh();
        }

        private void Update()
        {
            Refresh();
        }

        private void Refresh()
        {
            PlayerProgress progress = _progressService.Progress;

            if (progress == null)
            {
                coins.text = "Coins: -";
                level.text = "Level: -";
                return;
            }

            coins.text = $"Coins: {progress.Coins}";
            level.text = $"Level: {progress.Level}";
        }
    }
}