sealed class SignupState {}

class SignupInitial extends SignupState {}

class SignupLoading extends SignupState {}

class SignupSuccess extends SignupState {
  final String email;

  SignupSuccess({required this.email});
}

class SignupError extends SignupState {
  final String message;

  SignupError(this.message);
}

class OtpVerificationLoading extends SignupState {}

class OtpVerificationSuccess extends SignupState {}

class OtpVerificationError extends SignupState {
  final String message;

  OtpVerificationError(this.message);
}

class OtpResending extends SignupState {}

class OtpResent extends SignupState {
  final String message;

  OtpResent(this.message);
}

class OtpResendError extends SignupState {
  final String message;

  OtpResendError(this.message);
}
