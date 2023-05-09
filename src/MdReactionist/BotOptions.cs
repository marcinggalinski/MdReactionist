namespace MdReactionist;

public class BotOptions
{
    public EmoteReactions[] EmoteReactions { get; set; } = Array.Empty<EmoteReactions>();
}
    
public class EmoteReactions
{
    public string? EmoteId { get; set; }
    public string? Emoji { get; set; }
    public ulong[] TriggeringUserIds { get; set; } = Array.Empty<ulong>();
    public ulong[] TriggeringRoleIds { get; set; } = Array.Empty<ulong>();
    public string[] TriggeringSubstrings { get; set; } = Array.Empty<string>();
}
