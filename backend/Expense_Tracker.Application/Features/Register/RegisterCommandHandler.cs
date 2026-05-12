using ErrorOr;
using Expense_Tracker.Application.Constants;
using Expense_Tracker.Application.Dtos;
using Expense_Tracker.Application.Events;
using Expense_Tracker.Application.Features.FilesFolder.Commads.UploadImage;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Contracts.Responses.Files;
using Expense_Tracker.Domain.Users;
using Wolverine;
using Expense_Tracker.Domain.Errors;

namespace Expense_Tracker.Application.Features.Register;

public sealed class RegisterCommandHandler(
    IIdentityService identityService,
    IRepository<User> users,
    IMessageBus bus)
{
    public async Task<ErrorOr<Success>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        // Step 1: Create Identity User with password
        ErrorOr<IdentityRegistrationResult> identityResult = await identityService.CreateIdentityByEmailAsync(
            email: request.Email,
            password: request.Password,
            userName: request.UserName,
            cancellationToken);

        if (identityResult.IsError)
            return identityResult.Errors;

        var identity = identityResult.Value;
        Guid userId = Guid.Parse(identity.IdentityUserId);

        // Step 2: Create Domain User
        ErrorOr<User> userResult = User.Create(
            id: userId,
            fullName: request.FullName,
            userName: request.UserName,
            email: request.Email,
            birthDate: request.BirthDate,
            isMale: request.IsMale);

        if (userResult.IsError)
            return userResult.Errors;

        User user = userResult.Value;

        // Step 3: Upload profile image using UploadFileCommand
        if (request.ProfileImage is not null)
        {
            var uploadFileCommand = new UploadImageCommand(
                EntityType: nameof(User),
                EntityId: userId,
                folder: DefaultFolders.Profiles,
                Image: request.ProfileImage
            );

            ErrorOr<UploadImageResponse> uploadResult = await bus.InvokeAsync<ErrorOr<UploadImageResponse>>(uploadFileCommand, cancellationToken);

            if (uploadResult.IsError)
                return uploadResult.Errors;

            // Assign uploaded file as profile image
            ErrorOr<Success> assignResult = user.AssignProfileImage(uploadResult.Value.FileId);
            if (assignResult.IsError)
                return assignResult.Errors;
        }

        // Step 4: Save user
        await users.AddAsync(user, cancellationToken);
        await users.SaveChangesAsync(cancellationToken);

        // Step 5: Publish event
        await bus.PublishAsync(new UserCreatedEvent(user));

        return new Success();
    }
}
