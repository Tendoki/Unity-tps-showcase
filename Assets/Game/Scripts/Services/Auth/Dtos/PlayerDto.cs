using System;
using CodeBase.Data;

namespace Game.Scripts.Services.Auth.Dtos
{
    [Serializable]
    public class PlayerDto
    {
        public int id;
        public string nickname;
        public int coins;
        public int level;

        public PlayerProgress ToPlayerProgress()
        {
            return new PlayerProgress(coins, level, nickname);
        }
    }
}
