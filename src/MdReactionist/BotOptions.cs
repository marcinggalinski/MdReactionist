﻿namespace MdReactionist;

public class BotOptions
{
    public int TimeZoneOffset { get; set; }
    public Logging? Logging { get; set; }
    public EmoteReaction[] EmoteReactions { get; set; } = Array.Empty<EmoteReaction>();
    public Correction[] Corrections { get; set; } = Array.Empty<Correction>();
    public RandomReply[] RandomReplies { get; set; } = Array.Empty<RandomReply>();
    public EventReminders EventReminders { get; set; } = new EventReminders();
    public SubredditReport[] SubredditReports { get; set; } = Array.Empty<SubredditReport>();
}

public class Logging
{
    public ulong ServerId { get; set; }
    public ulong ChannelId { get; set; }
}
    
public class EmoteReaction
{
    public string[] EmoteIds { get; set; } = Array.Empty<string>();
    public string[] Emojis { get; set; } = Array.Empty<string>();
    public ulong[] TriggeringUserIds { get; set; } = Array.Empty<ulong>();
    public ulong[] TriggeringRoleIds { get; set; } = Array.Empty<ulong>();
    public string[] TriggeringSubstrings { get; set; } = Array.Empty<string>();
    public bool IgnoreReplies { get; set; } = true;
}

public class Correction
{
    public string? StringToCorrect { get; set; }
    public string? CorrectedString { get; set; }
    public bool BoldCorrection { get; set; }
}

public class RandomReply
{
    public ulong[] TriggeringUserIds { get; set; } = Array.Empty<ulong>();
    public float Probability { get; set; }
    public string[] Replies { get; set; } = Array.Empty<string>();
}

public class EventReminders
{
    public ulong[] PermittedUserIds { get; set; } = Array.Empty<ulong>();
}

public class SubredditReport
{
    public ulong ChannelId { get; set; }
    public uint Count { get; set; }
    public string? Subreddit { get; set; }
    public TimeOnly ReportTime { get; set; }
    
}
