using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

        builder.Services.AddDbContext<PlayerDbContext>(options => options.UseSqlite(connectionString));

        var app = builder.Build();

        app.MapGet("/", () => "Hello World!");
        app.MapGet("/api/health", () => Results.Ok(new { status = "ok" }));
        app.MapPost(
            "/api/auth/guest",
            async (PlayerDbContext db) =>
            {
                var guestId = Guid.NewGuid().ToString("N");
                var player = new Player
                {
                    Nickname = $"Guest_{guestId[..8]}",
                    Coins = 0,
                    Level = 1,
                    AccessToken = Guid.NewGuid().ToString("N")
                };

                db.Players.Add(player);
                await db.SaveChangesAsync();

                return Results.Ok(player);
            }
        );
        app.MapGet(
            "/api/players/me",
            async (HttpRequest request, PlayerDbContext db) =>
            {
                const string bearerPrefix = "Bearer ";
                var authorizationHeader = request.Headers.Authorization.ToString();

                if (!authorizationHeader.StartsWith(bearerPrefix, StringComparison.OrdinalIgnoreCase))
                    return Results.Unauthorized();

                var accessToken = authorizationHeader[bearerPrefix.Length..].Trim();

                if (string.IsNullOrWhiteSpace(accessToken))
                    return Results.Unauthorized();

                var player = await db.Players.FirstOrDefaultAsync(player =>
                    player.AccessToken == accessToken
                );

                return player is null ? Results.Unauthorized() : Results.Ok(player);
            }
        );
        app.MapPatch(
            "/api/players/me",
            async (HttpRequest request, UpdateNicknameRequest updateNicknameRequest, PlayerDbContext db) =>
            {
                const string bearerPrefix = "Bearer ";
                var authorizationHeader = request.Headers.Authorization.ToString();

                if (!authorizationHeader.StartsWith(bearerPrefix, StringComparison.OrdinalIgnoreCase))
                    return Results.Unauthorized();

                var accessToken = authorizationHeader[bearerPrefix.Length..].Trim();

                if (string.IsNullOrWhiteSpace(accessToken))
                    return Results.Unauthorized();

                var nickname = updateNicknameRequest.Nickname?.Trim();

                if (string.IsNullOrWhiteSpace(nickname))
                    return Results.BadRequest(new { error = "Nickname is required." });

                var player = await db.Players.FirstOrDefaultAsync(player =>
                    player.AccessToken == accessToken
                );

                if (player is null)
                    return Results.Unauthorized();

                player.Nickname = nickname;
                await db.SaveChangesAsync();

                return Results.Ok(player);
            }
        );
        app.MapPost(
            "/api/players/me/save",
            async (HttpRequest request, SavePlayerProgressRequest savePlayerProgressRequest, PlayerDbContext db) =>
            {
                const string bearerPrefix = "Bearer ";
                var authorizationHeader = request.Headers.Authorization.ToString();

                if (!authorizationHeader.StartsWith(bearerPrefix, StringComparison.OrdinalIgnoreCase))
                    return Results.Unauthorized();

                var accessToken = authorizationHeader[bearerPrefix.Length..].Trim();

                if (string.IsNullOrWhiteSpace(accessToken))
                    return Results.Unauthorized();

                if (savePlayerProgressRequest.Coins < 0 || savePlayerProgressRequest.Level < 0)
                    return Results.BadRequest(new { error = "Coins and Level cannot be negative." });

                var player = await db.Players.FirstOrDefaultAsync(player =>
                    player.AccessToken == accessToken
                );

                if (player is null)
                    return Results.Unauthorized();

                player.Coins = savePlayerProgressRequest.Coins;
                player.Level = savePlayerProgressRequest.Level;
                await db.SaveChangesAsync();

                return Results.Ok(player);
            }
        );

        app.Run();
    }
}
