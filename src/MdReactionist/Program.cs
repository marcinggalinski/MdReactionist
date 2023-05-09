using Discord;
using Discord.WebSocket;

namespace MdReactionist;

public class Program
{
    private static readonly DiscordSocketClient _client = new DiscordSocketClient();
    
    public static async Task Main()
    {
        var token = Environment.GetEnvironmentVariable("MD_BOT_TOKEN");
        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();

        // block this task until the program is closed
        await Task.Delay(-1);
    }
}