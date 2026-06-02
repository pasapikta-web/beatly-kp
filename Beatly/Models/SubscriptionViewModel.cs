using System;

namespace Beatly.Models;

public class SubscriptionViewModel
{
    public string PlanName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime? EndDate { get; set; }
}