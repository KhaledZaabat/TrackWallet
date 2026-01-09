sealed class SignupState {}

/// Initial state
class SignupInitial extends SignupState {}

/// Loading during registration
class SignupLoading extends SignupState {}

/// Registration successful - needs OTP verification
class SignupSuccess extends SignupState {
  final String email;

  SignupSuccess({required this.email});
}

/// Registration failed
class SignupError extends SignupState {
  final String message;

  SignupError(this.message);
}

/// OTP verification loading
class OtpVerificationLoading extends SignupState {}

/// OTP verified successfully
class OtpVerificationSuccess extends SignupState {}

/// OTP verification failed
class OtpVerificationError extends SignupState {
  final String message;

  OtpVerificationError(this.message);
}

/// Resending OTP
class OtpResending extends SignupState {}

/// OTP resent successfully
class OtpResent extends SignupState {
  final String message;

  OtpResent(this.message);
}

/// OTP resend failed
class OtpResendError extends SignupState {
  final String message;

  OtpResendError(this.message);
}
