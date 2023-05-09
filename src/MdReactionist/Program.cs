using Discord;
using Discord.WebSocket;

namespace MdReactionist;

public class Program
{
    private const ulong UserId = 829016888846581812;
    private const ulong RoleId = 1098699786850422904;
    private const string EmoteId = "<:md:1025013446682619966>";
        
    private static readonly DiscordSocketClient _client = new DiscordSocketClient();
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

            if (message.MentionedUsers.Any(x => x.Id == UserId) || message.MentionedRoles.Any(x => x.Id == RoleId))
            {
                await message.AddReactionAsync(_mdEmote);
            }
        };

        // block this task until the program is closed
        await Task.Delay(-1);
    }
}