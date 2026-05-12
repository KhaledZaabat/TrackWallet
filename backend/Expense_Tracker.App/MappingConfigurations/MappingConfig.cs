using Expense_Tracker.Application.Dtos;
using Expense_Tracker.Contracts.Reponses.Category;
using Expense_Tracker.Contracts.Reponses.Family;
using Expense_Tracker.Contracts.Reponses.Identity;
using Expense_Tracker.Contracts.Reponses.Inv;
using Expense_Tracker.Contracts.Reponses.Transaction;
using Expense_Tracker.Domain.CategoryFolder;
using Expense_Tracker.Domain.Invitation;
using Expense_Tracker.Domain.TransactionFolder;
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
        config.NewConfig<(AuthDto dto, string? url, List<FamilyResponse>? families), AuthResponse>()
            .Map(dest => dest.UserId, src => src.dto.UserId)
            .Map(dest => dest.FullName, src => src.dto.FullName)
            .Map(dest => dest.Email, src => src.dto.Email)
            .Map(dest => dest.ProfileImageUrl, src => src.url)
            .Map(dest => dest.Families, src => src.families);

        // ===============================
        // (AuthDto, ProfileImageUrl) -> AuthResponse
        // Legacy support for backward compatibility
        // ===============================
        config.NewConfig<(AuthDto dto, string? url), AuthResponse>()
            .Map(dest => dest.UserId, src => src.dto.UserId)
            .Map(dest => dest.FullName, src => src.dto.FullName)
            .Map(dest => dest.Email, src => src.dto.Email)
            .Map(dest => dest.ProfileImageUrl, src => src.url);

        // ===============================
        // AuthDto -> AuthResponse
        // Legacy support for backward compatibility
        // ===============================
        config.NewConfig<AuthDto, AuthResponse>()
            .Map(dest => dest.UserId, src => src.UserId)
            .Map(dest => dest.FullName, src => src.FullName)
            .Map(dest => dest.Email, src => src.Email)
            .Map(dest => dest.ProfileImageUrl, src => (string?)null);

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


        TypeAdapterConfig<Category, CategoryResponse>
           .NewConfig()
           .Map(dest => dest.CategoryId, src => src.Id)
           .Map(dest => dest.Name, src => src.Type);

        TypeAdapterConfig<Transaction, TransactionResponse>
           .NewConfig()
           .Map(dest => dest.TransactionId, src => src.Id)
           .Map(dest => dest.Title, src => src.Title)
           .Map(dest => dest.Amount, src => src.Amount)
           .Map(dest => dest.Type, src => src.Type)
           .Map(dest => dest.TransactedOn, src => src.TransactedOn)
           .Map(dest => dest.Notes, src => src.Notes)
           .Map(dest => dest.CreatedAtUtc, src => src.CreatedAtUtc)
           .Map(dest => dest.Category, src => src.Category)
           .Map(dest => dest.Creator, src => src.CreatedBy);

        TypeAdapterConfig<Invitation, InvitationResponse>
           .NewConfig()
           .Map(dest => dest.InvitationId, src => src.Id)
           .Map(dest => dest.InviteeUserId, src => src.InviteeUserId)
           .Map(dest => dest.InviterUserId, src => src.InviterUserId)
           .Map(dest => dest.FamilyId, src => src.FamilyId)
           .Map(dest => dest.IsParent, src => src.IsParent)
           .Map(dest => dest.Status, src => src.Status)
           .Map(dest => dest.SentAtUtc, src => src.SentAtUtc);

        TypeAdapterConfig<Invitation, InvitationResponse>
            .NewConfig()
            .Map(dest => dest.InvitationId, src => src.Id)
            .Map(dest => dest.InviteeUserId, src => src.InviteeUserId)
            .Map(dest => dest.InviterUserId, src => src.InviterUserId)
            .Map(dest => dest.FamilyId, src => src.FamilyId)
            .Map(dest => dest.IsParent, src => src.IsParent)
            .Map(dest => dest.Status, src => src.Status)
            .Map(dest => dest.SentAtUtc, src => src.SentAtUtc);
    }

}
