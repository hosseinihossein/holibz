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
    public async Task<IActionResult> GetReviewModel([FromQuery][StringLength(32)] string subjectGuid,
    [FromQuery][StringLength(32)] string? commentGuid)
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

        Review_CommentModel? requestedCommentModel = null;
        if (commentGuid is not null)
        {
            requestedCommentModel = await reviewDb.Comments
            .Where(c => c.Guid == commentGuid)
            .Include(c => c.ReplyTo)
            .Include(c => c.Replies)
            .Select(c => new Review_CommentModel()
            {
                AmIThumbsDown = myGuid != null && c.ThumbsDownBy.Contains(myGuid),
                AmIThumbsUp = myGuid != null && c.ThumbsUpBy.Contains(myGuid),
                CreatedAt = c.CreatedAt,
                Guid = c.Guid,
                IsReply = c.ReplyTo != null,
                NumberOfReplies = c.Replies.Count,
                NumberOfThumbsDowns = c.ThumbsDownBy.Count,
                NumberOfThumbsUps = c.ThumbsUpBy.Count,
                ReplyToBrief = c.ReplyTo == null ? "" : c.ReplyTo.Text.Substring(0, c.ReplyTo.Text.Length > 128 ? 128 : c.ReplyTo.Text.Length),
                ReplyToGuid = c.ReplyTo == null ? "" : c.ReplyTo.Guid,
                ReplyToUsername = c.ReplyTo == null ? "" : c.ReplyTo.WriterGuid,//UserGuid instead of UserName
                Text = c.Text,
                WriterGuid = c.WriterGuid,
            })
            .AsSplitQuery()
            .FirstOrDefaultAsync();

            if (requestedCommentModel is not null && requestedCommentModel.IsReply)
            {
                string? replyToUserName = await userManager.Users
                .Where(u => u.UserGuid == requestedCommentModel.ReplyToUsername)//use the UserGuid got instead of UserName
                .Select(u => u.UserName)
                .FirstOrDefaultAsync();
                requestedCommentModel.ReplyToUsername = replyToUserName ?? "";// replace UserName by UserGuid
            }
        }

        int commentTakeNumber = 10;
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
            .Take(commentTakeNumber)
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

            if (myCommentsModels.Length < commentTakeNumber)
            {
                int numberOfNeededOthersComments = commentTakeNumber - myCommentsModels.Length;

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
            .Take(commentTakeNumber)
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

        Review_CommentModel[] comments = [];
        if (requestedCommentModel is not null)
        {
            comments = [requestedCommentModel, .. myCommentsModels, .. othersCommentsModels];
        }
        else
        {
            comments = [.. myCommentsModels, .. othersCommentsModels];
        }
        Review_ReviewModel reviewModel = new()
        {
            AmILiked = myGuid is not null && reviewDbModel.LikedByGuids.Contains(myGuid),
            Comments = comments,
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
                //.Include(r => r.Comments)
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
                //.AsSplitQuery()
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
                    //.Include(r => r.Comments)
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
                    //.AsSplitQuery()
                    .ToArrayAsync();
                }
            }
            else
            {
                othersCommentsModels = await reviewDb.Reviews
                .Where(r => r.SubjectGuid == subjectGuid)
                //.Include(r => r.Comments)
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
                //.AsSplitQuery()
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
                //.Include(r => r.Comments)
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
                    AmIThumbsDown = myGuid != null && c.ThumbsDownBy.Contains(myGuid),
                    AmIThumbsUp = myGuid != null && c.ThumbsUpBy.Contains(myGuid),
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
                //.AsSplitQuery()
                .ToArrayAsync();

                return Ok(commentsModels);
            }

            if (orderBy == "Oldest")
            {
                Review_CommentModel[] commentsModels = await reviewDb.Reviews
                .Where(r => r.SubjectGuid == subjectGuid)
                //.Include(r => r.Comments)
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
                    AmIThumbsDown = myGuid != null && c.ThumbsDownBy.Contains(myGuid),
                    AmIThumbsUp = myGuid != null && c.ThumbsUpBy.Contains(myGuid),
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
                //.AsSplitQuery()
                .ToArrayAsync();

                return Ok(commentsModels);
            }

            if (orderBy == "Most Agreed")
            {
                Review_CommentModel[] commentsModels = await reviewDb.Reviews
                .Where(r => r.SubjectGuid == subjectGuid)
                //.Include(r => r.Comments)
                //.ThenInclude(c => c.ReplyTo)
                .Include(r => r.Comments)
                .ThenInclude(c => c.Replies)
                .SelectMany(r => r.Comments)
                //.Where(c => c.ReplyTo == null)
                .OrderByDescending(c => c._thumbsUpBy.Length)
                .Skip(pageIndex.Value * pageSize.Value)
                .Take(pageSize.Value)
                .Select(c => new Review_CommentModel()
                {
                    AmIThumbsDown = myGuid != null && c.ThumbsDownBy.Contains(myGuid),
                    AmIThumbsUp = myGuid != null && c.ThumbsUpBy.Contains(myGuid),
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
                //.AsSplitQuery()
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
                ReplyToBrief = rep.ReplyTo!.Text.Substring(0, rep.ReplyTo.Text.Length > 128 ? 128 : rep.ReplyTo.Text.Length),
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
                    ReplyToBrief = rep.ReplyTo!.Text.Substring(0, rep.ReplyTo.Text.Length > 128 ? 128 : rep.ReplyTo.Text.Length),
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
                ReplyToBrief = rep.ReplyTo!.Text.Substring(0, rep.ReplyTo.Text.Length > 128 ? 128 : rep.ReplyTo.Text.Length),
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
    public async Task<IActionResult> SubmitNewComment([FromForm] Review_NewCommentFormModel formModel/*,
    [FromServices] Review_Process reviewProcess*/)
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

            //seed
            //_ = reviewProcess.Update_CommentSeed(comment.Guid, reviewDb);

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
    public async Task<IActionResult> SubmitNewReply([FromForm] Review_NewReplyFormModel formModel/*,
    [FromServices] Review_Process reviewProcess*/)
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

            //seed
            //_ = reviewProcess.Update_CommentSeed(comment.Guid, reviewDb);

            int briefLength = parentCommentDbModel.Text.Length > 128 ? 128 : parentCommentDbModel.Text.Length;
            Review_CommentModel replyModel = new()
            {
                CreatedAt = comment.CreatedAt,
                Guid = comment.Guid,
                Text = comment.Text,
                WriterGuid = comment.WriterGuid,
                IsReply = true,
                ReplyToBrief = parentCommentDbModel.Text[..briefLength],
                ReplyToGuid = parentCommentDbModel.Guid,
                ReplyToUsername = parentCommentWriterUserName,
            };

            return Ok(replyModel);
        }

        return BadRequest(ModelState);
    }

    [HttpDelete]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteComment([FromQuery][StringLength(32)] string commentGuid/*,
    [FromServices] Review_Process reviewProcess*/)
    {
        Review_CommentDbModel? commentDbModel = await reviewDb.Comments
        .FirstOrDefaultAsync(c => c.Guid == commentGuid);
        if (commentDbModel is null)
        {
            ModelState.AddModelError("commentGuid", "There's not comment with the specified guid!");
            return BadRequest(ModelState);
        }

        string myGuid = await userManager.Users
        .Where(u => u.NormalizedUserName == userManager.NormalizeName(User.Identity!.Name))
        .Select(u => u.UserGuid)
        .FirstAsync();

        if (commentDbModel.WriterGuid == myGuid)
        {
            reviewDb.Comments.Remove(commentDbModel);
            await reviewDb.SaveChangesAsync();

            //seed
            //reviewProcess.Delete_CommentSeed(commentDbModel.Guid);

            return Ok(new { success = true });
        }

        ModelState.AddModelError("Authorization", "Only the writer of the comment can delete the comment!");
        return BadRequest(ModelState);
    }





    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleLike([FromQuery][StringLength(32)] string subjectGuid/*,
    [FromServices] Review_Process reviewProcess*/)
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

        List<string> temp = new(reviewDbModel.LikedByGuids);
        if (reviewDbModel.LikedByGuids.Contains(myGuid))
        {
            temp.Remove(myGuid);
        }
        else
        {
            temp.Add(myGuid);
        }
        reviewDbModel.LikedByGuids = temp;

        await reviewDb.SaveChangesAsync();

        //seed
        //_ = reviewProcess.Update_ReviewSeed(reviewDbModel.SubjectGuid, reviewDb);

        return Ok(new { numberOfLikes = reviewDbModel.LikedByGuids.Count });
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleThumbsUp([FromQuery][StringLength(32)] string commentGuid/*,
    [FromServices] Review_Process reviewProcess*/)
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

        List<string> tempUp = new(commentDbModel.ThumbsUpBy);
        List<string> tempDown = new(commentDbModel.ThumbsDownBy);
        if (commentDbModel.ThumbsUpBy.Contains(myGuid))
        {
            tempUp.Remove(myGuid);
        }
        else
        {
            tempUp.Add(myGuid);
            tempDown.Remove(myGuid);
        }
        commentDbModel.ThumbsUpBy = tempUp;
        commentDbModel.ThumbsDownBy = tempDown;

        await reviewDb.SaveChangesAsync();

        //seed
        //_ = reviewProcess.Update_CommentSeed(commentDbModel.Guid, reviewDb);

        return Ok(new { numberOfThumbUps = commentDbModel.ThumbsUpBy.Count });
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleThumbsDown([FromQuery][StringLength(32)] string commentGuid/*,
    [FromServices] Review_Process reviewProcess*/)
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

        List<string> tempUp = new(commentDbModel.ThumbsUpBy);
        List<string> tempDown = new(commentDbModel.ThumbsDownBy);
        if (commentDbModel.ThumbsDownBy.Contains(myGuid))
        {
            tempDown.Remove(myGuid);
        }
        else
        {
            tempDown.Add(myGuid);
            tempUp.Remove(myGuid);
        }
        commentDbModel.ThumbsUpBy = tempUp;
        commentDbModel.ThumbsDownBy = tempDown;

        await reviewDb.SaveChangesAsync();

        //seed
        //_ = reviewProcess.Update_CommentSeed(commentDbModel.Guid, reviewDb);

        return Ok(new { numberOfThumbDowns = commentDbModel.ThumbsDownBy.Count });
    }





    [HttpGet]
    public async Task<IActionResult> GetLikesUserList([FromQuery][StringLength(32)] string subjectGuid,
    [FromQuery] int? bunchIndex, [FromQuery][StringLength(30)] string? filter,
    [FromServices] Library_DbContext libraryDb)
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
            .Where(u => u.NormalizedUserName == userManager.NormalizeName(User.Identity!.Name))
            .Select(u => u.UserGuid)
            .FirstAsync();
        }

        bunchIndex ??= 0;
        int bunchSize = 10;

        if (string.IsNullOrWhiteSpace(filter))
        {
            filter = null;
        }
        else
        {
            filter = filter.Trim();
        }

        List<string> likedByGuids = reviewDbModel.LikedByGuids;
        if (filter is not null)
        {
            likedByGuids = await userManager.Users
            .Where(u => reviewDbModel.LikedByGuids.Contains(u.UserGuid) &&
            u.NormalizedUserName!.Contains(userManager.NormalizeName(filter)))
            .Select(u => u.UserGuid)
            .ToListAsync();
        }

        List<string> myFollowingsLikesGuids = [];
        List<string> othersLikesGuids = [];
        if (myGuid is not null)
        {
            List<string> myFollowingsGuids = await libraryDb.Owners
            .Where(o => o.Guid == myGuid)
            .Include(o => o.Followings)
            .SelectMany(o => o.Followings)
            .Select(f => f.Guid)
            .ToListAsync();

            myFollowingsLikesGuids = likedByGuids
            .Intersect(myFollowingsGuids)
            .Skip(bunchIndex.Value * bunchSize)
            .Take(bunchSize)
            .ToList();

            if (myFollowingsLikesGuids.Count < bunchSize)
            {
                int totalNumberOfMyFollowingsLikes = likedByGuids
                .Intersect(myFollowingsGuids)
                .Count();
                int numberOfSkipOthersLikes = (bunchIndex.Value * bunchSize) - totalNumberOfMyFollowingsLikes;
                if (numberOfSkipOthersLikes < 0) numberOfSkipOthersLikes = 0;

                int numberOfNeededOthersLikes = bunchSize - myFollowingsLikesGuids.Count;

                othersLikesGuids = likedByGuids
                .Where(lg => !myFollowingsLikesGuids.Contains(lg))
                .Skip(numberOfSkipOthersLikes)
                .Take(numberOfNeededOthersLikes)
                .ToList();
            }

        }
        else
        {
            othersLikesGuids = likedByGuids
            .Skip(bunchIndex.Value * bunchSize)
            .Take(bunchSize)
            .ToList();
        }

        List<string> likesUserGuids = [.. myFollowingsLikesGuids, .. othersLikesGuids];

        Library_OwnerModel[] likesOwnerModels = await userManager.Users
        .Where(u => likesUserGuids.Contains(u.UserGuid))
        .Select(u => new Library_OwnerModel()
        {
            Guid = u.UserGuid,
            HasImage = u.HasImage,
            IntegrityVersion = u.IntegrityVersion,
            Username = u.UserName!,
        })
        .ToArrayAsync();

        return Ok(likesOwnerModels);
    }

    [HttpGet]
    public async Task<IActionResult> GetThumbsUpUserList([FromQuery][StringLength(32)] string commentGuid,
    [FromQuery] int? bunchIndex, [FromQuery][StringLength(30)] string? filter,
    [FromServices] Library_DbContext libraryDb)
    {
        Review_CommentDbModel? commentDbModel = await reviewDb.Comments
        .FirstOrDefaultAsync(c => c.Guid == commentGuid);
        if (commentDbModel is null)
        {
            ModelState.AddModelError("commentGuid", "There's no comment with the specified guid!");
            return BadRequest(ModelState);
        }

        string? myGuid = null;
        if ((User.Identity?.IsAuthenticated ?? false) && User.Identity.Name is not null)
        {
            myGuid = await userManager.Users
            .Where(u => u.NormalizedUserName == userManager.NormalizeName(User.Identity!.Name))
            .Select(u => u.UserGuid)
            .FirstAsync();
        }

        bunchIndex ??= 0;
        int bunchSize = 10;

        if (string.IsNullOrWhiteSpace(filter))
        {
            filter = null;
        }
        else
        {
            filter = filter.Trim();
        }

        List<string> thumbsUpByGuids = commentDbModel.ThumbsUpBy;
        if (filter is not null)
        {
            thumbsUpByGuids = await userManager.Users
            .Where(u => commentDbModel.ThumbsUpBy.Contains(u.UserGuid) &&
            u.NormalizedUserName!.Contains(userManager.NormalizeName(filter)))
            .Select(u => u.UserGuid)
            .ToListAsync();
        }

        List<string> myFollowingsThumbsUpsGuids = [];
        List<string> othersThumbsUpsGuids = [];
        if (myGuid is not null)
        {
            List<string> myFollowingsGuids = await libraryDb.Owners
            .Where(o => o.Guid == myGuid)
            .Include(o => o.Followings)
            .SelectMany(o => o.Followings)
            .Select(f => f.Guid)
            .ToListAsync();

            myFollowingsThumbsUpsGuids = thumbsUpByGuids
            .Intersect(myFollowingsGuids)
            .Skip(bunchIndex.Value * bunchSize)
            .Take(bunchSize)
            .ToList();

            if (myFollowingsThumbsUpsGuids.Count < bunchSize)
            {
                int totalNumberOfMyFollowingsThumbsUps = thumbsUpByGuids
                .Intersect(myFollowingsGuids)
                .Count();
                int numberOfSkipOthersThumbsUps = (bunchIndex.Value * bunchSize) - totalNumberOfMyFollowingsThumbsUps;
                if (numberOfSkipOthersThumbsUps < 0) numberOfSkipOthersThumbsUps = 0;

                int numberOfNeededOthersThumbsUps = bunchSize - myFollowingsThumbsUpsGuids.Count;

                othersThumbsUpsGuids = thumbsUpByGuids
                .Where(lg => !myFollowingsThumbsUpsGuids.Contains(lg))
                .Skip(numberOfSkipOthersThumbsUps)
                .Take(numberOfNeededOthersThumbsUps)
                .ToList();
            }
        }
        else
        {
            othersThumbsUpsGuids = thumbsUpByGuids
            .Skip(bunchIndex.Value * bunchSize)
            .Take(bunchSize)
            .ToList();
        }

        List<string> thumbsUpsUserGuids = [.. myFollowingsThumbsUpsGuids, .. othersThumbsUpsGuids];

        Library_OwnerModel[] thumbsUpsOwnerModels = await userManager.Users
        .Where(u => thumbsUpsUserGuids.Contains(u.UserGuid))
        .Select(u => new Library_OwnerModel()
        {
            Guid = u.UserGuid,
            HasImage = u.HasImage,
            IntegrityVersion = u.IntegrityVersion,
            Username = u.UserName!,
        })
        .ToArrayAsync();

        return Ok(thumbsUpsOwnerModels);
    }

    [HttpGet]
    public async Task<IActionResult> GetThumbsDownUserList([FromQuery][StringLength(32)] string commentGuid,
    [FromQuery] int? bunchIndex, [FromQuery][StringLength(30)] string? filter,
    [FromServices] Library_DbContext libraryDb)
    {
        Review_CommentDbModel? commentDbModel = await reviewDb.Comments
        .FirstOrDefaultAsync(c => c.Guid == commentGuid);
        if (commentDbModel is null)
        {
            ModelState.AddModelError("commentGuid", "There's no comment with the specified guid!");
            return BadRequest(ModelState);
        }

        string? myGuid = null;
        if ((User.Identity?.IsAuthenticated ?? false) && User.Identity.Name is not null)
        {
            myGuid = await userManager.Users
            .Where(u => u.NormalizedUserName == userManager.NormalizeName(User.Identity!.Name))
            .Select(u => u.UserGuid)
            .FirstAsync();
        }

        bunchIndex ??= 0;
        int bunchSize = 10;

        if (string.IsNullOrWhiteSpace(filter))
        {
            filter = null;
        }
        else
        {
            filter = filter.Trim();
        }

        List<string> thumbsDownByGuids = commentDbModel.ThumbsDownBy;
        if (filter is not null)
        {
            thumbsDownByGuids = await userManager.Users
            .Where(u => commentDbModel.ThumbsDownBy.Contains(u.UserGuid) &&
            u.NormalizedUserName!.Contains(userManager.NormalizeName(filter)))
            .Select(u => u.UserGuid)
            .ToListAsync();
        }

        List<string> myFollowingsThumbsDownsGuids = [];
        List<string> othersThumbsDownsGuids = [];
        if (myGuid is not null)
        {
            List<string> myFollowingsGuids = await libraryDb.Owners
            .Where(o => o.Guid == myGuid)
            .Include(o => o.Followings)
            .SelectMany(o => o.Followings)
            .Select(f => f.Guid)
            .ToListAsync();

            myFollowingsThumbsDownsGuids = thumbsDownByGuids
            .Intersect(myFollowingsGuids)
            .Skip(bunchIndex.Value * bunchSize)
            .Take(bunchSize)
            .ToList();

            if (myFollowingsThumbsDownsGuids.Count < bunchSize)
            {
                int totalNumberOfMyFollowingsThumbsDowns = thumbsDownByGuids
                .Intersect(myFollowingsGuids)
                .Count();
                int numberOfSkipOthersThumbsDowns = (bunchIndex.Value * bunchSize) - totalNumberOfMyFollowingsThumbsDowns;
                if (numberOfSkipOthersThumbsDowns < 0) numberOfSkipOthersThumbsDowns = 0;

                int numberOfNeededOthersThumbsDowns = bunchSize - myFollowingsThumbsDownsGuids.Count;

                othersThumbsDownsGuids = thumbsDownByGuids
                .Where(lg => !myFollowingsThumbsDownsGuids.Contains(lg))
                .Skip(numberOfSkipOthersThumbsDowns)
                .Take(numberOfNeededOthersThumbsDowns)
                .ToList();
            }
        }
        else
        {
            othersThumbsDownsGuids = thumbsDownByGuids
            .Skip(bunchIndex.Value * bunchSize)
            .Take(bunchSize)
            .ToList();
        }

        List<string> thumbsDownsUserGuids = [.. myFollowingsThumbsDownsGuids, .. othersThumbsDownsGuids];

        Library_OwnerModel[] thumbsUpsOwnerModels = await userManager.Users
        .Where(u => thumbsDownsUserGuids.Contains(u.UserGuid))
        .Select(u => new Library_OwnerModel()
        {
            Guid = u.UserGuid,
            HasImage = u.HasImage,
            IntegrityVersion = u.IntegrityVersion,
            Username = u.UserName!,
        })
        .ToArrayAsync();

        return Ok(thumbsUpsOwnerModels);
    }

}