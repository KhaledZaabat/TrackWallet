using ErrorOr;
using Expense_Tracker.Application.Constants;
using Expense_Tracker.Application.Features.FilesFolder.Commads.UploadImage;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Contracts.Responses.Files;
using Expense_Tracker.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Expense_Tracker.Domain.Errors;
using Wolverine;

namespace Expense_Tracker.Application.Features.Userr.UpdateProfile;

public sealed class UpdateProfileCommandHandler(
    IRepository<User> userRepo,
    IUserContext userContext,
    IMessageBus bus,
    IFileService fileService)
{
    public async Task<ErrorOr<Success>> Handle(UpdateProfileCommand cmd, CancellationToken ct)
    {
        Guid? userId = userContext.UserId;
        if (userId is null)
            return DomainErrors.UserErrors.NotFound();

        User? user = await userRepo.QueryTracked()
            .FirstOrDefaultAsync(u => u.Id == userId, ct);

        if (user is null)
            return DomainErrors.UserErrors.NotFound();

        if (!string.IsNullOrWhiteSpace(cmd.FullName))
        {
            var updateResult = user.UpdateFullName(cmd.FullName);
            if (updateResult.IsError)
                return updateResult.FirstError;
        }

        if (cmd.BirthDate.HasValue)
        {
            var updateResult = user.UpdateBirthDate(cmd.BirthDate.Value);
            if (updateResult.IsError)
                return updateResult.FirstError;
        }

        if (cmd.IsMale.HasValue)
        {
            var updateResult = user.UpdateGender(cmd.IsMale.Value);
            if (updateResult.IsError)
                return updateResult.FirstError;
        }

        if (cmd.ProfileImage is not null)
        {
            // Delete old profile image if exists
            if (user.ProfileImageFileId.HasValue && user.ProfileImageFileId.Value != Guid.Empty)
            {
                if (user.ProfileImageFileId is Guid oldId && oldId != Guid.Empty)
                {
                    var delResult = await fileService.DeleteAsync(oldId, ct);
                    if (delResult.IsError)
                        return delResult.FirstError;
                }

            }

            var uploadImageCommand = new UploadImageCommand(
                EntityType: nameof(User),
                EntityId: userId.Value,
                folder: DefaultFolders.Profiles,
                Image: cmd.ProfileImage
            );

            var uploadResult = await bus.InvokeAsync<ErrorOr<UploadImageResponse>>(uploadImageCommand, ct);
            if (uploadResult.IsError)
                return uploadResult.FirstError;

            var assignResult = user.AssignProfileImage(uploadResult.Value.FileId);
            if (assignResult.IsError)
                return assignResult.FirstError;
        }

        await userRepo.SaveChangesAsync(ct);

        return new Success();
    }
}
