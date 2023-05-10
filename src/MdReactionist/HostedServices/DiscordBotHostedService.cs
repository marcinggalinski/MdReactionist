using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Options;

namespace MdReactionist.HostedServices;

public class DiscordBotHostedService : IHostedService, IDisposable
{
    private readonly BotOptions _options;
    private readonly DiscordSocketClient _client;

    public DiscordBotHostedService(IOptions<BotOptions> options, DiscordSocketClient client)
    {
        _options = options.Value;
        _client = client;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _client.MessageReceived += AddReaction;
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _client.MessageReceived -= AddReaction;

        await _client.StopAsync();
        await _client.LogoutAsync();
    }

    public void Dispose()
    { }

    private async Task AddReaction(SocketMessage message)
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

