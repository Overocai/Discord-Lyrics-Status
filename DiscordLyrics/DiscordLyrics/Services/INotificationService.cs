namespace DiscordLyrics.Services;

public enum NotificationKind { Info, Success, Warning, Error }

public sealed record Notification(string Message, NotificationKind Kind, string? Title = null);

/// <summary>Raises elegant in-app toasts. The shell listens and renders them.</summary>
public interface INotificationService
{
    event Action<Notification>? Shown;
    void Info(string message, string? title = null);
    void Success(string message, string? title = null);
    void Warning(string message, string? title = null);
    void Error(string message, string? title = null);
}

public sealed class NotificationService : INotificationService
{
    public event Action<Notification>? Shown;

    public void Info(string m, string? t = null) => Shown?.Invoke(new(m, NotificationKind.Info, t));
    public void Success(string m, string? t = null) => Shown?.Invoke(new(m, NotificationKind.Success, t));
    public void Warning(string m, string? t = null) => Shown?.Invoke(new(m, NotificationKind.Warning, t));
    public void Error(string m, string? t = null) => Shown?.Invoke(new(m, NotificationKind.Error, t));
}
