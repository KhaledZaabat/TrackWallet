using Expense_Tracker.Application.Dtos;
using Expense_Tracker.Contracts.Reponses.Identity;
using Expense_Tracker.Infrastructure.Idenitity;
using Mapster;

namespace Expense_Tracker.App.MappingConfigurations;

public class MappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {




        config.NewConfig<(AuthDto dto, string url), AuthResponse>()
             .Map(des => des.UserId, src => src.dto.UserId)
           .Map(des => des.FullName, src => src.dto.FullName)
           .Map(des => des.Email, src => src.dto.Email)
           .Map(des => des.JwtToken, src => src.dto.JwtToken)
           .Map(des => des.RefreshToken, src => src.dto.RefreshToken)
          .Map(des => des.ProfileImageUrl, src => src.url);

        config.NewConfig<AuthDto, AuthResponse>()
                  .Map(des => des.UserId, src => src.UserId)
                .Map(des => des.FullName, src => src.FullName)
                .Map(des => des.Email, src => src.Email)
                .Map(des => des.JwtToken, src => src.JwtToken)
                .Map(des => des.RefreshToken, src => src.RefreshToken);

        // ===============================
        // ApplicationUser -> AuthenticatedUser
        // ===============================
        config.NewConfig<ApplicationUser, AuthenticatedUser>()
          .Map(dest => dest.Id, src => src.Id)
          .Map(dest => dest.Email, src => src.Email!)
          .Map(dest => dest.UserName, src => src.UserName!)
          .Ignore(dest => dest.Role);

        // ===============================
        // (ApplicationUser, Role) -> AuthenticatedUser
        // ===============================
        config.NewConfig<(ApplicationUser user, string? role), AuthenticatedUser>()
            .Map(dest => dest.Id, src => src.user.Id)
            .Map(dest => dest.Email, src => src.user.Email!)
            .Map(dest => dest.UserName, src => src.user.UserName!)
            .Map(dest => dest.Role, src => src.role);
    }

}
