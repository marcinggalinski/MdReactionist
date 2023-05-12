using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Options;

namespace MdReactionist.HostedServices;

public class DiscordBotHostedService : IHostedService
{
    private readonly BotOptions _options;
    private readonly DiscordSocketClient _client;
    private readonly Random _random = new Random();

    private readonly IReadOnlyList<char> _charsToSkip = new []
    {
        '`', '~', '!', '^', '&', '*', '(', ')', '+', '=', ':', ';', ',', '.', '[', ']', '{', '}', '\\', '/', '|'
    };

    public DiscordBotHostedService(IOptions<BotOptions> options)
    {
        _options = options.Value;
        _client = new DiscordSocketClient(
            new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent
            });
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _client.Ready += LogBotStart;
        _client.MessageReceived += AddReaction;
        _client.MessageReceived += Correct;
        _client.MessageReceived += RandomReply;

        var token = Environment.GetEnvironmentVariable("MD_BOT_TOKEN");
        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _client.MessageReceived -= AddReaction;
        _client.MessageReceived -= Correct;
        _client.MessageReceived -= RandomReply;

        if (_options.Logging is not null)
            await _client.GetGuild(_options.Logging.ServerId).GetTextChannel(_options.Logging.ChannelId).SendMessageAsync("Bot stopped");

        await _client.StopAsync();
        await _client.LogoutAsync();
    }

    private async Task LogBotStart()
    {
        if (_options.Logging is not null)
            await _client.GetGuild(_options.Logging.ServerId).GetTextChannel(_options.Logging.ChannelId).SendMessageAsync("Bot started");
        _client.Ready -= LogBotStart;
    }

    private async Task AddReaction(SocketMessage message)
    {
        if (message is not SocketUserMessage msg)
            return;

        foreach (var options in _options.EmoteReactions)
        {
            bool isUserMentioned;
            bool isRoleMentioned;

            if (options.IgnoreReplies)
            {
                // manual check for mentions in message content to ignore replies, which are treated the same as mentions in SocketMessage
                isUserMentioned = options.TriggeringUserIds.Select(x => $"<@{x}>")
                    .Any(x => msg.Content.Contains(x, StringComparison.InvariantCultureIgnoreCase));
                isRoleMentioned = options.TriggeringRoleIds.Select(x => $"<@&{x}>")
                    .Any(x => msg.Content.Contains(x, StringComparison.InvariantCultureIgnoreCase));
            }
            else
            {
                isUserMentioned = msg.MentionedUsers.Select(x => x.Id).Intersect(options.TriggeringUserIds).Any();
                isRoleMentioned = msg.MentionedRoles.Select(x => x.Id).Intersect(options.TriggeringRoleIds).Any();
            }
            var isContainingSubstring = options.TriggeringSubstrings.Any(x => msg.Content.Contains(x, StringComparison.InvariantCultureIgnoreCase));

            if (isUserMentioned || isRoleMentioned || isContainingSubstring)
            {
                await msg.AddReactionsAsync(options.EmoteIds.Select(Emote.Parse));
                await msg.AddReactionsAsync(options.Emojis.Select(x => new Emoji(x)));
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

            var prevOmitted = msg.Content[..index].LastOrDefault(x => char.IsWhiteSpace(x) || _charsToSkip.Contains(x));
            if (prevOmitted != default(char))
                correctionStart = msg.Content[..index].LastIndexOf(prevOmitted) + 1;

            var nextOmitted = msg.Content[(index + options.StringToCorrect.Length)..].FirstOrDefault(x => char.IsWhiteSpace(x) || _charsToSkip.Contains(x));
            if (nextOmitted != default(char))
                correctionEnd = index + msg.Content[index..].IndexOf(nextOmitted);

            var correctedString = options.BoldCorrection ? $"**{options.CorrectedString}**" : options.CorrectedString;
            var correction = $"*{msg.Content[correctionStart..correctionEnd].Replace(options.StringToCorrect, correctedString)}";
            await msg.ReplyAsync(correction);
        }
    }
    
    private async Task RandomReply(SocketMessage message)
    {
        if (message is not SocketUserMessage msg)
            return;

        foreach (var options in _options.RandomReplies)
        {
            if (options.TriggeringUserIds.Length > 0 && !options.TriggeringUserIds.Contains(msg.Author.Id))
                continue;

            if (_random.NextSingle() > options.Probability)
                continue;

            var reply = options.Replies[_random.Next(options.Replies.Length)];
            await msg.ReplyAsync(reply);
        }
    }
}

