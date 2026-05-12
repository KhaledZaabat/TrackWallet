using Expense_Tracker.Domain.Users;
using Expense_Tracker.Application.Interfaces;
namespace Expense_Tracker.Application.Features.UsersFeatures.Queries.GetUsers;

//public sealed class GetUsersQueryHandler(
//    IRepository<User> userRepo
//)
//{
//    public async Task<ErrorOr<IReadOnlyList<UserListItemDto>>> Handle(
//        GetUsersQuery request,
//        CancellationToken ct)
//    {
//        IQueryable<User> query =
//            userRepo.Query()
//              .IgnoreQueryFilters();

//        if (request.Role is not null)
//        {
//            query = query.Where(u => u.Role == request.Role.Value);
//        }

//        List<UserListItemDto> users =
//            await query
//                .Select(u => new UserListItemDto(
//                    u.Id,
//                    u.FullName,
//                    u.Email,
//                    u.Role,
//                    u.Status
//                ))
//                .ToListAsync(ct);

//        return users;
//    }
//}
