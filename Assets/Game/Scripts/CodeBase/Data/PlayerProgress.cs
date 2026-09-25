using System;

namespace CodeBase.Data
{
    [Serializable]
    public class PlayerProgress
    {
        public int Coins;
        public int Level;
        public string Nickname;

        public PlayerProgress()
        {
        }

        public PlayerProgress(int coins, int level, string nickname)
        {
            Coins = coins;
            Level = level;
            Nickname = nickname;
        }
    }
}
