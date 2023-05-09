using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

namespace MdReactionist;

public class Program
{
    public static async Task Main()
    {
        var token = Environment.GetEnvironmentVariable("MD_BOT_TOKEN");
        if (string.IsNullOrEmpty(token))
            throw new ArgumentException("Valid token is required for bot to work.");

        var options = GetBotOptions();
        var client  = new DiscordSocketClient(
            new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent
            });
        
        await client.LoginAsync(TokenType.Bot, token);
        await client.StartAsync();

        client.MessageReceived += async messageParam =>
        {
            if (messageParam is not SocketUserMessage message)
                return;

            var isUserMentioned = message.MentionedUsers.Select(x => x.Id).Intersect(options.TriggeringUserIds).Any();
            var isRoleMentioned = message.MentionedRoles.Select(x => x.Id).Intersect(options.TriggeringRoleIds).Any();
            var isContainingSubstring = options.TriggeringSubstrings.Any(x => message.Content.Contains(x, StringComparison.InvariantCultureIgnoreCase));

            if (isUserMentioned || isRoleMentioned || isContainingSubstring)
            {
                await message.AddReactionAsync(Emote.Parse(options.EmoteId));
            }
        };

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
}