using System;
using System.Collections.Concurrent;

namespace Beatly.Services;

public class UserSubscription
{
    public string Username { get; set; } = string.Empty;
    public string PlanName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive => DateTime.UtcNow <= EndDate;
}

public class SubscriptionService
{
    private static readonly ConcurrentDictionary<string, UserSubscription> _subscriptions = new();

    public UserSubscription? GetSubscription(string username)
    {
        if (string.IsNullOrEmpty(username)) return null;
        _subscriptions.TryGetValue(username, out var sub);
        return sub;
    }

    public void ActivateSubscription(string username, string planName, int monthsDuration)
    {
        if (string.IsNullOrEmpty(username)) return;

        var sub = new UserSubscription
        {
            Username = username,
            PlanName = planName,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMonths(monthsDuration)
        };

        _subscriptions[username] = sub;
    }

    public void CancelSubscription(string username)
    {
        if (string.IsNullOrEmpty(username)) return;
        _subscriptions.TryRemove(username, out _);
    }

    public bool HasActiveSubscription(string userId)
    {
        if (string.IsNullOrEmpty(userId)) return false;
        _subscriptions.TryGetValue(userId, out var sub);
        return sub != null && sub.IsActive;
    }
}