using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace MdReactionist.HostedServices;

public class DiscordBotHostedService : IHostedService
{
    private readonly BotOptions _options;
    private readonly DiscordSocketClient _client;
    private readonly HttpClient _http = new HttpClient();
    private readonly Random _random = new Random();

    private readonly IReadOnlyCollection<char> _charsToSkip = new []
    {
        '`', '~', '!', '^', '&', '*', '(', ')', '+', '=', ':', ';', ',', '.', '[', ']', '{', '}', '\\', '/', '|'
    };

    private Regex _eventReminderRegex = new Regex(string.Empty);

    private readonly IReadOnlyCollection<TimeSpan> _timeSpans = new []
    {
        TimeSpan.Zero,
        TimeSpan.FromMinutes(1),
        TimeSpan.FromMinutes(2),
        TimeSpan.FromMinutes(3),
        TimeSpan.FromMinutes(5),
        TimeSpan.FromMinutes(10),
        TimeSpan.FromMinutes(20),
        TimeSpan.FromMinutes(30),
        TimeSpan.FromMinutes(40),
        TimeSpan.FromMinutes(50),
        TimeSpan.FromHours(1),
        TimeSpan.FromHours(2),
        TimeSpan.FromHours(3),
        TimeSpan.FromHours(4),
        TimeSpan.FromHours(5),
        TimeSpan.FromHours(6),
        TimeSpan.FromHours(7),
        TimeSpan.FromHours(8),
        TimeSpan.FromHours(9),
        TimeSpan.FromHours(10),
        TimeSpan.FromHours(11),
        TimeSpan.FromHours(12),
        TimeSpan.FromHours(13),
        TimeSpan.FromHours(14),
        TimeSpan.FromHours(15),
        TimeSpan.FromHours(16),
        TimeSpan.FromHours(17),
        TimeSpan.FromHours(18),
        TimeSpan.FromHours(19),
        TimeSpan.FromHours(20),
        TimeSpan.FromHours(21),
        TimeSpan.FromHours(22),
        TimeSpan.FromHours(23),
        TimeSpan.FromDays(1),
        TimeSpan.FromDays(2),
        TimeSpan.FromDays(3),
        TimeSpan.FromDays(4),
        TimeSpan.FromDays(5),
        TimeSpan.FromDays(6),
        TimeSpan.FromDays(7)
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
        _client.Ready += SetUpRegex;
        _client.Ready += SetUpSubredditReports;
        _client.Ready += LogBotStart;
        _client.MessageReceived += AddReaction;
        _client.MessageReceived += Correct;
        _client.MessageReceived += RandomReply;
        _client.MessageReceived += ScheduleEventReminders;

        var token = Environment.GetEnvironmentVariable("MD_BOT_TOKEN");
        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _client.MessageReceived -= AddReaction;
        _client.MessageReceived -= Correct;
        _client.MessageReceived -= RandomReply;
        _client.MessageReceived -= ScheduleEventReminders;

        if (_options.Logging is not null)
            await _client.GetGuild(_options.Logging.ServerId).GetTextChannel(_options.Logging.ChannelId).SendMessageAsync("Bot stopped");

        await _client.StopAsync();
        await _client.LogoutAsync();
    }

    private Task SetUpRegex()
    {
        var mention = $"@{_client.CurrentUser.Username}#{_client.CurrentUser.DiscriminatorValue}";
        _eventReminderRegex = new Regex(
            @$"^{mention} (?<name>.+) dnia (?<date>\d{{4}}-\d{{2}}-\d{{2}}) o (?<time>\d{{2}}:\d{{2}})$",
            RegexOptions.Compiled);

        _client.Ready -= SetUpRegex;
        return Task.CompletedTask;
    }

    private Task SetUpSubredditReports()
    {
        async void ReportSubreddit(object? state)
        {
            if (state is not SubredditReport report)
                return;

            if (_client.GetChannel(report.ChannelId) is not SocketTextChannel channel)
                return;

            var response = await _http.GetAsync($"https://reddit.com/r/{report.Subreddit}.json");
            var content = await response.Content.ReadAsStreamAsync();
            var json = await JsonDocument.ParseAsync(content);

            var posts = json.RootElement.GetProperty("data").GetProperty("children");
            for (var i = 0; i < report.Count; i++)
            {
                var post = posts[i];
                var title = post.GetProperty("title");
                var url = post.GetProperty("url");
                var score = post.GetProperty("score");
                var author = post.GetProperty("author");

                var sb = new StringBuilder();
                sb.AppendLine($"[{score}] **{title}** by {author}");
                sb.Append(url);

                await channel.SendMessageAsync(sb.ToString());
            }
        }
        
        foreach (var options in _options.SubredditReports)
        {
            var now = DateTime.UtcNow.AddHours(_options.TimeZoneOffset);
            var scheduledTime = new DateTime(now.Year, now.Month, now.Day, options.ReportTime.Hour, options.ReportTime.Minute, options.ReportTime.Second);
            if (now > scheduledTime)
                scheduledTime = scheduledTime.AddDays(1);

            var _ = new Timer(ReportSubreddit, options, scheduledTime - now, TimeSpan.FromDays(1));
        }

        _client.Ready -= SetUpSubredditReports;
        return Task.CompletedTask;
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
                    .Any(x => msg.Content.Contains(x, StringComparison.OrdinalIgnoreCase));
                isRoleMentioned = options.TriggeringRoleIds.Select(x => $"<@&{x}>")
                    .Any(x => msg.Content.Contains(x, StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                isUserMentioned = msg.MentionedUsers.Select(x => x.Id).Intersect(options.TriggeringUserIds).Any();
                isRoleMentioned = msg.MentionedRoles.Select(x => x.Id).Intersect(options.TriggeringRoleIds).Any();
            }
            var isContainingSubstring = options.TriggeringSubstrings.Any(x => msg.Content.Contains(x, StringComparison.OrdinalIgnoreCase));

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
            
            var index = msg.CleanContent.IndexOf(options.StringToCorrect, StringComparison.Ordinal);
            if (index == -1)
                continue;

            var correctionStart = 0;
            var correctionEnd = msg.CleanContent.Length;

            var prevOmitted = msg.CleanContent[..index].LastOrDefault(x => char.IsWhiteSpace(x) || _charsToSkip.Contains(x));
            if (prevOmitted != default(char))
                correctionStart = msg.CleanContent[..index].LastIndexOf(prevOmitted) + 1;

            var nextOmitted = msg.CleanContent[(index + options.StringToCorrect.Length)..].FirstOrDefault(x => char.IsWhiteSpace(x) || _charsToSkip.Contains(x));
            if (nextOmitted != default(char))
                correctionEnd = index + msg.CleanContent[index..].IndexOf(nextOmitted);

            var correctedString = options.BoldCorrection ? $"**{options.CorrectedString}**" : options.CorrectedString;
            var correction = $"*{msg.CleanContent[correctionStart..index]}{correctedString}{msg.CleanContent[(index + options.StringToCorrect.Length)..correctionEnd]}";
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

    private async Task ScheduleEventReminders(SocketMessage message)
    {
        async void SendEventReminder(object? state)
        {
            if (state is not EventReminder reminder)
                return;

            if (_client.GetChannel(reminder.ChannelId) is not SocketTextChannel channel)
                return;

            var text = reminder.RemainingTime.Days switch
            {
                <= 7 and >= 2 => $"{reminder.EventName} już za {reminder.RemainingTime.Days} dni!",
                1 => $"{reminder.EventName} już za 1 dzień!",
                0 => reminder.RemainingTime.Hours switch
                {
                    (<= 24 and >= 22) or (<= 4 and >= 2) => $"{reminder.EventName} już za {reminder.RemainingTime.Hours} godziny!",
                    <= 21 and >= 5 => $"{reminder.EventName} już za {reminder.RemainingTime.Hours} godzin!",
                    1 => $"{reminder.EventName} już za 1 godzinę!",
                    0 => reminder.RemainingTime.Minutes switch
                    {
                        (<= 60 and >= 55) or (<= 50 and >= 45) or (<= 40 and >= 35) or (<= 30 and >= 25) or (<= 20 and >= 5) => $"{reminder.EventName} już za {reminder.RemainingTime.Minutes} minut!",
                        51 or 41 or 31 or 21 => $"{reminder.EventName} już za {reminder.RemainingTime.Minutes} minut!",
                        (<= 54 and >= 52) or (<= 44 and >= 42) or (<= 34 and >= 32) or (<= 24 and >= 22) or (<= 4 and >= 2) => $"{reminder.EventName} już za {reminder.RemainingTime.Minutes} minuty!",
                        1 => $"{reminder.EventName} już za 1 minutę!",
                        0 => $"It's {reminder.EventName} time! <3",
                        _ => throw new UnreachableException()
                    },
                    _ => throw new UnreachableException()
                },
                _ => throw new UnreachableException()
            };

            await channel.SendMessageAsync(text);
            await reminder.Timer!.DisposeAsync();
        }
        
        if (message is not SocketUserMessage msg)
            return;

        if (msg.MentionedUsers.All(x => x.Id != _client.CurrentUser.Id))
            return;

        if (!_eventReminderRegex.IsMatch(msg.CleanContent))
            return;

        if (!_options.EventReminders.PermittedUserIds.Contains(msg.Author.Id))
        {
            await msg.ReplyAsync("Rejected.");
            return;
        }
        
        var match = _eventReminderRegex.Match(msg.CleanContent);

        if (!match.Groups.TryGetValue("name", out var eventNameGroup))
            return;

        var eventName = eventNameGroup.Value;
        if (string.IsNullOrWhiteSpace(eventName))
            return;
        
        if (!match.Groups.TryGetValue("date", out var dateStringGroup))
            return;
        if (!match.Groups.TryGetValue("time", out var timeStringGroup))
            return;

        var dateTimeString = $"{dateStringGroup.Value} {timeStringGroup.Value}";
        if (!DateTime.TryParse(dateTimeString, out var eventDateTime))
        {
            await msg.ReplyAsync("Nieprawidłowy format daty i/lub czasu wydarzenia.");
            return;
        }

        if (eventDateTime - DateTime.Now > TimeSpan.FromDays(8))
        {
            await msg.ReplyAsync("Wydarzenie zbyt odległe.");
            return;
        }
        
        foreach (var timeSpan in _timeSpans)
        {
            var now = DateTime.UtcNow.AddHours(_options.TimeZoneOffset);
            if (eventDateTime - now < timeSpan)
                continue;
            
            var reminder = new EventReminder(msg.Channel.Id, eventName, timeSpan);
            var timer = new Timer(SendEventReminder, reminder, (eventDateTime - now).Subtract(timeSpan), Timeout.InfiniteTimeSpan);

            reminder.Timer = timer;
        }

        await msg.AddReactionAsync(new Emoji("✅"));
    }

    private record EventReminder(ulong ChannelId, string EventName, TimeSpan RemainingTime)
    {
        public Timer? Timer { get; set; }
    }
}

