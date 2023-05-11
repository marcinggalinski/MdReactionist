namespace MdReactionist;

public class BotOptions
{
    public Logging? Logging { get; set; }
    public EmoteReaction[] EmoteReactions { get; set; } = Array.Empty<EmoteReaction>();
    public Correction[] Corrections { get; set; } = Array.Empty<Correction>();
    public RandomReply[] RandomReplies { get; set; } = Array.Empty<RandomReply>();
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
    public ulong TriggeringUserId { get; set; }
    public float Probability { get; set; }
    public string[] Replies { get; set; } = Array.Empty<string>();
}
