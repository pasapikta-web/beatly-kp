using System;
using System.Collections.Generic;
using System.Linq;

namespace Beatly.Services;

public class AppNotification
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string ColorClass { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsRead { get; set; }
}

public class NotificationService
{
    private static readonly List<AppNotification> _notifications = new();
    private static int _nextId = 1;

    public List<AppNotification> GetNotifications()
    {
        return _notifications.OrderByDescending(n => n.CreatedAt).ToList();
    }

    public List<AppNotification> GetUserNotifications(string username)
    {
        if (string.IsNullOrEmpty(username)) return new List<AppNotification>();
        return _notifications.Where(n => n.Username == username).OrderByDescending(n => n.CreatedAt).ToList();
    }

    public void AddNotification(string username, string title, string message, string icon = "info", string colorClass = "text-[#499BED]")
    {
        if (string.IsNullOrEmpty(username)) return;

        _notifications.Add(new AppNotification
        {
            Id = _nextId++,
            Username = username,
            Title = title,
            Message = message,
            Icon = icon,
            ColorClass = colorClass,
            CreatedAt = DateTime.UtcNow,
            IsRead = false
        });
    }

    public bool MarkAsRead(int id)
    {
        var notification = _notifications.FirstOrDefault(n => n.Id == id);
        if (notification != null)
        {
            notification.IsRead = true;
            return true;
        }
        return false;
    }

    public bool MarkAsRead(int id, string username)
    {
        if (string.IsNullOrEmpty(username)) return false;

        var notification = _notifications.FirstOrDefault(n => n.Id == id && n.Username == username);
        if (notification != null)
        {
            notification.IsRead = true;
            return true;
        }
        return false;
    }

    public void ClearNotifications()
    {
        _notifications.Clear();
    }

    public void ClearAll(string username)
    {
        if (string.IsNullOrEmpty(username)) return;
        _notifications.RemoveAll(n => n.Username == username);
    }

    public int GetUnreadCount(string username)
    {
        if (string.IsNullOrEmpty(username)) return 0;
        return _notifications.Count(n => n.Username == username && !n.IsRead);
    }
}