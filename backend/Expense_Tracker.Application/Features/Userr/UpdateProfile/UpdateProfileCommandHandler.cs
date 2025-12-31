using Expense_Tracker.Application.Constants;
using Expense_Tracker.Application.Features.FilesFolder.Commads.UploadImage;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Contracts.Responses.Files;
using Expense_Tracker.Domain.Common.ResultPattern.Error;
using Expense_Tracker.Domain.Common.ResultPattern.Result;
using Expense_Tracker.Domain.Users;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Expense_Tracker.Application.Features.Userr.UpdateProfile;

public sealed class UpdateProfileCommandHandler(
    IAppDbContext db,
    IUserContext userContext,
    ISender sender,
    IFileService fileService)
    : IRequestHandler<UpdateProfileCommand, Result>
{
    public async Task<Result> Handle(UpdateProfileCommand cmd, CancellationToken ct)
    {
        Guid? userId = userContext.UserId;
        if (userId is null)
            return Result.Failure(UserError.NotFound());

        User? user = await db.Users
            .FirstOrDefaultAsync(u => u.Id == userId, ct);

        if (user is null)
            return Result.Failure(UserError.NotFound());



        // Update FullName if provided
        if (!string.IsNullOrWhiteSpace(cmd.FullName))
        {
            Result updateResult = user.UpdateFullName(cmd.FullName);
            if (updateResult.IsFailure)
                return Result.Failure(updateResult.TryGetError());
        }

        // Update BirthDate if provided
        if (cmd.BirthDate.HasValue)
        {
            Result updateResult = user.UpdateBirthDate(cmd.BirthDate.Value);
            if (updateResult.IsFailure)
                return Result.Failure(updateResult.TryGetError());
        }

        // Update Gender if provided
        if (cmd.IsMale.HasValue)
        {
            Result updateResult = user.UpdateGender(cmd.IsMale.Value);
            if (updateResult.IsFailure)
                return Result.Failure(updateResult.TryGetError());
        }

        // Update ProfileImage if provided
        if (cmd.ProfileImage is not null)
        {
            // Delete old profile image if exists
            if (user.ProfileImageFileId.HasValue && user.ProfileImageFileId.Value != Guid.Empty)
            {
                if (user.ProfileImageFileId is Guid oldId && oldId != Guid.Empty)
                {
                    Result del = await fileService.DeleteAsync(oldId, ct);
                    if (del.IsFailure)
                        return Result.Failure(del.TryGetError());
                }

            }

            var uploadImageCommand = new UploadImageCommand(
                EntityType: nameof(User),
                EntityId: userId.Value,
                folder: DefaultFolders.Profiles,
                Image: cmd.ProfileImage
            );

            Result<UploadImageResponse> uploadResult = await sender.Send(uploadImageCommand, ct);
            if (uploadResult.IsFailure)
                return Result.Failure(uploadResult.TryGetError());

            Result assignResult = user.AssignProfileImage(uploadResult.TryGetValue().FileId);
            if (assignResult.IsFailure)
                return Result.Failure(assignResult.TryGetError());
        }

        await db.SaveChangesAsync(ct);

        return Result.Success();
    }
}