using System.ComponentModel.DataAnnotations;
using AspNetCoreApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AspNetCoreApp.ControllersApi;

[ApiController]
[Route("api/[controller]/[action]")]
public class NotificationController : ControllerBase
{
    readonly Notification_DbContext notifDb;
    readonly UserManager<Identity_UserDbModel> userManager;





    public NotificationController(Notification_DbContext notifDb,
    UserManager<Identity_UserDbModel> userManager)
    {
        this.notifDb = notifDb;
        this.userManager = userManager;
    }





    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetNumberOfNotifications()
    {
        string myGuid = await userManager.Users
        .Where(u => u.NormalizedUserName == userManager.NormalizeName(User.Identity!.Name))
        .Select(u => u.UserGuid)
        .FirstAsync();

        int numberOfNotifs = await notifDb.Users
        .Where(u => u.Guid == myGuid)
        .Include(u => u.Notifications)
        .SelectMany(u => u.Notifications)
        .CountAsync();

        return Ok(new { numberOfNotifications = numberOfNotifs });
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetNotifications([FromQuery] int? pageIndex,
    [FromQuery] int? pageSize)
    {
        pageIndex ??= 0;
        pageSize ??= 10;

        string myGuid = await userManager.Users
        .Where(u => u.NormalizedUserName == userManager.NormalizeName(User.Identity!.Name))
        .Select(u => u.UserGuid)
        .FirstAsync();

        Notification_NotifClient_ViewModel[] notifs = await notifDb.Users
        .Where(u => u.Guid == myGuid)
        .Include(u => u.Notifications)
        .SelectMany(u => u.Notifications)
        .OrderByDescending(n => n.CreatedAt)
        .Skip(pageIndex.Value * pageSize.Value)
        .Take(pageSize.Value)
        .Select(n => new Notification_NotifClient_ViewModel()
        {
            CreatedAt = n.CreatedAt,
            Description = n.Description,
            Guid = n.Guid,
            Link = n.Link,
            Title = n.Title,
        })
        .ToArrayAsync();

        return Ok(notifs);
    }

    [HttpDelete]
    [Authorize]
    public async Task<IActionResult> DeleteNotification([FromQuery][StringLength(32)] string notifGuid,
    [FromServices] Notification_Process notifProcess)
    {
        string myGuid = await userManager.Users
        .Where(u => u.NormalizedUserName == userManager.NormalizeName(User.Identity!.Name))
        .Select(u => u.UserGuid)
        .FirstAsync();

        await notifDb.Notifications
        .Include(n => n.Owner)
        .Where(n => n.Guid == notifGuid && n.Owner.Guid == myGuid)
        .ExecuteDeleteAsync();

        //seed
        notifProcess.Delete_NotificationDirectory(notifGuid);

        return Ok(new { success = true });
    }

    [HttpDelete]
    [Authorize]
    public async Task<IActionResult> DeleteAllNotifications([FromServices] Notification_Process notifProcess)
    {
        string myGuid = await userManager.Users
        .Where(u => u.NormalizedUserName == userManager.NormalizeName(User.Identity!.Name))
        .Select(u => u.UserGuid)
        .FirstAsync();

        List<string> deletingGuids = await notifDb.Users
        .Where(u => u.Guid == myGuid)
        .Include(u => u.Notifications)
        .SelectMany(u => u.Notifications)
        .Select(n => n.Guid)
        .ToListAsync();

        await notifDb.Users
        .Where(u => u.Guid == myGuid)
        .Include(u => u.Notifications)
        .SelectMany(u => u.Notifications)
        .ExecuteDeleteAsync();

        //seed
        foreach (string notifGuid in deletingGuids)
        {
            notifProcess.Delete_NotificationDirectory(notifGuid);
        }

        return Ok(new { success = true });
    }


}