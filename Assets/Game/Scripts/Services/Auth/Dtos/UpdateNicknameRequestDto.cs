using System;

namespace Game.Scripts.Services.Auth.Dtos
{
    [Serializable]
    public class UpdateNicknameRequestDto
    {
        public string nickname;

        public UpdateNicknameRequestDto(string nickname)
        {
            this.nickname = nickname;
        }
    }
}
