using Expense_Tracker.Application.Dtos;
using Expense_Tracker.Contracts.Reponses.Identity;
using Expense_Tracker.Infrastructure.Idenitity;
using Mapster;

namespace Expense_Tracker.App.MappingConfigurations;

public class MappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {


        config.NewConfig<(ApplicationUser user, string? role), AuthenticatedUser>()
           .Map(des => des.Email, src => src.user.Email)
           .Map(des => des.Id, src => src.user.Id)
           .Map(des => des.FullName, src => src.user.UserName)
           .Map(des => des.Role, src => src.role);


        config.NewConfig<(ApplicationUser user, string role), IdentityRegistrationResult>()
           .Map(des => des.IdentityUserId, src => src.user.Id)
           .Map(des => des.FullName, src => src.user.UserName)
           .Map(des => des.Email, src => src.user.Email)
           .Map(des => des.Role, src => src.role);

        config.NewConfig<AuthenticatedUser, AuthResponse>()
             .Map(des => des.UserId, src => src.Id)
           .Map(des => des.FullName, src => src.FullName)
           .Map(des => des.Email, src => src.Email)
           .Map(des => des.Role, src => src.Role);





    }
}