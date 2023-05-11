namespace MdReactionist;

public class BotOptions
{
    public Logging? Logging { get; set; }
    public EmoteReaction[] EmoteReactions { get; set; } = Array.Empty<EmoteReaction>();
    public Correction[] Corrections { get; set; } = Array.Empty<Correction>();
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

public class Logging
{
    public ulong ServerId { get; set; }
    public ulong ChannelId { get; set; }
}
