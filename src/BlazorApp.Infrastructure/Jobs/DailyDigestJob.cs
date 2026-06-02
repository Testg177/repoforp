using BlazorApp.Application.Interfaces;
using BlazorApp.Domain.Entities;
using BlazorApp.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Quartz;

namespace BlazorApp.Infrastructure.Jobs;

public sealed class DailyDigestJob : IJob
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly INotificationService _notificationService;
    private readonly IFormsService _formsService;

    public DailyDigestJob(UserManager<ApplicationUser> userManager, INotificationService notificationService, IFormsService formsService)
    {
        _userManager = userManager;
        _notificationService = notificationService;
        _formsService = formsService;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var managers = new List<ApplicationUser>();
        managers.AddRange(await _userManager.GetUsersInRoleAsync("Manager"));
        managers.AddRange(await _userManager.GetUsersInRoleAsync("Admin"));
        managers.AddRange(await _userManager.GetUsersInRoleAsync("SuperAdmin"));

        var pendingCount = (await _formsService.GetPendingApprovalsAsync(context.CancellationToken)).Count;
        foreach (var manager in managers.DistinctBy(x => x.Id))
        {
            await _notificationService.CreateAsync(
                manager.Id,
                NotificationType.SystemAlert,
                "Daily digest",
                $"Do zatwierdzenia pozostaje {pendingCount} formularzy.",
                "/approvals",
                null,
                null,
                context.CancellationToken);
        }
    }
}
