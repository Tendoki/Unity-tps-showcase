using System;
using CodeBase.Data;

namespace Game.Scripts.Services.Auth.Dtos
{
    [Serializable]
    public class SavePlayerProgressRequestDto
    {
        public int coins;
        public int level;

        public SavePlayerProgressRequestDto(PlayerProgress progress)
        {
            coins = progress.Coins;
            level = progress.Level;
        }
    }
}
