namespace Backend.Models;

public class Player
{
    public int Id { get; set; }

    public string Nickname { get; set; } = string.Empty;

    public int Coins { get; set; }

    public int Level { get; set; }

    public string AccessToken { get; set; } = string.Empty;
}
