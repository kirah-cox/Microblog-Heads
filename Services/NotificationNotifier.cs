namespace microblog_heads.Services;

/// <summary>
/// Singleton that tells open notification bells to refresh. NotificationService calls Notify after
/// a user's notifications change (new ones saved, an unlike removed one, or they were marked read);
/// every NotificationBell listens and reloads its count if the user id is its signed-in user.
///
/// This works because Blazor Server keeps each browser connected to this one server process.
/// If the app ever ran on several servers, this would need a shared channel (e.g. Redis pub/sub).
/// </summary>
public class NotificationNotifier
{
    public event Action<int>? NotificationsChanged;

    public void Notify(int userId) => NotificationsChanged?.Invoke(userId);
}
