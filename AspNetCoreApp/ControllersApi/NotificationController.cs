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
        Guid myGuid = await userManager.Users
        .Where(u => u.NormalizedUserName == userManager.NormalizeName(User.Identity!.Name))
        .Select(u => u.UserGuid)
        .FirstAsync();

        int numberOfNotifs = await notifDb.Users
        .Where(u => u.Guid == myGuid)
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

        Guid myGuid = await userManager.Users
        .Where(u => u.NormalizedUserName == userManager.NormalizeName(User.Identity!.Name))
        .Select(u => u.UserGuid)
        .FirstAsync();

        Notification_NotifClient_ViewModel[] notifs = await notifDb.Users
        .Where(u => u.Guid == myGuid)
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
        if (!Guid.TryParseExact(notifGuid, "N", out Guid notifGuid_Guid))
        {
            ModelState.AddModelError("Parse Guid", "Couldn't parse the specified guid!");
            return BadRequest(ModelState);
        }
        Guid myGuid = await userManager.Users
        .Where(u => u.NormalizedUserName == userManager.NormalizeName(User.Identity!.Name))
        .Select(u => u.UserGuid)
        .FirstAsync();

        await notifDb.Notifications
        .Where(n => n.Guid == notifGuid_Guid && n.Owner.Guid == myGuid)
        .ExecuteDeleteAsync();

        return Ok(new { success = true });
    }

    [HttpDelete]
    [Authorize]
    public async Task<IActionResult> DeleteAllNotifications()
    {
        Guid myGuid = await userManager.Users
        .Where(u => u.NormalizedUserName == userManager.NormalizeName(User.Identity!.Name))
        .Select(u => u.UserGuid)
        .FirstAsync();

        await notifDb.Notifications
        .Where(notif => notif.Owner.Guid == myGuid)
        .ExecuteDeleteAsync();

        return Ok(new { success = true });
    }


}