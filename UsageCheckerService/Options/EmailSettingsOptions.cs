namespace UsageCheckerService;

public class EmailSettingsOptions
{
    public string MainGunApiKey { get; set; }
    public string NotificationEmails { get; set; }
    public bool NotificationEnabled { get; set; }
    public string[] EmailsParsed => NotificationEmails?.Split(',') ?? [];

    public string Subject { get; set; }
    public string Domain { get; set; }
    public string SenderAddress { get; set; }
    public string SenderDisplayName { get; set; }
}