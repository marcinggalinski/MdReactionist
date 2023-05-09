namespace MdReactionist;

public class BotOptions
{
    public string? EmoteId { get; set; }
    public ulong[] TriggeringUserIds { get; set; } = Array.Empty<ulong>();
    public ulong[] TriggeringRoleIds { get; set; } = Array.Empty<ulong>();
    public string[] TriggeringSubstrings { get; set; } = Array.Empty<string>();
}
