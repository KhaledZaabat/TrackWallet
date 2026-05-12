sealed class ResetPasswordState {}

class ResetPasswordInitial extends ResetPasswordState {}

class SendingOtp extends ResetPasswordState {}

class OtpSent extends ResetPasswordState {
  final String email;
  OtpSent({required this.email});
}

class SendOtpError extends ResetPasswordState {
  final String message;
  SendOtpError(this.message);
}

class VerifyingOtp extends ResetPasswordState {}

class OtpVerified extends ResetPasswordState {
  final String email;
  OtpVerified({required this.email});
}

class VerifyOtpError extends ResetPasswordState {
  final String message;
  VerifyOtpError(this.message);
}

class ResendingOtp extends ResetPasswordState {}

class OtpResent extends ResetPasswordState {
  final String message;
  OtpResent(this.message);
}

class ResendOtpError extends ResetPasswordState {
  final String message;
  ResendOtpError(this.message);
}

class ResettingPassword extends ResetPasswordState {}

class PasswordResetSuccess extends ResetPasswordState {}

class ResetPasswordError extends ResetPasswordState {
  final String message;
  ResetPasswordError(this.message);
}
