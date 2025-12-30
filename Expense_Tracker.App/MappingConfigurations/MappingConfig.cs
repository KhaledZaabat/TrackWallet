using Expense_Tracker.Application.Dtos;
using Expense_Tracker.Contracts.Reponses.Family;
using Expense_Tracker.Contracts.Reponses.Identity;
using Expense_Tracker.Infrastructure.Idenitity;
using Mapster;

namespace Expense_Tracker.App.MappingConfigurations;


public class MappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // ===============================
        // (AuthDto, ProfileImageUrl, FamilyResponse) -> AuthResponse
        // Main mapping used in LoginCommandHandler
        // ===============================
        config.NewConfig<(AuthDto dto, string? url, FamilyResponse? family), AuthResponse>()
            .Map(dest => dest.UserId, src => src.dto.UserId)
            .Map(dest => dest.FullName, src => src.dto.FullName)
            .Map(dest => dest.Email, src => src.dto.Email)
            .Map(dest => dest.JwtToken, src => src.dto.JwtToken)
            .Map(dest => dest.RefreshToken, src => src.dto.RefreshToken)
            .Map(dest => dest.ProfileImageUrl, src => src.url)
            .Map(dest => dest.FamilyResponse, src => src.family);

        // ===============================
        // (AuthDto, ProfileImageUrl) -> AuthResponse
        // Legacy support for backward compatibility
        // ===============================
        config.NewConfig<(AuthDto dto, string? url), AuthResponse>()
            .Map(dest => dest.UserId, src => src.dto.UserId)
            .Map(dest => dest.FullName, src => src.dto.FullName)
            .Map(dest => dest.Email, src => src.dto.Email)
            .Map(dest => dest.JwtToken, src => src.dto.JwtToken)
            .Map(dest => dest.RefreshToken, src => src.dto.RefreshToken)
            .Map(dest => dest.ProfileImageUrl, src => src.url)
            .Map(dest => dest.FamilyResponse, src => (FamilyResponse?)null);

        // ===============================
        // AuthDto -> AuthResponse
        // Legacy support for backward compatibility
        // ===============================
        config.NewConfig<AuthDto, AuthResponse>()
            .Map(dest => dest.UserId, src => src.UserId)
            .Map(dest => dest.FullName, src => src.FullName)
            .Map(dest => dest.Email, src => src.Email)
            .Map(dest => dest.JwtToken, src => src.JwtToken)
            .Map(dest => dest.RefreshToken, src => src.RefreshToken)
            .Map(dest => dest.ProfileImageUrl, src => (string?)null)
            .Map(dest => dest.FamilyResponse, src => (FamilyResponse?)null);

        // ===============================
        // ApplicationUser -> AuthenticatedUser
        // Maps identity user to authenticated user without role
        // ===============================
        config.NewConfig<ApplicationUser, AuthenticatedUser>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Email, src => src.Email!)
            .Map(dest => dest.UserName, src => src.UserName!)
            .Ignore(dest => dest.Role);

        // ===============================
        // (ApplicationUser, Role) -> AuthenticatedUser
        // Maps identity user with role to authenticated user
        // ===============================
        config.NewConfig<(ApplicationUser user, string? role), AuthenticatedUser>()
            .Map(dest => dest.Id, src => src.user.Id)
            .Map(dest => dest.Email, src => src.user.Email!)
            .Map(dest => dest.UserName, src => src.user.UserName!)
            .Map(dest => dest.Role, src => src.role);
    }
}