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
        //.ThenInclude(c => c.ReplyTo)
        .SelectMany(r => r.Comments)
        //.Where(c => c.ReplyTo == null)
        .CountAsync();

        Review_CommentModel[] myCommentsModels = [];
        Review_CommentModel[] othersCommentsModels = [];
        if (myGuid is not null)
        {
            myCommentsModels = await reviewDb.Reviews
            .Where(r => r.SubjectGuid == subjectGuid)
            .Include(r => r.Comments)
            //.ThenInclude(c => c.ReplyTo)
            .Include(r => r.Comments)
            .ThenInclude(c => c.Replies)
            .SelectMany(r => r.Comments)
            .Where(c => c.WriterGuid == myGuid/* && c.ReplyTo == null*/)
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
                //.ThenInclude(c => c.ReplyTo)
                .Include(r => r.Comments)
                .ThenInclude(c => c.Replies)
                .SelectMany(r => r.Comments)
                .Where(c => c.WriterGuid != myGuid/* && c.ReplyTo == null*/)
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
            //.ThenInclude(c => c.ReplyTo)
            .Include(r => r.Comments)
            .ThenInclude(c => c.Replies)
            .SelectMany(r => r.Comments)
            //.Where(c => c.ReplyTo == null)
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
        //.ThenInclude(c => c.ReplyTo)
        .SelectMany(r => r.Comments)
        //.Where(c => c.ReplyTo == null)
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
                //.ThenInclude(c => c.ReplyTo)
                .Include(r => r.Comments)
                .ThenInclude(c => c.Replies)
                .SelectMany(r => r.Comments)
                .Where(c => c.WriterGuid == myGuid/* && c.ReplyTo == null*/)
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
                    int totalNumberOfMyComments;
                    if (pageIndex.Value > 0)
                    {
                        totalNumberOfMyComments = await reviewDb.Reviews
                        .Where(r => r.SubjectGuid == subjectGuid)
                        .Include(r => r.Comments)
                        //.ThenInclude(c => c.ReplyTo)
                        .SelectMany(r => r.Comments)
                        .Where(c => c.WriterGuid == myGuid/* && c.ReplyTo == null*/)
                        .CountAsync();
                    }
                    else
                    {
                        totalNumberOfMyComments = myCommentsModels.Length;
                    }

                    int numberOfSkipOthersComments = (pageIndex.Value * pageSize.Value) - totalNumberOfMyComments;
                    if (numberOfSkipOthersComments < 0) numberOfSkipOthersComments = 0;

                    int numberOfNeededOthersComments = pageSize.Value - myCommentsModels.Length;

                    othersCommentsModels = await reviewDb.Reviews
                    .Where(r => r.SubjectGuid == subjectGuid)
                    .Include(r => r.Comments)
                    //.ThenInclude(c => c.ReplyTo)
                    .Include(r => r.Comments)
                    .ThenInclude(c => c.Replies)
                    .SelectMany(r => r.Comments)
                    .Where(c => c.WriterGuid != myGuid/* && c.ReplyTo == null*/)
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
                //.ThenInclude(c => c.ReplyTo)
                .Include(r => r.Comments)
                .ThenInclude(c => c.Replies)
                .SelectMany(r => r.Comments)
                //.Where(c => c.ReplyTo == null)
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
                //.ThenInclude(c => c.ReplyTo)
                .Include(r => r.Comments)
                .ThenInclude(c => c.Replies)
                .SelectMany(r => r.Comments)
                //.Where(c => c.ReplyTo == null)
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
                //.ThenInclude(c => c.ReplyTo)
                .Include(r => r.Comments)
                .ThenInclude(c => c.Replies)
                .SelectMany(r => r.Comments)
                //.Where(c => c.ReplyTo == null)
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
                //.ThenInclude(c => c.ReplyTo)
                .Include(r => r.Comments)
                .ThenInclude(c => c.Replies)
                .SelectMany(r => r.Comments)
                //.Where(c => c.ReplyTo == null)
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
    public async Task<IActionResult> GetReplies([FromQuery][StringLength(32)] string commentGuid,
    [FromQuery] int? bunchIndex)
    {
        Review_CommentDbModel? commentDbModel = await reviewDb.Comments
        .FirstOrDefaultAsync(c => c.Guid == commentGuid);
        if (commentDbModel is null)
        {
            ModelState.AddModelError("commentGuid", "There's not comment with the specified guid!");
            return BadRequest(ModelState);
        }

        string commentWriterUsername = await userManager.Users
        .Where(u => u.UserGuid == commentDbModel.WriterGuid)
        .Select(u => u.UserName)
        .FirstOrDefaultAsync() ?? "Unkown UserName";

        string? myGuid = null;
        if ((User.Identity?.IsAuthenticated ?? false) && User.Identity.Name is not null)
        {
            myGuid = await userManager.Users
            .Where(u => u.NormalizedUserName == userManager.NormalizeName(User.Identity.Name))
            .Select(u => u.UserGuid)
            .FirstAsync();
        }

        bunchIndex ??= 0;

        Review_CommentModel[] myRepliesModels = [];
        Review_CommentModel[] othersRepliesModels = [];
        if (myGuid is not null)
        {
            myRepliesModels = await reviewDb.Comments
            .Where(c => c.Guid == commentGuid)
            .Include(c => c.Replies)
            .ThenInclude(rep => rep.Replies)
            .SelectMany(c => c.Replies)
            .Where(rep => rep.WriterGuid == myGuid)
            .Skip(bunchIndex.Value * 10)
            .Take(10)
            .Select(rep => new Review_CommentModel()
            {
                AmIThumbsDown = rep.ThumbsDownBy.Contains(myGuid),
                AmIThumbsUp = rep.ThumbsUpBy.Contains(myGuid),
                CreatedAt = rep.CreatedAt,
                Guid = rep.Guid,
                IsReply = true,
                NumberOfReplies = rep.Replies.Count,
                NumberOfThumbsDowns = rep.ThumbsDownBy.Count,
                NumberOfThumbsUps = rep.ThumbsUpBy.Count,
                ReplyToBrief = rep.ReplyTo!.Text.Substring(0, 128),
                ReplyToGuid = rep.ReplyTo.Guid,
                ReplyToUsername = commentWriterUsername,
                Text = rep.Text,
                WriterGuid = rep.WriterGuid,
            })
            .ToArrayAsync();

            if (myRepliesModels.Length < 10)
            {
                int totalNumberOfMyReplies;
                if (bunchIndex.Value > 0)
                {
                    totalNumberOfMyReplies = await reviewDb.Comments
                    .Where(c => c.Guid == commentGuid)
                    .Include(c => c.Replies)
                    .SelectMany(c => c.Replies)
                    .Where(rep => rep.WriterGuid == myGuid)
                    .CountAsync();
                }
                else
                {
                    totalNumberOfMyReplies = myRepliesModels.Length;
                }
                int numberOfSkipOthersReplies = (bunchIndex.Value * 10) - totalNumberOfMyReplies;
                if (numberOfSkipOthersReplies < 0) numberOfSkipOthersReplies = 0;

                int numberOfNeededOthersReplies = 10 - myRepliesModels.Length;

                othersRepliesModels = await reviewDb.Comments
                .Where(c => c.Guid == commentGuid)
                .Include(c => c.Replies)
                .ThenInclude(rep => rep.Replies)
                .SelectMany(c => c.Replies)
                .Where(rep => rep.WriterGuid != myGuid)
                .Skip(numberOfSkipOthersReplies)
                .Take(numberOfNeededOthersReplies)
                .Select(rep => new Review_CommentModel()
                {
                    AmIThumbsDown = rep.ThumbsDownBy.Contains(myGuid),
                    AmIThumbsUp = rep.ThumbsUpBy.Contains(myGuid),
                    CreatedAt = rep.CreatedAt,
                    Guid = rep.Guid,
                    IsReply = true,
                    NumberOfReplies = rep.Replies.Count,
                    NumberOfThumbsDowns = rep.ThumbsDownBy.Count,
                    NumberOfThumbsUps = rep.ThumbsUpBy.Count,
                    ReplyToBrief = rep.ReplyTo!.Text.Substring(0, 128),
                    ReplyToGuid = rep.ReplyTo.Guid,
                    ReplyToUsername = commentWriterUsername,
                    Text = rep.Text,
                    WriterGuid = rep.WriterGuid,
                })
                .ToArrayAsync();
            }
        }
        else
        {
            othersRepliesModels = await reviewDb.Comments
            .Where(c => c.Guid == commentGuid)
            .Include(c => c.Replies)
            .ThenInclude(rep => rep.Replies)
            .SelectMany(c => c.Replies)
            .Skip(bunchIndex.Value * 10)
            .Take(10)
            .Select(rep => new Review_CommentModel()
            {
                AmIThumbsDown = false,
                AmIThumbsUp = false,
                CreatedAt = rep.CreatedAt,
                Guid = rep.Guid,
                IsReply = true,
                NumberOfReplies = rep.Replies.Count,
                NumberOfThumbsDowns = rep.ThumbsDownBy.Count,
                NumberOfThumbsUps = rep.ThumbsUpBy.Count,
                ReplyToBrief = rep.ReplyTo!.Text.Substring(0, 128),
                ReplyToGuid = rep.ReplyTo.Guid,
                ReplyToUsername = commentWriterUsername,
                Text = rep.Text,
                WriterGuid = rep.WriterGuid,
            })
            .ToArrayAsync();
        }

        Review_CommentModel[] repliesModels = [.. myRepliesModels, .. othersRepliesModels];
        return Ok(repliesModels);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitNewComment([FromForm] Review_NewCommentFormModel formModel)
    {
        if (ModelState.IsValid)
        {
            Review_ReviewDbModel? parentReviewDbModel = await reviewDb.Reviews
            .FirstOrDefaultAsync(r => r.SubjectGuid == formModel.ParentSubjectGuid);
            if (parentReviewDbModel is null)
            {
                ModelState.AddModelError("parentSubjectGuid", "There's no review with the specified guid!");
                return BadRequest(ModelState);
            }

            string myGuid = await userManager.Users
            .Where(u => u.NormalizedUserName == userManager.NormalizeName(User.Identity!.Name))
            .Select(u => u.UserGuid)
            .FirstAsync();

            Review_CommentDbModel comment = new()
            {
                ParentReview = parentReviewDbModel,
                Text = formModel.Text,
                WriterGuid = myGuid,
            };

            await reviewDb.Comments.AddAsync(comment);
            await reviewDb.SaveChangesAsync();

            Review_CommentModel commentModel = new()
            {
                CreatedAt = comment.CreatedAt,
                Guid = comment.Guid,
                Text = comment.Text,
                WriterGuid = comment.WriterGuid,
            };

            return Ok(commentModel);
        }

        return BadRequest(ModelState);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitNewReply([FromForm] Review_NewReplyFormModel formModel)
    {
        if (ModelState.IsValid)
        {
            Review_CommentDbModel? parentCommentDbModel = await reviewDb.Comments
            .FirstOrDefaultAsync(c => c.Guid == formModel.ParentCommentGuid);
            if (parentCommentDbModel is null)
            {
                ModelState.AddModelError("parentCommentGuid", "There's no comment with the specified guid!");
                return BadRequest(ModelState);
            }

            string parentCommentWriterUserName = await userManager.Users
            .Where(u => u.UserGuid == parentCommentDbModel.WriterGuid)
            .Select(u => u.UserName!)
            .FirstAsync();

            string myGuid = await userManager.Users
            .Where(u => u.NormalizedUserName == userManager.NormalizeName(User.Identity!.Name))
            .Select(u => u.UserGuid)
            .FirstAsync();

            Review_CommentDbModel comment = new()
            {
                ReplyTo = parentCommentDbModel,
                Text = formModel.Text,
                WriterGuid = myGuid,
            };

            await reviewDb.Comments.AddAsync(comment);
            await reviewDb.SaveChangesAsync();

            Review_CommentModel replyModel = new()
            {
                CreatedAt = comment.CreatedAt,
                Guid = comment.Guid,
                Text = comment.Text,
                WriterGuid = comment.WriterGuid,
                IsReply = true,
                ReplyToBrief = parentCommentDbModel.Text.Substring(0, 128),
                ReplyToGuid = parentCommentDbModel.Guid,
                ReplyToUsername = parentCommentWriterUserName,
            };

            return Ok(replyModel);
        }

        return BadRequest(ModelState);
    }





    [HttpPost]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleLike([FromQuery][StringLength(32)] string subjectGuid)
    {
        Review_ReviewDbModel? reviewDbModel = await reviewDb.Reviews
        .FirstOrDefaultAsync(r => r.SubjectGuid == subjectGuid);
        if (reviewDbModel is null)
        {
            ModelState.AddModelError("subjectGuid", "There's no review with the specified guid!");
            return BadRequest(ModelState);
        }

        string myGuid = await userManager.Users
        .Where(u => u.NormalizedUserName == userManager.NormalizeName(User.Identity!.Name))
        .Select(u => u.UserGuid)
        .FirstAsync();

        if (!reviewDbModel.LikedByGuids.Remove(myGuid))
        {
            reviewDbModel.LikedByGuids.Add(myGuid);
        }

        await reviewDb.SaveChangesAsync();

        return Ok(new { numberOfLikes = reviewDbModel.LikedByGuids.Count });
    }

    [HttpPost]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleThumbsUp([FromQuery][StringLength(32)] string commentGuid)
    {
        Review_CommentDbModel? commentDbModel = await reviewDb.Comments
        .FirstOrDefaultAsync(c => c.Guid == commentGuid);
        if (commentDbModel is null)
        {
            ModelState.AddModelError("commentGuid", "There's no comment with the specified guid!");
            return BadRequest(ModelState);
        }

        string myGuid = await userManager.Users
        .Where(u => u.NormalizedUserName == userManager.NormalizeName(User.Identity!.Name))
        .Select(u => u.UserGuid)
        .FirstAsync();

        if (!commentDbModel.ThumbsUpBy.Remove(myGuid))
        {
            commentDbModel.ThumbsUpBy.Add(myGuid);
        }

        await reviewDb.SaveChangesAsync();

        return Ok(new { numberOfThumbUps = commentDbModel.ThumbsUpBy.Count });
    }

    [HttpPost]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleThumbsDown([FromQuery][StringLength(32)] string commentGuid)
    {
        Review_CommentDbModel? commentDbModel = await reviewDb.Comments
        .FirstOrDefaultAsync(c => c.Guid == commentGuid);
        if (commentDbModel is null)
        {
            ModelState.AddModelError("commentGuid", "There's no comment with the specified guid!");
            return BadRequest(ModelState);
        }

        string myGuid = await userManager.Users
        .Where(u => u.NormalizedUserName == userManager.NormalizeName(User.Identity!.Name))
        .Select(u => u.UserGuid)
        .FirstAsync();

        if (!commentDbModel.ThumbsDownBy.Remove(myGuid))
        {
            commentDbModel.ThumbsDownBy.Add(myGuid);
        }

        await reviewDb.SaveChangesAsync();

        return Ok(new { numberOfThumbDowns = commentDbModel.ThumbsDownBy.Count });
    }





    [HttpGet]
    public async Task<IActionResult> GetLikedUserList([FromQuery][StringLength(32)] string subjectGuid,
    [FromQuery] int bunchIndex, [FromQuery][StringLength(30)] string? filter)
    {

    }

    [HttpGet]
    public async Task<IActionResult> GetThumbsUpUserList([FromQuery][StringLength(32)] string commentGuid,
    [FromQuery] int bunchIndex, [FromQuery][StringLength(30)] string? filter)
    {

    }

    [HttpGet]
    public async Task<IActionResult> GetThumbsDownUserList([FromQuery][StringLength(32)] string commentGuid,
    [FromQuery] int bunchIndex, [FromQuery][StringLength(30)] string? filter)
    {

    }

}