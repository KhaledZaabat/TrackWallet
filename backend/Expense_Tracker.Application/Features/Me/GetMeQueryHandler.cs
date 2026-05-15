using ErrorOr;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Contracts.Reponses.Identity;
using Expense_Tracker.Domain.Users;
using Microsoft.Extensions.DependencyInjection;

public sealed class GetMeQueryHandler(IRepository<User> users, IFileUrlResolver fileUrlResolver)
{
    public async Task<ErrorOr<MeResult>> HandleAsync(
        GetMeQuery query,
        CancellationToken ct)
    {
        var user = await users.GetByIdAsync
            (query.UserId, ct);

        if (user is null)
            return Error.NotFound("User.NotFound", "User not found.");

        return new MeResult(
            user.Id,
            user.Email,
            user.UserName,
            user.FullName,
            user.BirthDate,
            user.IsMale,
            user.ProfileImageFileId.HasValue
                                 ? fileUrlResolver.GetUrl(user.ProfileImageFileId.Value)
                                 : null
        );
    }
}