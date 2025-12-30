using Expense_Tracker.Application.Constants;
using Expense_Tracker.Application.Dtos;
using Expense_Tracker.Application.Features.FilesFolder.Commads.UploadImage;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Contracts.Responses.Files;
using Expense_Tracker.Domain.Common.ResultPattern.Result;
using Expense_Tracker.Domain.Users;
using MediatR;

namespace Expense_Tracker.Application.Features.Register;

public sealed class RegisterCommandHandler(
    IIdentityService identityService,
    IAppDbContext db,
    ISender sender)
    : IRequestHandler<RegisterCommand, Result>
{
    public async Task<Result> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        // Step 1: Create Identity User with password
        Result<IdentityRegistrationResult> identityResult = await identityService.CreateIdentityByEmailAsync(
            email: request.Email,
            password: request.Password,
            userName: request.UserName,
            cancellationToken);

        if (identityResult.IsFailure)
            return Result.Failure(identityResult.TryGetError());

        var identity = identityResult.TryGetValue();
        Guid userId = Guid.Parse(identity.IdentityUserId);

        // Step 2: Begin TransactionFolder
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            // Step 3: Create Domain User
            Result<User> userResult = User.Create(
                id: userId,
                fullName: request.FullName,
                userName: request.UserName,
                email: request.Email,
                birthDate: request.BirthDate,
                isMale: request.IsMale,
                fireEvent: false);

            if (userResult.IsFailure)
                return Result.Failure(userResult.TryGetError());

            User user = userResult.TryGetValue();

            // Step 4: Upload profile image using UploadFileCommand
            if (request.ProfileImage is not null)
            {
                var uploadFileCommand = new UploadImageCommand(
                    EntityType: nameof(User),
                     EntityId: userId,
                    folder: DefaultFolders.Profiles,
                    Image: request.ProfileImage
                );

                Result<UploadImageResponse> uploadResult = await sender.Send(uploadFileCommand, cancellationToken);

                if (uploadResult.IsFailure)
                    return Result.Failure(uploadResult.TryGetError());

                // Assign uploaded file as profile image
                Result assignResult = user.AssignProfileImage(uploadResult.TryGetValue().FileId);
                if (assignResult.IsFailure)
                    return Result.Failure(assignResult.TryGetError());
            }

            // Step 5: Fire the event and save
            user.FireUserCreatedEvent();
            await db.Users.AddAsync(user, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);

            // Step 6: Commit transaction
            await transaction.CommitAsync(cancellationToken);

            return Result.Success();
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
