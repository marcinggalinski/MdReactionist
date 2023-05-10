using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;

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
        _client.MessageReceived += Correct;
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _client.MessageReceived -= AddReaction;
        _client.MessageReceived -= Correct;

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

    private async Task Correct(SocketMessage message)
    {
        if (message is not SocketUserMessage msg)
            return;

        foreach (var options in _options.Corrections)
        {
            if (string.IsNullOrEmpty(options.StringToCorrect) || string.IsNullOrEmpty(options.StringToCorrect))
                continue;
            
            var index = msg.Content.IndexOf(options.StringToCorrect, StringComparison.InvariantCulture);
            if (index == -1)
                continue;

            var correctionStart = 0;
            var correctionEnd = msg.Content.Length;

            var prevWhitespace = msg.Content[..index].LastOrDefault(char.IsWhiteSpace);
            if (prevWhitespace != default(char))
                correctionStart = msg.Content[..index].LastIndexOf(prevWhitespace) + 1;

            var nextWhitespace = msg.Content[index..].FirstOrDefault(char.IsWhiteSpace);
            if (nextWhitespace != default(char))
                correctionEnd = index + msg.Content[index..].IndexOf(nextWhitespace);

            var correctedString = options.BoldCorrection ? $"**{options.CorrectedString}**" : options.CorrectedString;
            var correction = $"*{msg.Content[correctionStart..correctionEnd].Replace(options.StringToCorrect, correctedString)}";
            await msg.ReplyAsync(correction);
        }
    }
}

