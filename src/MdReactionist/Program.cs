using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

namespace MdReactionist;

public class Program
{
    private static BotOptions _options = new BotOptions();
    
    public static async Task Main()
    {
        var token = Environment.GetEnvironmentVariable("MD_BOT_TOKEN");
        if (string.IsNullOrEmpty(token))
            throw new ArgumentException("Valid token is required for bot to work.");

        _options = GetBotOptions();
        var client  = new DiscordSocketClient(
            new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent
            });
        
        await client.LoginAsync(TokenType.Bot, token);
        await client.StartAsync();

        client.MessageReceived += AddReaction;

        // block this task until the program is closed
        await Task.Delay(-1);
    }

    private static BotOptions GetBotOptions()
    {
        var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json");
        var config = builder.Build();
        var botOptions = config.GetSection("BotOptions").Get<BotOptions>();

        return botOptions ?? new BotOptions();
    }

    private static async Task AddReaction(SocketMessage message)
    {
        if (message is not SocketUserMessage msg)
            return;

        foreach (var options in _options.EmoteReactions)
        {
            // manual check for mentions in message content to ignore replies, which are treated the same as mentions in SocketMessage
            var isUserMentioned = options.TriggeringUserIds.Select(x => $"<@{x}>")
                .Any(x => msg.Content.Contains(x, StringComparison.InvariantCultureIgnoreCase));
            var isRoleMentioned = options.TriggeringRoleIds.Select(x => $"<@&{x}>")
                .Any(x => msg.Content.Contains(x, StringComparison.InvariantCultureIgnoreCase));
            var isContainingSubstring = options.TriggeringSubstrings.Any(x => msg.Content.Contains(x, StringComparison.InvariantCultureIgnoreCase));

            
            if (isUserMentioned || isRoleMentioned || isContainingSubstring)
            {
                if (options.EmoteId is not null)
                    await msg.AddReactionAsync(Emote.Parse(options.EmoteId));
                else if (options.Emoji is not null)
                    await msg.AddReactionAsync(new Emoji(options.Emoji));
            }
        }
    }
}