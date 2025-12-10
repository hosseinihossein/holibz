using AspNetCoreApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

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





    //[]

}