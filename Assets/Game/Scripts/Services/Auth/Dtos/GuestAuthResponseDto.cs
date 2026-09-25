using System;

namespace Game.Scripts.Services.Auth.Dtos
{
    [Serializable]
    public class GuestAuthResponseDto
    {
        public int id;
        public string accessToken;
    }
}
