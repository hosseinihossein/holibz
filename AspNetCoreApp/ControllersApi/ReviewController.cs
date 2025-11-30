using System.ComponentModel.DataAnnotations;
using AspNetCoreApp.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AspNetCoreApp.ControllersApi;

[ApiController]
[Route("api/[controller]/[action]")]
public class ReviewController : ControllerBase
{
    readonly Review_DbContext reviewDb;
    readonly UserManager<Identity_UserDbModel> userManager;





    public ReviewController(Review_DbContext reviewDb, UserManager<Identity_UserDbModel> userManager)
    {
        this.reviewDb = reviewDb;
        this.userManager = userManager;
    }





    [HttpGet]
    public async Task<IActionResult> GetUserTotalLikes([FromQuery][StringLength(32)] string userGuid)
    {

    }





    [HttpGet]
    public async Task<IActionResult> GetReviewModel([FromQuery][StringLength(32)] string subjectGuid)
    {

    }





    [HttpGet]
    public async Task<IActionResult> GetComments([FromQuery][StringLength(32)] string subjectGuid,
    [FromQuery][StringLength(20)] string? sortedBy, [FromQuery] int? pageIndex, [FromQuery] int? pageSize)
    {

    }

    [HttpPost]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitNewComment([FromForm] Review_NewCommentFormModel formModel)
    {

    }





    [HttpPost]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleLike([FromQuery][StringLength(32)] string subjectGuid)
    {

    }





    [HttpGet]
    public async Task<IActionResult> GetLikedUserList([FromQuery][StringLength(32)] string subjectGuid,
    [FromQuery] int bunch, [FromQuery][StringLength(30)] string filter)
    {

    }
    [HttpGet]
    public async Task<IActionResult> GetThumbsUpUserList([FromQuery][StringLength(32)] string subjectGuid,
    [FromQuery] int bunch, [FromQuery][StringLength(30)] string filter)
    {

    }
    [HttpGet]
    public async Task<IActionResult> GetThumbsDownUserList([FromQuery][StringLength(32)] string subjectGuid,
    [FromQuery] int bunch, [FromQuery][StringLength(30)] string filter)
    {

    }

}