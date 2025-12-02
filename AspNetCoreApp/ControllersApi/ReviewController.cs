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
        int userTotalNumberOfLikes = await reviewDb.Reviews
        .Where(r => r.SubjectOwnerGuid == userGuid)
        .SelectMany(r => r.LikedByGuids)
        .CountAsync();

        return Ok(new { totalNumberOfLikes = userTotalNumberOfLikes });
    }





    [HttpGet]
    public async Task<IActionResult> GetReviewModel([FromQuery][StringLength(32)] string subjectGuid)
    {
        Review_ReviewDbModel? reviewDbModel = await reviewDb.Reviews
        .FirstOrDefaultAsync(r => r.SubjectGuid == subjectGuid);
        if (reviewDbModel is null)
        {
            ModelState.AddModelError("subjectGuid", "There's no review with the specified guid!");
            return BadRequest(ModelState);
        }

        string? myGuid = null;
        if ((User.Identity?.IsAuthenticated ?? false) && User.Identity.Name is not null)
        {
            myGuid = await userManager.Users
            .Where(u => u.NormalizedUserName == userManager.NormalizeName(User.Identity.Name))
            .Select(u => u.UserGuid)
            .FirstAsync();
        }

        int totalNumberOfComments = await reviewDb.Reviews
        .Where(r => r.SubjectGuid == subjectGuid)
        .Include(r => r.Comments)
        .ThenInclude(c => c.ReplyTo)
        .SelectMany(r => r.Comments)
        .Where(c => c.ReplyTo == null)
        .CountAsync();

        Review_CommentModel[] myCommentsModels = [];
        Review_CommentModel[] othersCommentsModels = [];
        if (myGuid is not null)
        {
            myCommentsModels = await reviewDb.Reviews
            .Where(r => r.SubjectGuid == subjectGuid)
            .Include(r => r.Comments)
            .ThenInclude(c => c.ReplyTo)
            .Include(r => r.Comments)
            .ThenInclude(c => c.Replies)
            .SelectMany(r => r.Comments)
            .Where(c => c.WriterGuid == myGuid && c.ReplyTo == null)
            .Take(10)
            .Select(c => new Review_CommentModel()
            {
                AmIThumbsDown = c.ThumbsDownBy.Contains(myGuid),
                AmIThumbsUp = c.ThumbsUpBy.Contains(myGuid),
                CreatedAt = c.CreatedAt,
                Guid = c.Guid,
                IsReply = false,
                NumberOfReplies = c.Replies.Count,
                NumberOfThumbsDowns = c.ThumbsDownBy.Count,
                NumberOfThumbsUps = c.ThumbsUpBy.Count,
                ReplyToBrief = "",
                ReplyToGuid = "",
                ReplyToUsername = "",
                Text = c.Text,
                WriterGuid = c.WriterGuid,
            })
            .AsSplitQuery()
            .ToArrayAsync();

            if (myCommentsModels.Length < 10)
            {
                int numberOfNeededOthersComments = 10 - myCommentsModels.Length;

                othersCommentsModels = await reviewDb.Reviews
                .Where(r => r.SubjectGuid == subjectGuid)
                .Include(r => r.Comments)
                .ThenInclude(c => c.ReplyTo)
                .Include(r => r.Comments)
                .ThenInclude(c => c.Replies)
                .SelectMany(r => r.Comments)
                .Where(c => c.WriterGuid != myGuid && c.ReplyTo == null)
                .Take(numberOfNeededOthersComments)
                .Select(c => new Review_CommentModel()
                {
                    AmIThumbsDown = c.ThumbsDownBy.Contains(myGuid),
                    AmIThumbsUp = c.ThumbsUpBy.Contains(myGuid),
                    CreatedAt = c.CreatedAt,
                    Guid = c.Guid,
                    IsReply = false,
                    NumberOfReplies = c.Replies.Count,
                    NumberOfThumbsDowns = c.ThumbsDownBy.Count,
                    NumberOfThumbsUps = c.ThumbsUpBy.Count,
                    ReplyToBrief = "",
                    ReplyToGuid = "",
                    ReplyToUsername = "",
                    Text = c.Text,
                    WriterGuid = c.WriterGuid,
                })
                .AsSplitQuery()
                .ToArrayAsync();
            }
        }
        else
        {
            othersCommentsModels = await reviewDb.Reviews
            .Where(r => r.SubjectGuid == subjectGuid)
            .Include(r => r.Comments)
            .ThenInclude(c => c.ReplyTo)
            .Include(r => r.Comments)
            .ThenInclude(c => c.Replies)
            .SelectMany(r => r.Comments)
            .Where(c => c.ReplyTo == null)
            .Take(10)
            .Select(c => new Review_CommentModel()
            {
                AmIThumbsDown = false,
                AmIThumbsUp = false,
                CreatedAt = c.CreatedAt,
                Guid = c.Guid,
                IsReply = false,
                NumberOfReplies = c.Replies.Count,
                NumberOfThumbsDowns = c.ThumbsDownBy.Count,
                NumberOfThumbsUps = c.ThumbsUpBy.Count,
                ReplyToBrief = "",
                ReplyToGuid = "",
                ReplyToUsername = "",
                Text = c.Text,
                WriterGuid = c.WriterGuid,
            })
            .AsSplitQuery()
            .ToArrayAsync();
        }

        Review_ReviewModel reviewModel = new()
        {
            AmILiked = myGuid is not null && reviewDbModel.LikedByGuids.Contains(myGuid),
            Comments = [.. myCommentsModels, .. othersCommentsModels],
            NumberOfLikes = reviewDbModel.LikedByGuids.Count,
            TotalNumberOfComments = totalNumberOfComments,
        };

        return Ok(reviewModel);
    }





    [HttpGet]
    public async Task<IActionResult> GetComments([FromQuery][StringLength(32)] string subjectGuid,
    [FromQuery][StringLength(20)] string? orderBy, [FromQuery] int? pageIndex, [FromQuery] int? pageSize)
    {
        Review_ReviewDbModel? reviewDbModel = await reviewDb.Reviews
        .FirstOrDefaultAsync(r => r.SubjectGuid == subjectGuid);
        if (reviewDbModel is null)
        {
            ModelState.AddModelError("subjectGuid", "There's no review with the specified guid!");
            return BadRequest(ModelState);
        }

        pageIndex ??= 0;
        pageSize ??= 10;

        int totalNumberOfComments = await reviewDb.Reviews
        .Where(r => r.SubjectGuid == subjectGuid)
        .Include(r => r.Comments)
        .ThenInclude(c => c.ReplyTo)
        .SelectMany(r => r.Comments)
        .Where(c => c.ReplyTo == null)
        .CountAsync();

        if (pageIndex.Value > 0 && (pageIndex.Value * pageSize) >= totalNumberOfComments)
        {
            return Ok(Array.Empty<Review_CommentModel>());
        }

        string? myGuid = null;
        if ((User.Identity?.IsAuthenticated ?? false) && User.Identity.Name is not null)
        {
            myGuid = await userManager.Users
            .Where(u => u.NormalizedUserName == userManager.NormalizeName(User.Identity.Name))
            .Select(u => u.UserGuid)
            .FirstAsync();
        }

        if (orderBy is null)
        {
            Review_CommentModel[] myCommentsModels = [];
            Review_CommentModel[] othersCommentsModels = [];
            if (myGuid is not null)
            {
                myCommentsModels = await reviewDb.Reviews
                .Where(r => r.SubjectGuid == subjectGuid)
                .Include(r => r.Comments)
                .ThenInclude(c => c.ReplyTo)
                .Include(r => r.Comments)
                .ThenInclude(c => c.Replies)
                .SelectMany(r => r.Comments)
                .Where(c => c.WriterGuid == myGuid && c.ReplyTo == null)
                .Skip(pageIndex.Value * pageSize.Value)
                .Take(pageSize.Value)
                .Select(c => new Review_CommentModel()
                {
                    AmIThumbsDown = c.ThumbsDownBy.Contains(myGuid),
                    AmIThumbsUp = c.ThumbsUpBy.Contains(myGuid),
                    CreatedAt = c.CreatedAt,
                    Guid = c.Guid,
                    IsReply = false,
                    NumberOfReplies = c.Replies.Count,
                    NumberOfThumbsDowns = c.ThumbsDownBy.Count,
                    NumberOfThumbsUps = c.ThumbsUpBy.Count,
                    ReplyToBrief = "",
                    ReplyToGuid = "",
                    ReplyToUsername = "",
                    Text = c.Text,
                    WriterGuid = c.WriterGuid,
                })
                .AsSplitQuery()
                .ToArrayAsync();

                if (myCommentsModels.Length < pageSize.Value)
                {
                    int totalNumberOfMyComments = await reviewDb.Reviews
                    .Where(r => r.SubjectGuid == subjectGuid)
                    .Include(r => r.Comments)
                    .ThenInclude(c => c.ReplyTo)
                    .SelectMany(r => r.Comments)
                    .Where(c => c.WriterGuid == myGuid && c.ReplyTo == null)
                    .CountAsync();
                    int numberOfSkipOthersComments = (pageIndex.Value * pageSize.Value) - totalNumberOfMyComments;
                    int numberOfNeededOthersComments = pageSize.Value - myCommentsModels.Length;

                    othersCommentsModels = await reviewDb.Reviews
                    .Where(r => r.SubjectGuid == subjectGuid)
                    .Include(r => r.Comments)
                    .ThenInclude(c => c.ReplyTo)
                    .Include(r => r.Comments)
                    .ThenInclude(c => c.Replies)
                    .SelectMany(r => r.Comments)
                    .Where(c => c.WriterGuid != myGuid && c.ReplyTo == null)
                    .Skip(numberOfSkipOthersComments)
                    .Take(numberOfNeededOthersComments)
                    .Select(c => new Review_CommentModel()
                    {
                        AmIThumbsDown = c.ThumbsDownBy.Contains(myGuid),
                        AmIThumbsUp = c.ThumbsUpBy.Contains(myGuid),
                        CreatedAt = c.CreatedAt,
                        Guid = c.Guid,
                        IsReply = false,
                        NumberOfReplies = c.Replies.Count,
                        NumberOfThumbsDowns = c.ThumbsDownBy.Count,
                        NumberOfThumbsUps = c.ThumbsUpBy.Count,
                        ReplyToBrief = "",
                        ReplyToGuid = "",
                        ReplyToUsername = "",
                        Text = c.Text,
                        WriterGuid = c.WriterGuid,
                    })
                    .AsSplitQuery()
                    .ToArrayAsync();
                }
            }
            else
            {
                othersCommentsModels = await reviewDb.Reviews
                .Where(r => r.SubjectGuid == subjectGuid)
                .Include(r => r.Comments)
                .ThenInclude(c => c.ReplyTo)
                .Include(r => r.Comments)
                .ThenInclude(c => c.Replies)
                .SelectMany(r => r.Comments)
                .Where(c => c.ReplyTo == null)
                .Skip(pageIndex.Value * pageSize.Value)
                .Take(pageSize.Value)
                .Select(c => new Review_CommentModel()
                {
                    AmIThumbsDown = false,
                    AmIThumbsUp = false,
                    CreatedAt = c.CreatedAt,
                    Guid = c.Guid,
                    IsReply = false,
                    NumberOfReplies = c.Replies.Count,
                    NumberOfThumbsDowns = c.ThumbsDownBy.Count,
                    NumberOfThumbsUps = c.ThumbsUpBy.Count,
                    ReplyToBrief = "",
                    ReplyToGuid = "",
                    ReplyToUsername = "",
                    Text = c.Text,
                    WriterGuid = c.WriterGuid,
                })
                .AsSplitQuery()
                .ToArrayAsync();
            }

            Review_CommentModel[] selectedComments = [.. myCommentsModels, .. othersCommentsModels];
            return Ok(selectedComments);
        }
        else//orderBy
        {
            if (orderBy == "Newest")
            {
                Review_CommentModel[] commentsModels = await reviewDb.Reviews
                .Where(r => r.SubjectGuid == subjectGuid)
                .Include(r => r.Comments)
                .ThenInclude(c => c.ReplyTo)
                .Include(r => r.Comments)
                .ThenInclude(c => c.Replies)
                .SelectMany(r => r.Comments)
                .Where(c => c.ReplyTo == null)
                .OrderByDescending(c => c.CreatedAt)
                .Skip(pageIndex.Value * pageSize.Value)
                .Take(pageSize.Value)
                .Select(c => new Review_CommentModel()
                {
                    AmIThumbsDown = false,
                    AmIThumbsUp = false,
                    CreatedAt = c.CreatedAt,
                    Guid = c.Guid,
                    IsReply = false,
                    NumberOfReplies = c.Replies.Count,
                    NumberOfThumbsDowns = c.ThumbsDownBy.Count,
                    NumberOfThumbsUps = c.ThumbsUpBy.Count,
                    ReplyToBrief = "",
                    ReplyToGuid = "",
                    ReplyToUsername = "",
                    Text = c.Text,
                    WriterGuid = c.WriterGuid,
                })
                .AsSplitQuery()
                .ToArrayAsync();

                return Ok(commentsModels);
            }

            if (orderBy == "Oldest")
            {
                Review_CommentModel[] commentsModels = await reviewDb.Reviews
                .Where(r => r.SubjectGuid == subjectGuid)
                .Include(r => r.Comments)
                .ThenInclude(c => c.ReplyTo)
                .Include(r => r.Comments)
                .ThenInclude(c => c.Replies)
                .SelectMany(r => r.Comments)
                .Where(c => c.ReplyTo == null)
                .OrderBy(c => c.CreatedAt)
                .Skip(pageIndex.Value * pageSize.Value)
                .Take(pageSize.Value)
                .Select(c => new Review_CommentModel()
                {
                    AmIThumbsDown = false,
                    AmIThumbsUp = false,
                    CreatedAt = c.CreatedAt,
                    Guid = c.Guid,
                    IsReply = false,
                    NumberOfReplies = c.Replies.Count,
                    NumberOfThumbsDowns = c.ThumbsDownBy.Count,
                    NumberOfThumbsUps = c.ThumbsUpBy.Count,
                    ReplyToBrief = "",
                    ReplyToGuid = "",
                    ReplyToUsername = "",
                    Text = c.Text,
                    WriterGuid = c.WriterGuid,
                })
                .AsSplitQuery()
                .ToArrayAsync();

                return Ok(commentsModels);
            }

            if (orderBy == "Most Agreed")
            {
                Review_CommentModel[] commentsModels = await reviewDb.Reviews
                .Where(r => r.SubjectGuid == subjectGuid)
                .Include(r => r.Comments)
                .ThenInclude(c => c.ReplyTo)
                .Include(r => r.Comments)
                .ThenInclude(c => c.Replies)
                .SelectMany(r => r.Comments)
                .Where(c => c.ReplyTo == null)
                .OrderByDescending(c => c.ThumbsUpBy.Count)
                .Skip(pageIndex.Value * pageSize.Value)
                .Take(pageSize.Value)
                .Select(c => new Review_CommentModel()
                {
                    AmIThumbsDown = false,
                    AmIThumbsUp = false,
                    CreatedAt = c.CreatedAt,
                    Guid = c.Guid,
                    IsReply = false,
                    NumberOfReplies = c.Replies.Count,
                    NumberOfThumbsDowns = c.ThumbsDownBy.Count,
                    NumberOfThumbsUps = c.ThumbsUpBy.Count,
                    ReplyToBrief = "",
                    ReplyToGuid = "",
                    ReplyToUsername = "",
                    Text = c.Text,
                    WriterGuid = c.WriterGuid,
                })
                .AsSplitQuery()
                .ToArrayAsync();

                return Ok(commentsModels);
            }

            return Ok(Array.Empty<Review_CommentModel>());
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetReplies([FromQuery][StringLength(32)] string subjectGuid,
    [FromQuery] int? pageIndex)
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
    [HttpPost]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleThumbsUp([FromQuery][StringLength(32)] string subjectGuid)
    {

    }
    [HttpPost]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleThumbsDown([FromQuery][StringLength(32)] string subjectGuid)
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