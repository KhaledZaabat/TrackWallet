// features/auth/presentation/reset_password/cubit/reset_password_state.dart

sealed class ResetPasswordState {}

/// Initial state
class ResetPasswordInitial extends ResetPasswordState {}

/// Sending OTP
class SendingOtp extends ResetPasswordState {}

/// OTP sent successfully
class OtpSent extends ResetPasswordState {
  final String email;
  OtpSent({required this.email});
}

/// Failed to send OTP
class SendOtpError extends ResetPasswordState {
  final String message;
  SendOtpError(this.message);
}

/// Verifying OTP
class VerifyingOtp extends ResetPasswordState {}

/// OTP verified successfully
class OtpVerified extends ResetPasswordState {
  final String email;
  OtpVerified({required this.email});
}

/// OTP verification failed
class VerifyOtpError extends ResetPasswordState {
  final String message;
  VerifyOtpError(this.message);
}

/// Resending OTP
class ResendingOtp extends ResetPasswordState {}

/// OTP resent successfully
class OtpResent extends ResetPasswordState {
  final String message;
  OtpResent(this.message);
}

/// Resend OTP failed
class ResendOtpError extends ResetPasswordState {
  final String message;
  ResendOtpError(this.message);
}

/// Resetting password
class ResettingPassword extends ResetPasswordState {}

/// Password reset successful
class PasswordResetSuccess extends ResetPasswordState {}

/// Password reset failed
class ResetPasswordError extends ResetPasswordState {
  final String message;
  ResetPasswordError(this.message);
}
