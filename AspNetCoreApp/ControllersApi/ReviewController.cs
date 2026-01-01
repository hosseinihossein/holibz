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
        if (!Guid.TryParseExact(userGuid, "N", out Guid userGuid_Guid))
        {
            ModelState.AddModelError("Parse Guid", "Couldn't parse the specified guid!");
            return BadRequest(ModelState);
        }

        int userTotalNumberOfLikes = await reviewDb.Users
        .Where(u => u.Guid == userGuid_Guid)
        .Include(u => u.GotReviews)
        .ThenInclude(r => r.LikedBy)
        .SelectMany(u => u.GotReviews)
        .SelectMany(r => r.LikedBy)
        .CountAsync();

        return Ok(new { totalNumberOfLikes = userTotalNumberOfLikes });
    }





    [HttpGet]
    public async Task<IActionResult> GetReviewModel([FromQuery][StringLength(32)] string subjectGuid,
    [FromQuery][StringLength(32)] string? commentGuid)
    {
        if (!Guid.TryParseExact(subjectGuid, "N", out Guid subjectGuid_Guid))
        {
            ModelState.AddModelError("Parse Guid", "Couldn't parse the specified guid!");
            return BadRequest(ModelState);
        }

        Guid? myGuid = null;
        if ((User.Identity?.IsAuthenticated ?? false) && User.Identity.Name is not null)
        {
            myGuid = await userManager.Users
            .Where(u => u.NormalizedUserName == userManager.NormalizeName(User.Identity.Name))
            .Select(u => u.UserGuid)
            .FirstAsync();
        }

        var reviewDbInfo = await reviewDb.Reviews
        .Where(r => r.SubjectGuid == subjectGuid_Guid)
        .Select(r => new
        {
            r.Id,
            totalLikes = r.LikedBy.Count(),
            iLiked = myGuid != null && r.LikedBy.Any(ur => ur.User.Guid == myGuid),
            totalComments = r.Comments.Count(c => c.ReplyTo == null),
        })
        .FirstOrDefaultAsync();

        if (reviewDbInfo is null)
        {
            ModelState.AddModelError("subjectGuid", "There's no review with the specified guid!");
            return BadRequest(ModelState);
        }

        Review_Comment_ViewModel[] requestedCommentWithParentsModels = [];
        Guid? mainParentCommentGuid = null;
        bool hasRequestedCommentGuid = Guid.TryParseExact(commentGuid, "N", out Guid commentGuid_Guid);
        while (hasRequestedCommentGuid)
        {
            Review_Comment_ViewModel? requestedCommentModel =
            await GetRequestedComment(commentGuid_Guid, myGuid);

            if (requestedCommentModel is not null)
            {
                requestedCommentWithParentsModels = [requestedCommentModel, .. requestedCommentWithParentsModels];
                if (requestedCommentModel.IsReply && requestedCommentModel.ReplyToGuid.HasValue)
                {
                    commentGuid_Guid = requestedCommentModel.ReplyToGuid.Value;
                }
                else
                {
                    hasRequestedCommentGuid = false;//breaks the loop
                    mainParentCommentGuid = requestedCommentModel.Guid;
                    break;
                }
            }
            else
            {
                hasRequestedCommentGuid = false;//breaks the loop
                break;
            }
        }

        int commentTakeNumber = 10;
        Review_Comment_ViewModel[] myCommentsModels = [];
        Review_Comment_ViewModel[] mutualCommentsModels = [];
        Review_Comment_ViewModel[] othersCommentsModels = [];
        if (myGuid is not null)
        {
            myCommentsModels = await reviewDb.Reviews
            .Where(r => r.SubjectGuid == subjectGuid_Guid)
            .SelectMany(r => r.Comments)
            .Where(c => c.ReplyTo == null &&
                c.Writer.Guid == myGuid &&
                (mainParentCommentGuid == null || c.Guid != mainParentCommentGuid)
            )
            .OrderBy(c => c.CreatedAt)
            .Take(commentTakeNumber)
            .Select(c => new Review_Comment_ViewModel()
            {
                AmIThumbsDown = c.ThumbsDownsBy.Any(udn => udn.User.Guid == myGuid),
                AmIThumbsUp = c.ThumbsUpsBy.Any(uup => uup.User.Guid == myGuid),
                CreatedAt = c.CreatedAt,
                Guid = c.Guid,
                IsReply = false,
                NumberOfReplies = c.Replies.Count,
                NumberOfThumbsDowns = c.ThumbsDownsBy.Count,
                NumberOfThumbsUps = c.ThumbsUpsBy.Count,
                Text = c.Text,
                WriterGuid = c.Writer.Guid,
            })
            .ToArrayAsync();

            if (myCommentsModels.Length < commentTakeNumber)
            {
                int numberOfNeededMutualComments = commentTakeNumber - myCommentsModels.Length;

                mutualCommentsModels = await reviewDb.Reviews
                .Where(r => r.SubjectGuid == subjectGuid_Guid)
                .SelectMany(r => r.Comments)
                .Where(c => c.ReplyTo == null &&
                    c.Writer.Followers.Any(ff => ff.Follower.Guid == myGuid) &&
                    (mainParentCommentGuid == null || c.Guid != mainParentCommentGuid)
                )
                .OrderBy(c => c.CreatedAt)
                .Take(numberOfNeededMutualComments)
                .Select(c => new Review_Comment_ViewModel()
                {
                    AmIThumbsDown = c.ThumbsDownsBy.Any(udn => udn.User.Guid == myGuid),
                    AmIThumbsUp = c.ThumbsUpsBy.Any(uup => uup.User.Guid == myGuid),
                    CreatedAt = c.CreatedAt,
                    Guid = c.Guid,
                    IsReply = false,
                    NumberOfReplies = c.Replies.Count,
                    NumberOfThumbsDowns = c.ThumbsDownsBy.Count,
                    NumberOfThumbsUps = c.ThumbsUpsBy.Count,
                    Text = c.Text,
                    WriterGuid = c.Writer.Guid,
                })
                .ToArrayAsync();
            }

            if ((myCommentsModels.Length + mutualCommentsModels.Length) < commentTakeNumber)
            {
                int numberOfNeededOthersComments = commentTakeNumber -
                (myCommentsModels.Length + mutualCommentsModels.Length);

                othersCommentsModels = await reviewDb.Reviews
                .Where(r => r.SubjectGuid == subjectGuid_Guid)
                .SelectMany(r => r.Comments)
                .Where(c => c.ReplyTo == null &&
                    !c.Writer.Followers.Any(ff => ff.Follower.Guid == myGuid) &&
                    c.Writer.Guid != myGuid &&
                    (mainParentCommentGuid == null || c.Guid != mainParentCommentGuid)
                )
                .OrderBy(c => c.CreatedAt)
                .Take(numberOfNeededOthersComments)
                .Select(c => new Review_Comment_ViewModel()
                {
                    AmIThumbsDown = c.ThumbsDownsBy.Any(udn => udn.User.Guid == myGuid),
                    AmIThumbsUp = c.ThumbsUpsBy.Any(uup => uup.User.Guid == myGuid),
                    CreatedAt = c.CreatedAt,
                    Guid = c.Guid,
                    IsReply = false,
                    NumberOfReplies = c.Replies.Count,
                    NumberOfThumbsDowns = c.ThumbsDownsBy.Count,
                    NumberOfThumbsUps = c.ThumbsUpsBy.Count,
                    Text = c.Text,
                    WriterGuid = c.Writer.Guid,
                })
                .ToArrayAsync();
            }
        }
        else
        {
            othersCommentsModels = await reviewDb.Reviews
            .Where(r => r.SubjectGuid == subjectGuid_Guid)
            .SelectMany(r => r.Comments)
            .Where(c => c.ReplyTo == null &&
                (mainParentCommentGuid == null || c.Guid != mainParentCommentGuid)
            )
            .OrderBy(c => c.CreatedAt)
            .Take(commentTakeNumber)
            .Select(c => new Review_Comment_ViewModel()
            {
                AmIThumbsDown = false,
                AmIThumbsUp = false,
                CreatedAt = c.CreatedAt,
                Guid = c.Guid,
                IsReply = false,
                NumberOfReplies = c.Replies.Count,
                NumberOfThumbsDowns = c.ThumbsDownsBy.Count,
                NumberOfThumbsUps = c.ThumbsUpsBy.Count,
                Text = c.Text,
                WriterGuid = c.Writer.Guid,
            })
            .ToArrayAsync();
        }

        Review_Comment_ViewModel[] comments =
        [.. requestedCommentWithParentsModels, .. myCommentsModels,
        .. mutualCommentsModels, .. othersCommentsModels];

        Review_Review_ViewModel reviewModel = new()
        {
            AmILiked = reviewDbInfo.iLiked,
            Comments = comments,
            NumberOfLikes = reviewDbInfo.totalLikes,
            TotalNumberOfComments = reviewDbInfo.totalComments,
        };

        return Ok(reviewModel);
    }
    private async Task<Review_Comment_ViewModel?> GetRequestedComment(Guid commentGuid,
    Guid? myGuid)
    {
        Review_Comment_ViewModel? requestedCommentModel = await reviewDb.Comments
        .Where(c => c.Guid == commentGuid)
        .Select(c => new Review_Comment_ViewModel()
        {
            AmIThumbsDown = myGuid != null && c.ThumbsDownsBy.Any(udn => udn.User.Guid == myGuid),
            AmIThumbsUp = myGuid != null && c.ThumbsUpsBy.Any(uup => uup.User.Guid == myGuid),
            CreatedAt = c.CreatedAt,
            Guid = c.Guid,
            IsReply = c.ReplyTo != null,
            NumberOfReplies = c.Replies.Count,
            NumberOfThumbsDowns = c.ThumbsDownsBy.Count,
            NumberOfThumbsUps = c.ThumbsUpsBy.Count,
            ReplyToBrief = c.ReplyTo == null ? "" :
                c.ReplyTo.Text.Substring(0, c.ReplyTo.Text.Length > 128 ? 128 : c.ReplyTo.Text.Length),
            ReplyToGuid = c.ReplyTo == null ? null : c.ReplyTo.Guid,
            ReplyToUsername = c.ReplyTo == null ? null : c.ReplyTo.Writer.NormalizedUserName,//UserGuid instead of UserName
            Text = c.Text,
            WriterGuid = c.Writer.Guid,
        })
        .FirstOrDefaultAsync();

        if (requestedCommentModel is not null && requestedCommentModel.IsReply &&
        requestedCommentModel.ReplyToUsername is not null)
        {
            /*string? replyToUserName = await userManager.Users
            .Where(u => u.UserGuid == requestedCommentModel.ReplyToUsername)//use the UserGuid got instead of UserName
            .Select(u => u.UserName)
            .FirstOrDefaultAsync();
            requestedCommentModel.ReplyToUsername = replyToUserName ?? "";*/// replace UserName by UserGuid
            requestedCommentModel.ReplyToUsername = requestedCommentModel.ReplyToUsername.ToLower();
        }

        return requestedCommentModel;
    }





    [HttpGet]
    public async Task<IActionResult> GetComments([FromQuery][StringLength(32)] string subjectGuid,
    [FromQuery][StringLength(20)] string? orderBy, [FromQuery] int? pageIndex, [FromQuery] int? pageSize)
    {
        if (!Guid.TryParseExact(subjectGuid, "N", out Guid subjectGuid_Guid))
        {
            ModelState.AddModelError("Parse Guid", "Couldn't parse the specified guid!");
            return BadRequest(ModelState);
        }

        var reviewDbInfo = await reviewDb.Reviews
        .Where(r => r.SubjectGuid == subjectGuid_Guid)
        .Select(r => new
        {
            r.Id,
            totalComments = r.Comments.Count(c => c.ReplyTo == null),
        })
        .FirstOrDefaultAsync();

        if (reviewDbInfo is null)
        {
            ModelState.AddModelError("subjectGuid", "There's no review with the specified guid!");
            return BadRequest(ModelState);
        }

        pageIndex ??= 0;
        pageSize ??= 10;

        if (pageIndex.Value > 0 && (pageIndex.Value * pageSize) >= reviewDbInfo.totalComments)
        {
            return Ok(Array.Empty<Review_Comment_ViewModel>());
        }

        Guid? myGuid = null;
        if ((User.Identity?.IsAuthenticated ?? false) && User.Identity.Name is not null)
        {
            myGuid = await userManager.Users
            .Where(u => u.NormalizedUserName == userManager.NormalizeName(User.Identity.Name))
            .Select(u => u.UserGuid)
            .FirstAsync();
        }

        if (orderBy is null)
        {
            Review_Comment_ViewModel[] myCommentsModels = [];
            Review_Comment_ViewModel[] mutualCommentsModels = [];
            Review_Comment_ViewModel[] othersCommentsModels = [];
            if (myGuid is not null)
            {
                myCommentsModels = await reviewDb.Reviews
                .Where(r => r.SubjectGuid == subjectGuid_Guid)
                .SelectMany(r => r.Comments)
                .Where(c => c.ReplyTo == null &&
                    c.Writer.Guid == myGuid
                )
                .OrderBy(c => c.CreatedAt)
                .Skip(pageIndex.Value * pageSize.Value)
                .Take(pageSize.Value)
                .Select(c => new Review_Comment_ViewModel()
                {
                    AmIThumbsDown = c.ThumbsDownsBy.Any(udn => udn.User.Guid == myGuid),
                    AmIThumbsUp = c.ThumbsUpsBy.Any(uup => uup.User.Guid == myGuid),
                    CreatedAt = c.CreatedAt,
                    Guid = c.Guid,
                    IsReply = false,
                    NumberOfReplies = c.Replies.Count,
                    NumberOfThumbsDowns = c.ThumbsDownsBy.Count,
                    NumberOfThumbsUps = c.ThumbsUpsBy.Count,
                    Text = c.Text,
                    WriterGuid = c.Writer.Guid,
                })
                .ToArrayAsync();

                if (myCommentsModels.Length < pageSize.Value)
                {
                    int totalNumberOfMyComments;
                    if (pageIndex.Value > 0)
                    {
                        totalNumberOfMyComments = await reviewDb.Reviews
                        .Where(r => r.SubjectGuid == subjectGuid_Guid)
                        .SelectMany(r => r.Comments)
                        .CountAsync(c => c.ReplyTo == null &&
                            c.Writer.Guid == myGuid
                        );
                    }
                    else
                    {
                        totalNumberOfMyComments = myCommentsModels.Length;
                    }

                    int numberOfSkipMutualComments = (pageIndex.Value * pageSize.Value) - totalNumberOfMyComments;
                    if (numberOfSkipMutualComments < 0) numberOfSkipMutualComments = 0;

                    int numberOfNeededMutualComments = pageSize.Value - myCommentsModels.Length;

                    mutualCommentsModels = await reviewDb.Reviews
                    .Where(r => r.SubjectGuid == subjectGuid_Guid)
                    .SelectMany(r => r.Comments)
                    .Where(c => c.ReplyTo == null &&
                        c.Writer.Followers.Any(ff => ff.Follower.Guid == myGuid)
                    )
                    .OrderBy(c => c.CreatedAt)
                    .Skip(numberOfSkipMutualComments)
                    .Take(numberOfNeededMutualComments)
                    .Select(c => new Review_Comment_ViewModel()
                    {
                        AmIThumbsDown = c.ThumbsDownsBy.Any(udn => udn.User.Guid == myGuid),
                        AmIThumbsUp = c.ThumbsUpsBy.Any(uup => uup.User.Guid == myGuid),
                        CreatedAt = c.CreatedAt,
                        Guid = c.Guid,
                        IsReply = false,
                        NumberOfReplies = c.Replies.Count,
                        NumberOfThumbsDowns = c.ThumbsDownsBy.Count,
                        NumberOfThumbsUps = c.ThumbsUpsBy.Count,
                        Text = c.Text,
                        WriterGuid = c.Writer.Guid,
                    })
                    .ToArrayAsync();
                }

                if ((myCommentsModels.Length + mutualCommentsModels.Length) < pageSize.Value)
                {
                    int totalNumberOfMeAndMutualComments;
                    if (pageIndex.Value > 0)
                    {
                        totalNumberOfMeAndMutualComments = await reviewDb.Reviews
                        .Where(r => r.SubjectGuid == subjectGuid_Guid)
                        .SelectMany(r => r.Comments)
                        .CountAsync(c => c.ReplyTo == null &&
                            (c.Writer.Guid == myGuid ||
                            c.Writer.Followers.Any(ff => ff.Follower.Guid == myGuid))
                        );
                    }
                    else
                    {
                        totalNumberOfMeAndMutualComments = myCommentsModels.Length + mutualCommentsModels.Length;
                    }

                    int numberOfSkipOtherComments = (pageIndex.Value * pageSize.Value) - totalNumberOfMeAndMutualComments;
                    if (numberOfSkipOtherComments < 0) numberOfSkipOtherComments = 0;

                    int numberOfNeededOtherComments = pageSize.Value - (myCommentsModels.Length + mutualCommentsModels.Length);

                    mutualCommentsModels = await reviewDb.Reviews
                    .Where(r => r.SubjectGuid == subjectGuid_Guid)
                    .SelectMany(r => r.Comments)
                    .Where(c => c.ReplyTo == null &&
                        c.Writer.Guid != myGuid &&
                        c.Writer.Followers.Any(ff => ff.Follower.Guid == myGuid)
                    )
                    .OrderBy(c => c.CreatedAt)
                    .Skip(numberOfSkipOtherComments)
                    .Take(numberOfNeededOtherComments)
                    .Select(c => new Review_Comment_ViewModel()
                    {
                        AmIThumbsDown = c.ThumbsDownsBy.Any(udn => udn.User.Guid == myGuid),
                        AmIThumbsUp = c.ThumbsUpsBy.Any(uup => uup.User.Guid == myGuid),
                        CreatedAt = c.CreatedAt,
                        Guid = c.Guid,
                        IsReply = false,
                        NumberOfReplies = c.Replies.Count,
                        NumberOfThumbsDowns = c.ThumbsDownsBy.Count,
                        NumberOfThumbsUps = c.ThumbsUpsBy.Count,
                        Text = c.Text,
                        WriterGuid = c.Writer.Guid,
                    })
                    .ToArrayAsync();
                }
            }
            else
            {
                othersCommentsModels = await reviewDb.Reviews
                .Where(r => r.SubjectGuid == subjectGuid_Guid)
                .SelectMany(r => r.Comments)
                .Where(c => c.ReplyTo == null &&
                    c.Writer.Guid != myGuid &&
                    c.Writer.Followers.Any(ff => ff.Follower.Guid == myGuid)
                )
                .OrderBy(c => c.CreatedAt)
                .Skip(pageIndex.Value * pageSize.Value)
                .Take(pageSize.Value)
                .Select(c => new Review_Comment_ViewModel()
                {
                    AmIThumbsDown = false,
                    AmIThumbsUp = false,
                    CreatedAt = c.CreatedAt,
                    Guid = c.Guid,
                    IsReply = false,
                    NumberOfReplies = c.Replies.Count,
                    NumberOfThumbsDowns = c.ThumbsDownsBy.Count,
                    NumberOfThumbsUps = c.ThumbsUpsBy.Count,
                    Text = c.Text,
                    WriterGuid = c.Writer.Guid,
                })
                .ToArrayAsync();
            }

            Review_Comment_ViewModel[] selectedComments = [.. myCommentsModels,
            .. mutualCommentsModels, .. othersCommentsModels];

            return Ok(selectedComments);
        }
        else//orderBy
        {
            if (orderBy == "Newest")
            {
                Review_Comment_ViewModel[] commentsModels = await reviewDb.Reviews
                .Where(r => r.SubjectGuid == subjectGuid_Guid)
                .SelectMany(r => r.Comments)
                .Where(c => c.ReplyTo == null)
                .OrderByDescending(c => c.CreatedAt)
                .Skip(pageIndex.Value * pageSize.Value)
                .Take(pageSize.Value)
                .Select(c => new Review_Comment_ViewModel()
                {
                    AmIThumbsDown = myGuid != null && c.ThumbsDownsBy.Any(udn => udn.User.Guid == myGuid),
                    AmIThumbsUp = myGuid != null && c.ThumbsUpsBy.Any(uup => uup.User.Guid == myGuid),
                    CreatedAt = c.CreatedAt,
                    Guid = c.Guid,
                    IsReply = false,
                    NumberOfReplies = c.Replies.Count,
                    NumberOfThumbsDowns = c.ThumbsDownsBy.Count,
                    NumberOfThumbsUps = c.ThumbsUpsBy.Count,
                    Text = c.Text,
                    WriterGuid = c.Writer.Guid,
                })
                .ToArrayAsync();

                return Ok(commentsModels);
            }

            if (orderBy == "Oldest")
            {
                Review_Comment_ViewModel[] commentsModels = await reviewDb.Reviews
                .Where(r => r.SubjectGuid == subjectGuid_Guid)
                .SelectMany(r => r.Comments)
                .Where(c => c.ReplyTo == null)
                .OrderBy(c => c.CreatedAt)
                .Skip(pageIndex.Value * pageSize.Value)
                .Take(pageSize.Value)
                .Select(c => new Review_Comment_ViewModel()
                {
                    AmIThumbsDown = myGuid != null && c.ThumbsDownsBy.Any(udn => udn.User.Guid == myGuid),
                    AmIThumbsUp = myGuid != null && c.ThumbsUpsBy.Any(uup => uup.User.Guid == myGuid),
                    CreatedAt = c.CreatedAt,
                    Guid = c.Guid,
                    IsReply = false,
                    NumberOfReplies = c.Replies.Count,
                    NumberOfThumbsDowns = c.ThumbsDownsBy.Count,
                    NumberOfThumbsUps = c.ThumbsUpsBy.Count,
                    Text = c.Text,
                    WriterGuid = c.Writer.Guid,
                })
                .ToArrayAsync();

                return Ok(commentsModels);
            }

            if (orderBy == "Most Agreed")
            {
                Review_Comment_ViewModel[] commentsModels = await reviewDb.Reviews
                .Where(r => r.SubjectGuid == subjectGuid_Guid)
                .SelectMany(r => r.Comments)
                .Where(c => c.ReplyTo == null)
                .OrderByDescending(c => c.ThumbsUpsBy.Count)
                .ThenBy(c => c.CreatedAt)
                .Skip(pageIndex.Value * pageSize.Value)
                .Take(pageSize.Value)
                .Select(c => new Review_Comment_ViewModel()
                {
                    AmIThumbsDown = myGuid != null && c.ThumbsDownsBy.Any(udn => udn.User.Guid == myGuid),
                    AmIThumbsUp = myGuid != null && c.ThumbsUpsBy.Any(uup => uup.User.Guid == myGuid),
                    CreatedAt = c.CreatedAt,
                    Guid = c.Guid,
                    IsReply = false,
                    NumberOfReplies = c.Replies.Count,
                    NumberOfThumbsDowns = c.ThumbsDownsBy.Count,
                    NumberOfThumbsUps = c.ThumbsUpsBy.Count,
                    Text = c.Text,
                    WriterGuid = c.Writer.Guid,
                })
                .ToArrayAsync();

                return Ok(commentsModels);
            }

            return Ok(Array.Empty<Review_Comment_ViewModel>());
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetReplies([FromQuery][StringLength(32)] string commentGuid,
    [FromQuery] int? bunchIndex)
    {
        if (!Guid.TryParseExact(commentGuid, "N", out Guid commentGuid_Guid))
        {
            ModelState.AddModelError("Parse Guid", "Couldn't parse the specified guid!");
            return BadRequest(ModelState);
        }

        var commentDbInfo = await reviewDb.Comments
        .Where(c => c.Guid == commentGuid_Guid)
        .Select(c => new
        {
            c.Id,
            writerNormalizedUserName = c.Writer.NormalizedUserName,
        })
        .FirstOrDefaultAsync();
        if (commentDbInfo is null)
        {
            ModelState.AddModelError("commentGuid", "There's not comment with the specified guid!");
            return BadRequest(ModelState);
        }

        /*string commentWriterUsername = await userManager.Users
        .Where(u => u.UserGuid == commentDbModel.Writer.Guid)
        .Select(u => u.UserName)
        .FirstOrDefaultAsync() ?? "Unkown UserName";*/

        Guid? myGuid = null;
        if ((User.Identity?.IsAuthenticated ?? false) && User.Identity.Name is not null)
        {
            myGuid = await userManager.Users
            .Where(u => u.NormalizedUserName == userManager.NormalizeName(User.Identity.Name))
            .Select(u => u.UserGuid)
            .FirstAsync();
        }

        bunchIndex ??= 0;

        Review_Comment_ViewModel[] myRepliesModels = [];
        Review_Comment_ViewModel[] mutualRepliesModels = [];
        Review_Comment_ViewModel[] othersRepliesModels = [];
        if (myGuid is not null)
        {
            myRepliesModels = await reviewDb.Comments
            .Where(c => c.Guid == commentGuid_Guid)
            .SelectMany(c => c.Replies)
            .Where(rep => rep.Writer.Guid == myGuid)
            .OrderBy(rep => rep.CreatedAt)
            .Skip(bunchIndex.Value * 10)
            .Take(10)
            .Select(rep => new Review_Comment_ViewModel()
            {
                AmIThumbsDown = rep.ThumbsDownsBy.Any(udn => udn.User.Guid == myGuid),
                AmIThumbsUp = rep.ThumbsUpsBy.Any(uup => uup.User.Guid == myGuid),
                CreatedAt = rep.CreatedAt,
                Guid = rep.Guid,
                IsReply = true,
                NumberOfReplies = rep.Replies.Count,
                NumberOfThumbsDowns = rep.ThumbsDownsBy.Count,
                NumberOfThumbsUps = rep.ThumbsUpsBy.Count,
                ReplyToBrief = rep.ReplyTo!.Text.Substring(0, rep.ReplyTo.Text.Length > 128 ? 128 : rep.ReplyTo.Text.Length),
                ReplyToGuid = rep.ReplyTo.Guid,
                ReplyToUsername = commentDbInfo.writerNormalizedUserName.ToLower(),
                Text = rep.Text,
                WriterGuid = rep.Writer.Guid,
            })
            .ToArrayAsync();

            if (myRepliesModels.Length < 10)
            {
                int totalNumberOfMyReplies;
                if (bunchIndex.Value > 0)
                {
                    totalNumberOfMyReplies = await reviewDb.Comments
                    .Where(c => c.Guid == commentGuid_Guid)
                    .SelectMany(c => c.Replies)
                    .CountAsync(rep => rep.Writer.Guid == myGuid);
                }
                else
                {
                    totalNumberOfMyReplies = myRepliesModels.Length;
                }
                int numberOfSkipMutualReplies = (bunchIndex.Value * 10) - totalNumberOfMyReplies;
                if (numberOfSkipMutualReplies < 0) numberOfSkipMutualReplies = 0;

                int numberOfNeededMutualReplies = 10 - myRepliesModels.Length;

                mutualRepliesModels = await reviewDb.Comments
                .Where(c => c.Guid == commentGuid_Guid)
                .SelectMany(c => c.Replies)
                .Where(rep => rep.Writer.Followers.Any(ff => ff.Follower.Guid == myGuid))
                .OrderBy(rep => rep.CreatedAt)
                .Skip(numberOfSkipMutualReplies)
                .Take(numberOfNeededMutualReplies)
                .Select(rep => new Review_Comment_ViewModel()
                {
                    AmIThumbsDown = rep.ThumbsDownsBy.Any(udn => udn.User.Guid == myGuid),
                    AmIThumbsUp = rep.ThumbsUpsBy.Any(uup => uup.User.Guid == myGuid),
                    CreatedAt = rep.CreatedAt,
                    Guid = rep.Guid,
                    IsReply = true,
                    NumberOfReplies = rep.Replies.Count,
                    NumberOfThumbsDowns = rep.ThumbsDownsBy.Count,
                    NumberOfThumbsUps = rep.ThumbsUpsBy.Count,
                    ReplyToBrief = rep.ReplyTo!.Text.Substring(0, rep.ReplyTo.Text.Length > 128 ? 128 : rep.ReplyTo.Text.Length),
                    ReplyToGuid = rep.ReplyTo.Guid,
                    ReplyToUsername = commentDbInfo.writerNormalizedUserName.ToLower(),
                    Text = rep.Text,
                    WriterGuid = rep.Writer.Guid,
                })
                .ToArrayAsync();
            }

            if ((myRepliesModels.Length + mutualRepliesModels.Length) < 10)
            {
                int totalNumberOfMeAndMutualReplies;
                if (bunchIndex.Value > 0)
                {
                    totalNumberOfMeAndMutualReplies = await reviewDb.Comments
                    .Where(c => c.Guid == commentGuid_Guid)
                    .SelectMany(c => c.Replies)
                    .CountAsync(rep => rep.Writer.Guid == myGuid ||
                        rep.Writer.Followers.Any(ff => ff.Follower.Guid == myGuid)
                    );
                }
                else
                {
                    totalNumberOfMeAndMutualReplies = myRepliesModels.Length + mutualRepliesModels.Length;
                }
                int numberOfSkipOthersReplies = (bunchIndex.Value * 10) - totalNumberOfMeAndMutualReplies;
                if (numberOfSkipOthersReplies < 0) numberOfSkipOthersReplies = 0;

                int numberOfNeededOthersReplies = 10 - (myRepliesModels.Length + mutualRepliesModels.Length);

                othersRepliesModels = await reviewDb.Comments
                .Where(c => c.Guid == commentGuid_Guid)
                .SelectMany(c => c.Replies)
                .Where(rep => rep.Writer.Guid != myGuid &&
                    !rep.Writer.Followers.Any(ff => ff.Follower.Guid == myGuid)
                )
                .OrderBy(rep => rep.CreatedAt)
                .Skip(numberOfSkipOthersReplies)
                .Take(numberOfNeededOthersReplies)
                .Select(rep => new Review_Comment_ViewModel()
                {
                    AmIThumbsDown = rep.ThumbsDownsBy.Any(udn => udn.User.Guid == myGuid),
                    AmIThumbsUp = rep.ThumbsUpsBy.Any(uup => uup.User.Guid == myGuid),
                    CreatedAt = rep.CreatedAt,
                    Guid = rep.Guid,
                    IsReply = true,
                    NumberOfReplies = rep.Replies.Count,
                    NumberOfThumbsDowns = rep.ThumbsDownsBy.Count,
                    NumberOfThumbsUps = rep.ThumbsUpsBy.Count,
                    ReplyToBrief = rep.ReplyTo!.Text.Substring(0, rep.ReplyTo.Text.Length > 128 ? 128 : rep.ReplyTo.Text.Length),
                    ReplyToGuid = rep.ReplyTo.Guid,
                    ReplyToUsername = commentDbInfo.writerNormalizedUserName.ToLower(),
                    Text = rep.Text,
                    WriterGuid = rep.Writer.Guid,
                })
                .ToArrayAsync();
            }
        }
        else//here myGuid is null
        {
            othersRepliesModels = await reviewDb.Comments
            .Where(c => c.Guid == commentGuid_Guid)
            .SelectMany(c => c.Replies)
            .OrderBy(rep => rep.CreatedAt)
            .Skip(bunchIndex.Value * 10)
            .Take(10)
            .Select(rep => new Review_Comment_ViewModel()
            {
                AmIThumbsDown = false,
                AmIThumbsUp = false,
                CreatedAt = rep.CreatedAt,
                Guid = rep.Guid,
                IsReply = true,
                NumberOfReplies = rep.Replies.Count,
                NumberOfThumbsDowns = rep.ThumbsDownsBy.Count,
                NumberOfThumbsUps = rep.ThumbsUpsBy.Count,
                ReplyToBrief = rep.ReplyTo!.Text.Substring(0, rep.ReplyTo.Text.Length > 128 ? 128 : rep.ReplyTo.Text.Length),
                ReplyToGuid = rep.ReplyTo.Guid,
                ReplyToUsername = commentDbInfo.writerNormalizedUserName.ToLower(),
                Text = rep.Text,
                WriterGuid = rep.Writer.Guid,
            })
            .ToArrayAsync();
        }

        Review_Comment_ViewModel[] repliesModels = [.. myRepliesModels, .. mutualRepliesModels,
        .. othersRepliesModels];

        return Ok(repliesModels);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitNewComment([FromForm] Review_NewComment_FormModel formModel,
    [FromServices] Notification_DbContext notifDb,
    [FromServices] Notification_Process notifProcess, [FromServices] Library_DbContext libraryDb)
    {
        if (ModelState.IsValid)
        {
            var parentReviewDbInfo = await reviewDb.Reviews
            .Where(r => r.SubjectGuid == formModel.ParentSubjectGuid)
            .Select(r => new
            {
                r.Id,
                ownerGuid = r.Owner.Guid,
            })
            .FirstOrDefaultAsync();
            if (parentReviewDbInfo is null)
            {
                ModelState.AddModelError("parentSubjectGuid", "There's no review with the specified guid!");
                return BadRequest(ModelState);
            }

            var me = await userManager.Users
            .Where(u => u.NormalizedUserName == userManager.NormalizeName(User.Identity!.Name))
            .Select(u => new { u.UserGuid, u.UserName })
            .FirstAsync();

            //Create
            Review_UserDbModel myDbModel = await reviewDb.Users
            .Where(u => u.Guid == me.UserGuid)
            .Select(u => new Review_UserDbModel() { Id = u.Id })
            .FirstAsync();
            Review_ReviewDbModel parentReviewDbModel = new() { Id = parentReviewDbInfo.Id };
            //Attach
            reviewDb.Reviews.Attach(parentReviewDbModel);
            reviewDb.Users.Attach(myDbModel);

            Review_CommentDbModel comment = new()
            {
                ParentReview = parentReviewDbModel,
                Text = formModel.Text,
                Writer = myDbModel,
            };

            reviewDb.Comments.Add(comment);
            await reviewDb.SaveChangesAsync();

            //notif
            string documentTitle = await libraryDb.Documents
            .Where(doc => doc.Guid == formModel.ParentSubjectGuid)
            .Select(doc => doc.Title)
            .FirstAsync();
            Notification_NotifCreation_FormModel notifModel = new()
            {
                OwnerGuid = parentReviewDbInfo.ownerGuid,
                SubjectGuid = comment.Guid,
                Title = "New Comment",
                Description = [
                    $"From '{me.UserName}' for document '{documentTitle}': ",
                    comment.Text[..(comment.Text.Length > 128 ? 128 : comment.Text.Length)],
                ],
                Link = $"/document/{formModel.ParentSubjectGuid}?commentGuid={comment.Guid}",
            };
            await notifProcess.CreateNewNotification(notifDb, notifModel);

            Review_Comment_ViewModel commentModel = new()
            {
                CreatedAt = comment.CreatedAt,
                Guid = comment.Guid,
                Text = comment.Text,
                WriterGuid = comment.Writer.Guid,
            };

            return Ok(commentModel);
        }

        return BadRequest(ModelState);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitNewReply([FromForm] Review_NewReply_FormModel formModel,
    [FromServices] Notification_DbContext notifDb,
    [FromServices] Notification_Process notifProcess, [FromServices] Library_DbContext libraryDb)
    {
        if (ModelState.IsValid)
        {
            var parentCommentDbInfo = await reviewDb.Comments
            .Where(c => c.Guid == formModel.ParentCommentGuid)
            .Select(c => new
            {
                c.Id,
                writerGuid = c.Writer.Guid,
                writerNormalizedUserName = c.Writer.NormalizedUserName,
                parentReviewSubjectGuid = c.ParentReview.SubjectGuid,
                parentReviewId = c.ParentReview.Id,
                c.Text,
            })
            .FirstOrDefaultAsync();
            if (parentCommentDbInfo is null)
            {
                ModelState.AddModelError("parentCommentGuid", "There's no comment with the specified guid!");
                return BadRequest(ModelState);
            }

            /*string parentCommentWriterUserName = await userManager.Users
            .Where(u => u.UserGuid == parentCommentDbModel.Writer.Guid)
            .Select(u => u.UserName!)
            .FirstAsync();*/

            var me = await userManager.Users
            .Where(u => u.NormalizedUserName == userManager.NormalizeName(User.Identity!.Name))
            .Select(u => new { u.UserGuid, u.UserName })
            .FirstAsync();

            //create
            Review_UserDbModel myDbModel = await reviewDb.Users
            .Where(u => u.Guid == me.UserGuid)
            .Select(u => new Review_UserDbModel() { Id = u.Id, })
            .FirstAsync();
            Review_ReviewDbModel parentReviewDbModel = new() { Id = parentCommentDbInfo.parentReviewId };
            Review_CommentDbModel parentCommentDbModel = new() { Id = parentCommentDbInfo.Id };
            //Attach
            reviewDb.Users.Attach(myDbModel);
            reviewDb.Reviews.Attach(parentReviewDbModel);
            reviewDb.Comments.Attach(parentCommentDbModel);

            Review_CommentDbModel reply = new()
            {
                ReplyTo = parentCommentDbModel,
                Text = formModel.Text,
                Writer = myDbModel,
                ParentReview = parentReviewDbModel,
            };

            reviewDb.Comments.Add(reply);
            await reviewDb.SaveChangesAsync();

            //notif
            string documentTitle = await libraryDb.Documents
            .Where(doc => doc.Guid == parentCommentDbInfo.parentReviewSubjectGuid)
            .Select(doc => doc.Title)
            .FirstAsync();
            Notification_NotifCreation_FormModel notifModel = new()
            {
                OwnerGuid = parentCommentDbInfo.writerGuid,
                SubjectGuid = formModel.ParentCommentGuid,
                Title = "New Reply",
                Description = [
                    $"From '{me.UserName}' in document '{documentTitle}': ",
                    reply.Text[..(reply.Text.Length > 128 ? 128 : reply.Text.Length)],
                ],
                Link = $"/document/{parentCommentDbInfo.parentReviewSubjectGuid}?commentGuid={reply.Guid}",
            };
            await notifProcess.CreateNewNotification(notifDb, notifModel);

            int briefLength = parentCommentDbInfo.Text.Length > 128 ? 128 : parentCommentDbInfo.Text.Length;
            Review_Comment_ViewModel replyModel = new()
            {
                CreatedAt = reply.CreatedAt,
                Guid = reply.Guid,
                Text = reply.Text,
                WriterGuid = reply.Writer.Guid,
                IsReply = true,
                ReplyToBrief = parentCommentDbInfo.Text[..briefLength],
                ReplyToGuid = formModel.ParentCommentGuid,
                ReplyToUsername = parentCommentDbInfo.writerNormalizedUserName.ToLower(),
            };

            return Ok(replyModel);
        }

        return BadRequest(ModelState);
    }

    [HttpDelete]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteComment([FromQuery][StringLength(32)] string commentGuid,
    [FromServices] Notification_DbContext notifDb,
    [FromServices] Notification_Process notifProcess)
    {
        if (!Guid.TryParseExact(commentGuid, "N", out Guid commentGuid_Guid))
        {
            ModelState.AddModelError("Parse Guid", "Couldn't parse the specified guid!");
            return BadRequest(ModelState);
        }

        var commentDbInfo = await reviewDb.Comments
        .Where(c => c.Guid == commentGuid_Guid)
        .Select(c => new
        {
            c.Id,
            writerGuid = c.Writer.Guid,
        })
        .FirstOrDefaultAsync();
        if (commentDbInfo is null)
        {
            ModelState.AddModelError("commentGuid", "There's not comment with the specified guid!");
            return BadRequest(ModelState);
        }

        Guid myGuid = await userManager.Users
        .Where(u => u.NormalizedUserName == userManager.NormalizeName(User.Identity!.Name))
        .Select(u => u.UserGuid)
        .FirstAsync();

        if (commentDbInfo.writerGuid != myGuid)
        {
            ModelState.AddModelError("Authorization", "Only the writer of the comment can delete the comment!");
            return BadRequest(ModelState);
        }

        await reviewDb.Comments.Where(c => c.Id == commentDbInfo.Id).ExecuteDeleteAsync();

        //notif
        await notifProcess.DeleteNotification(notifDb, commentGuid_Guid);

        return Ok(new { success = true });
    }





    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleLike([FromQuery][StringLength(32)] string subjectGuid)
    {
        if (!Guid.TryParseExact(subjectGuid, "N", out Guid subjectGuid_Guid))
        {
            ModelState.AddModelError("Parse Guid", "Couldn't parse the specified guid!");
            return BadRequest(ModelState);
        }
        Guid myGuid = await userManager.Users
        .Where(u => u.NormalizedUserName == userManager.NormalizeName(User.Identity!.Name))
        .Select(u => u.UserGuid)
        .FirstAsync();

        var reviewDbInfo = await reviewDb.Reviews
        .Where(r => r.SubjectGuid == subjectGuid_Guid)
        .Select(r => new
        {
            r.Id,
            totalLikes = r.LikedBy.Count,
            iLiked = r.LikedBy.Any(u => u.User.Guid == myGuid),
        })
        .FirstOrDefaultAsync();
        if (reviewDbInfo is null)
        {
            ModelState.AddModelError("subjectGuid", "There's no review with the specified guid!");
            return BadRequest(ModelState);
        }

        int totalLikes = reviewDbInfo.totalLikes;

        if (reviewDbInfo.iLiked)
        {
            //delete UserLike join table
            await reviewDb.UserLikes
            .Where(ul => ul.ReviewId == reviewDbInfo.Id && ul.User.Guid == myGuid)
            .ExecuteDeleteAsync();

            totalLikes--;
        }
        else
        {
            //fetch
            Review_UserDbModel myDbModel = await reviewDb.Users
            .Where(u => u.Guid == myGuid)
            .Select(u => new Review_UserDbModel() { Id = u.Id })
            .FirstAsync();
            //create UserLike join table
            Review_UserLike_DbModel userLike = new() { UserId = myDbModel.Id, ReviewId = reviewDbInfo.Id };
            //add join table
            reviewDb.UserLikes.Add(userLike);
            //save
            await reviewDb.SaveChangesAsync();

            totalLikes++;
        }

        return Ok(new { numberOfLikes = totalLikes });
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleThumbsUp([FromQuery][StringLength(32)] string commentGuid)
    {
        if (!Guid.TryParseExact(commentGuid, "N", out Guid commentGuid_Guid))
        {
            ModelState.AddModelError("Parse Guid", "Couldn't parse the specified guid!");
            return BadRequest(ModelState);
        }
        Guid myGuid = await userManager.Users
        .Where(u => u.NormalizedUserName == userManager.NormalizeName(User.Identity!.Name))
        .Select(u => u.UserGuid)
        .FirstAsync();

        var commentDbInfo = await reviewDb.Comments
        .Where(c => c.Guid == commentGuid_Guid)
        .Select(c => new
        {
            c.Id,
            iThumbsUp = c.ThumbsUpsBy.Any(uup => uup.User.Guid == myGuid),
            iThumbsDown = c.ThumbsDownsBy.Any(udn => udn.User.Guid == myGuid),
            totalThumbsUps = c.ThumbsUpsBy.Count,
            totalThumbsDowns = c.ThumbsDownsBy.Count,
        })
        .FirstOrDefaultAsync();
        if (commentDbInfo is null)
        {
            ModelState.AddModelError("commentGuid", "There's no comment with the specified guid!");
            return BadRequest(ModelState);
        }

        int totalThumbsUps = commentDbInfo.totalThumbsUps;
        int totalThumbsDowns = commentDbInfo.totalThumbsDowns;

        if (commentDbInfo.iThumbsUp)
        {
            //delete UserThumbsUp join table
            await reviewDb.UserThumbsUp
            .Where(uup => uup.CommentId == commentDbInfo.Id && uup.User.Guid == myGuid)
            .ExecuteDeleteAsync();

            totalThumbsUps--;
        }
        else
        {
            //fetch
            Review_UserDbModel myDbModel = await reviewDb.Users
            .Where(u => u.Guid == myGuid)
            .Select(u => new Review_UserDbModel() { Id = u.Id })
            .FirstAsync();
            //create UserThumbsUp join table
            Review_UserThumbsUp_DbModel userThumbsUp = new() { UserId = myDbModel.Id, CommentId = commentDbInfo.Id };
            //add
            reviewDb.UserThumbsUp.Add(userThumbsUp);
            //save
            await reviewDb.SaveChangesAsync();

            totalThumbsUps++;

            if (commentDbInfo.iThumbsDown)
            {
                //delete UserThumbsDown join table
                await reviewDb.UserThumbsDown
                .Where(udn => udn.CommentId == commentDbInfo.Id && udn.UserId == myDbModel.Id)
                .ExecuteDeleteAsync();

                totalThumbsDowns--;
            }
        }

        return Ok(new
        {
            numberOfThumbUps = totalThumbsUps,
            numberOfThumbDowns = totalThumbsDowns
        });
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleThumbsDown([FromQuery][StringLength(32)] string commentGuid)
    {
        if (!Guid.TryParseExact(commentGuid, "N", out Guid commentGuid_Guid))
        {
            ModelState.AddModelError("Parse Guid", "Couldn't parse the specified guid!");
            return BadRequest(ModelState);
        }
        Guid myGuid = await userManager.Users
        .Where(u => u.NormalizedUserName == userManager.NormalizeName(User.Identity!.Name))
        .Select(u => u.UserGuid)
        .FirstAsync();

        var commentDbInfo = await reviewDb.Comments
        .Where(c => c.Guid == commentGuid_Guid)
        .Select(c => new
        {
            c.Id,
            iThumbsUp = c.ThumbsUpsBy.Any(uup => uup.User.Guid == myGuid),
            iThumbsDown = c.ThumbsDownsBy.Any(udn => udn.User.Guid == myGuid),
            totalThumbsUps = c.ThumbsUpsBy.Count,
            totalThumbsDowns = c.ThumbsDownsBy.Count,
        })
        .FirstOrDefaultAsync();
        if (commentDbInfo is null)
        {
            ModelState.AddModelError("commentGuid", "There's no comment with the specified guid!");
            return BadRequest(ModelState);
        }

        int totalThumbsUps = commentDbInfo.totalThumbsUps;
        int totalThumbsDowns = commentDbInfo.totalThumbsDowns;

        if (commentDbInfo.iThumbsDown)
        {
            //delete UserThumbsDown join table
            await reviewDb.UserThumbsDown
            .Where(udn => udn.CommentId == commentDbInfo.Id && udn.User.Guid == myGuid)
            .ExecuteDeleteAsync();

            totalThumbsDowns--;
        }
        else
        {
            //fetch
            Review_UserDbModel myDbModel = await reviewDb.Users
            .Where(u => u.Guid == myGuid)
            .Select(u => new Review_UserDbModel() { Id = u.Id })
            .FirstAsync();
            //create UserThumbsDown join table
            Review_UserThumbsDown_DbModel userThumbsDown = new() { UserId = myDbModel.Id, CommentId = commentDbInfo.Id };
            //add
            reviewDb.UserThumbsDown.Add(userThumbsDown);
            //save
            await reviewDb.SaveChangesAsync();

            totalThumbsDowns++;

            if (commentDbInfo.iThumbsUp)
            {
                //delete UserThumbsUp join table
                await reviewDb.UserThumbsUp
                .Where(uup => uup.CommentId == commentDbInfo.Id && uup.UserId == myDbModel.Id)
                .ExecuteDeleteAsync();

                totalThumbsUps--;
            }
        }

        return Ok(new
        {
            numberOfThumbUps = totalThumbsUps,
            numberOfThumbDowns = totalThumbsDowns
        });
    }





    [HttpGet]
    public async Task<IActionResult> GetLikesUserList([FromQuery][StringLength(32)] string subjectGuid,
    [FromQuery] int? bunchIndex, [FromQuery][StringLength(30)] string? filter)
    {
        if (!Guid.TryParseExact(subjectGuid, "N", out Guid subjectGuid_Guid))
        {
            ModelState.AddModelError("Parse Guid", "Couldn't parse the specified guid!");
            return BadRequest(ModelState);
        }
        Guid? myGuid = null;
        if ((User.Identity?.IsAuthenticated ?? false) && User.Identity.Name is not null)
        {
            myGuid = await userManager.Users
            .Where(u => u.NormalizedUserName == userManager.NormalizeName(User.Identity!.Name))
            .Select(u => u.UserGuid)
            .FirstAsync();
        }

        var reviewDbInfo = await reviewDb.Reviews
        .Where(r => r.SubjectGuid == subjectGuid_Guid)
        .Select(r => new
        {
            r.Id,
            iLiked = r.LikedBy.Any(ul => ul.User.Guid == myGuid),
        })
        .FirstAsync();
        if (reviewDbInfo is null)
        {
            ModelState.AddModelError("subjectGuid", "There's no review with the specified guid!");
            return BadRequest(ModelState);
        }

        bunchIndex ??= 0;
        int bunchSize = 10;

        if (string.IsNullOrWhiteSpace(filter))
        {
            filter = null;
        }
        else
        {
            filter = userManager.NormalizeName(filter.Trim());
        }

        List<Guid> likesGuids_MeAndMutual = [];
        List<Guid> likesGuids_Others = [];
        if (myGuid is not null)
        {
            likesGuids_MeAndMutual = await reviewDb.Reviews
            .Where(r => r.SubjectGuid == subjectGuid_Guid)
            .SelectMany(r => r.LikedBy)
            .Select(ul => ul.User)
            .Where(u =>
                (filter == null || u.NormalizedUserName.Contains(filter)) &&
                u.Followers.Any(ff => ff.Follower.Guid == myGuid)
            )
            .OrderBy(u => u.Id)
            .Select(u => u.Guid)
            .Skip(bunchIndex.Value * bunchSize)
            .Take(bunchSize)
            .ToListAsync();

            if (reviewDbInfo.iLiked)
            {
                likesGuids_MeAndMutual = [myGuid.Value, .. likesGuids_MeAndMutual];
            }

            if (likesGuids_MeAndMutual.Count < bunchSize)
            {
                int totalNumberOfMeAndMutualLikes = await reviewDb.Reviews
                .Where(r => r.SubjectGuid == subjectGuid_Guid)
                .SelectMany(r => r.LikedBy)
                .Select(ul => ul.User)
                .CountAsync(u =>
                    (filter == null || u.NormalizedUserName.Contains(filter)) &&
                    u.Followers.Any(ff => ff.Follower.Guid == myGuid)
                );
                int numberOfSkipOthersLikes = (bunchIndex.Value * bunchSize) - totalNumberOfMeAndMutualLikes;
                if (numberOfSkipOthersLikes < 0) numberOfSkipOthersLikes = 0;

                int numberOfNeededOthersLikes = bunchSize - likesGuids_MeAndMutual.Count;

                likesGuids_Others = await reviewDb.Reviews
                .Where(r => r.SubjectGuid == subjectGuid_Guid)
                .SelectMany(r => r.LikedBy)
                .Select(ul => ul.User)
                .Where(u =>
                    (filter == null || u.NormalizedUserName.Contains(filter)) &&
                    u.Guid != myGuid &&
                    !u.Followers.Any(ff => ff.Follower.Guid == myGuid)
                )
                .OrderBy(u => u.Id)
                .Select(u => u.Guid)
                .Skip(numberOfSkipOthersLikes)
                .Take(numberOfNeededOthersLikes)
                .ToListAsync();
            }

        }
        else
        {
            likesGuids_Others = await reviewDb.Reviews
            .Where(r => r.SubjectGuid == subjectGuid_Guid)
            .SelectMany(r => r.LikedBy)
            .Select(ul => ul.User)
            .Where(u => filter == null || u.NormalizedUserName.Contains(filter))
            .OrderBy(u => u.Id)
            .Select(u => u.Guid)
            .Skip(bunchIndex.Value * bunchSize)
            .Take(bunchSize)
            .ToListAsync();
        }

        List<Guid> likesUserGuids = [.. likesGuids_MeAndMutual, .. likesGuids_Others];

        Library_Owner_ViewModel[] likesOwnerModels = await userManager.Users
        .Where(u => likesUserGuids.Contains(u.UserGuid))
        .Select(u => new Library_Owner_ViewModel()
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
    [FromQuery] int? bunchIndex, [FromQuery][StringLength(30)] string? filter)
    {
        if (!Guid.TryParseExact(commentGuid, "N", out Guid commentGuid_Guid))
        {
            ModelState.AddModelError("Parse Guid", "Couldn't parse the specified guid!");
            return BadRequest(ModelState);
        }
        Guid? myGuid = null;
        if ((User.Identity?.IsAuthenticated ?? false) && User.Identity.Name is not null)
        {
            myGuid = await userManager.Users
            .Where(u => u.NormalizedUserName == userManager.NormalizeName(User.Identity!.Name))
            .Select(u => u.UserGuid)
            .FirstAsync();
        }

        var commentDbInfo = await reviewDb.Comments
        .Where(c => c.Guid == commentGuid_Guid)
        .Select(c => new
        {
            c.Id,
            iThumbsUp = c.ThumbsUpsBy.Any(uup => uup.User.Guid == myGuid),
        })
        .FirstOrDefaultAsync();
        if (commentDbInfo is null)
        {
            ModelState.AddModelError("commentGuid", "There's no comment with the specified guid!");
            return BadRequest(ModelState);
        }

        bunchIndex ??= 0;
        int bunchSize = 10;

        if (string.IsNullOrWhiteSpace(filter))
        {
            filter = null;
        }
        else
        {
            filter = userManager.NormalizeName(filter.Trim());
        }

        List<Guid> thumbsUpsGuids_MeAndMutual = [];
        List<Guid> thumbsUpsGuids_Others = [];
        if (myGuid is not null)
        {
            thumbsUpsGuids_MeAndMutual = await reviewDb.Comments
            .Where(c => c.Guid == commentGuid_Guid)
            .SelectMany(c => c.ThumbsUpsBy)
            .Select(uup => uup.User)
            .Where(u =>
                (filter == null || u.NormalizedUserName.Contains(filter)) &&
                u.Followers.Any(ff => ff.Follower.Guid == myGuid)
            )
            .OrderBy(u => u.Id)
            .Select(u => u.Guid)
            .Skip(bunchIndex.Value * bunchSize)
            .Take(bunchSize)
            .ToListAsync();

            if (commentDbInfo.iThumbsUp)
            {
                thumbsUpsGuids_MeAndMutual = [myGuid.Value, .. thumbsUpsGuids_MeAndMutual];
            }

            if (thumbsUpsGuids_MeAndMutual.Count < bunchSize)
            {
                int totalNumberOfMyFollowingsThumbsUps = await reviewDb.Comments
                .Where(c => c.Guid == commentGuid_Guid)
                .SelectMany(c => c.ThumbsUpsBy)
                .Select(uup => uup.User)
                .CountAsync(u =>
                    (filter == null || u.NormalizedUserName.Contains(filter)) &&
                    u.Followers.Any(ff => ff.Follower.Guid == myGuid)
                );
                int numberOfSkipOthersThumbsUps = (bunchIndex.Value * bunchSize) - totalNumberOfMyFollowingsThumbsUps;
                if (numberOfSkipOthersThumbsUps < 0) numberOfSkipOthersThumbsUps = 0;

                int numberOfNeededOthersThumbsUps = bunchSize - thumbsUpsGuids_MeAndMutual.Count;

                thumbsUpsGuids_Others = await reviewDb.Comments
                .Where(c => c.Guid == commentGuid_Guid)
                .SelectMany(c => c.ThumbsUpsBy)
                .Select(uup => uup.User)
                .Where(u =>
                    (filter == null || u.NormalizedUserName.Contains(filter)) &&
                    u.Guid != myGuid &&
                    !u.Followers.Any(ff => ff.Follower.Guid == myGuid)
                )
                .OrderBy(u => u.Id)
                .Select(u => u.Guid)
                .Skip(numberOfSkipOthersThumbsUps)
                .Take(numberOfNeededOthersThumbsUps)
                .ToListAsync();
            }
        }
        else
        {
            thumbsUpsGuids_Others = await reviewDb.Comments
            .Where(c => c.Guid == commentGuid_Guid)
            .SelectMany(c => c.ThumbsUpsBy)
            .Select(uup => uup.User)
            .Where(u => filter == null || u.NormalizedUserName.Contains(filter))
            .OrderBy(u => u.Id)
            .Select(u => u.Guid)
            .Skip(bunchIndex.Value * bunchSize)
            .Take(bunchSize)
            .ToListAsync();
        }

        List<Guid> thumbsUpsUserGuids = [.. thumbsUpsGuids_MeAndMutual, .. thumbsUpsGuids_Others];

        Library_Owner_ViewModel[] thumbsUpsOwnerModels = await userManager.Users
        .Where(u => thumbsUpsUserGuids.Contains(u.UserGuid))
        .Select(u => new Library_Owner_ViewModel()
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
    [FromQuery] int? bunchIndex, [FromQuery][StringLength(30)] string? filter)
    {
        if (!Guid.TryParseExact(commentGuid, "N", out Guid commentGuid_Guid))
        {
            ModelState.AddModelError("Parse Guid", "Couldn't parse the specified guid!");
            return BadRequest(ModelState);
        }
        Guid? myGuid = null;
        if ((User.Identity?.IsAuthenticated ?? false) && User.Identity.Name is not null)
        {
            myGuid = await userManager.Users
            .Where(u => u.NormalizedUserName == userManager.NormalizeName(User.Identity!.Name))
            .Select(u => u.UserGuid)
            .FirstAsync();
        }

        var commentDbInfo = await reviewDb.Comments
        .Where(c => c.Guid == commentGuid_Guid)
        .Select(c => new
        {
            c.Id,
            iThumbsDown = c.ThumbsDownsBy.Any(uup => uup.User.Guid == myGuid),
        })
        .FirstOrDefaultAsync();
        if (commentDbInfo is null)
        {
            ModelState.AddModelError("commentGuid", "There's no comment with the specified guid!");
            return BadRequest(ModelState);
        }

        bunchIndex ??= 0;
        int bunchSize = 10;

        if (string.IsNullOrWhiteSpace(filter))
        {
            filter = null;
        }
        else
        {
            filter = userManager.NormalizeName(filter.Trim());
        }

        List<Guid> thumbsDownsGuids_MeAndMutual = [];
        List<Guid> thumbsDownsGuids_Others = [];
        if (myGuid is not null)
        {
            thumbsDownsGuids_MeAndMutual = await reviewDb.Comments
            .Where(c => c.Guid == commentGuid_Guid)
            .SelectMany(c => c.ThumbsDownsBy)
            .Select(udn => udn.User)
            .Where(u =>
                (filter == null || u.NormalizedUserName.Contains(filter)) &&
                u.Followers.Any(ff => ff.Follower.Guid == myGuid)
            )
            .OrderBy(u => u.Id)
            .Select(u => u.Guid)
            .Skip(bunchIndex.Value * bunchSize)
            .Take(bunchSize)
            .ToListAsync();

            if (commentDbInfo.iThumbsDown)
            {
                thumbsDownsGuids_MeAndMutual = [myGuid.Value, .. thumbsDownsGuids_MeAndMutual];
            }

            if (thumbsDownsGuids_MeAndMutual.Count < bunchSize)
            {
                int totalNumberOfMyFollowingsThumbsDowns = await reviewDb.Comments
                .Where(c => c.Guid == commentGuid_Guid)
                .SelectMany(c => c.ThumbsDownsBy)
                .Select(udn => udn.User)
                .CountAsync(u =>
                    (filter == null || u.NormalizedUserName.Contains(filter)) &&
                    u.Followers.Any(ff => ff.Follower.Guid == myGuid)
                );
                int numberOfSkipOthersThumbsDowns = (bunchIndex.Value * bunchSize) - totalNumberOfMyFollowingsThumbsDowns;
                if (numberOfSkipOthersThumbsDowns < 0) numberOfSkipOthersThumbsDowns = 0;

                int numberOfNeededOthersThumbsDowns = bunchSize - thumbsDownsGuids_MeAndMutual.Count;

                thumbsDownsGuids_Others = await reviewDb.Comments
                .Where(c => c.Guid == commentGuid_Guid)
                .SelectMany(c => c.ThumbsDownsBy)
                .Select(udn => udn.User)
                .Where(u =>
                    (filter == null || u.NormalizedUserName.Contains(filter)) &&
                    u.Guid != myGuid &&
                    !u.Followers.Any(ff => ff.Follower.Guid == myGuid)
                )
                .OrderBy(u => u.Id)
                .Select(u => u.Guid)
                .Skip(numberOfSkipOthersThumbsDowns)
                .Take(numberOfNeededOthersThumbsDowns)
                .ToListAsync();
            }
        }
        else
        {
            thumbsDownsGuids_Others = await reviewDb.Comments
            .Where(c => c.Guid == commentGuid_Guid)
            .SelectMany(c => c.ThumbsDownsBy)
            .Select(udn => udn.User)
            .Where(u => filter == null || u.NormalizedUserName.Contains(filter))
            .OrderBy(u => u.Id)
            .Select(u => u.Guid)
            .Skip(bunchIndex.Value * bunchSize)
            .Take(bunchSize)
            .ToListAsync();
        }

        List<Guid> thumbsDownsUserGuids = [.. thumbsDownsGuids_MeAndMutual, .. thumbsDownsGuids_Others];

        Library_Owner_ViewModel[] thumbsDownsOwnerModels = await userManager.Users
        .Where(u => thumbsDownsUserGuids.Contains(u.UserGuid))
        .Select(u => new Library_Owner_ViewModel()
        {
            Guid = u.UserGuid,
            HasImage = u.HasImage,
            IntegrityVersion = u.IntegrityVersion,
            Username = u.UserName!,
        })
        .ToArrayAsync();

        return Ok(thumbsDownsOwnerModels);
    }

}