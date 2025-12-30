using Expense_Tracker.Domain.Common.ResultPattern.Result;
using MediatR;

namespace Expense_Tracker.Application.Features.Identity.Commands.VerifyOtp;

public record VerifyOtpCommand(string Email, string Otp) : IRequest<Result>;
