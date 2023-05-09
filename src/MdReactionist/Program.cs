using Discord;
using Discord.WebSocket;

namespace MdReactionist;

public class Program
{
    private const string EmoteId = "<:md:1025013446682619966>";
    
    private static readonly ulong[] _triggeringUserIds = {
        829016888846581812
    };
    private static readonly ulong[] _triggeringRoleIds = {
        1098699786850422904
    };
    private static readonly string[] _triggeringStrings = {
        "Michał", "Michała", "Michałowi", "Michałem", "Michale"
    };

    private static readonly DiscordSocketClient _client = new DiscordSocketClient(
        new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent
        });
    private static readonly IEmote _mdEmote = Emote.Parse(EmoteId);
    
    public static async Task Main()
    {
        var token = Environment.GetEnvironmentVariable("MD_BOT_TOKEN");
        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();

        _client.MessageReceived += async messageParam =>
        {
            if (messageParam is not SocketUserMessage message)
                return;

            var isUserMentioned = message.MentionedUsers.Select(x => x.Id).Intersect(_triggeringUserIds).Any();
            var isRoleMentioned = message.MentionedRoles.Select(x => x.Id).Intersect(_triggeringRoleIds).Any();
            var isContainingString = _triggeringStrings.Any(x => message.Content.Contains(x));

            if (isUserMentioned || isRoleMentioned || isContainingString)
            {
                await message.AddReactionAsync(_mdEmote);
            }
        };

        // block this task until the program is closed
        await Task.Delay(-1);
    }
}