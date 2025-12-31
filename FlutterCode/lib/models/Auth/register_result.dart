class RegisterResult {
  final bool isSuccess;
  final String? email;
  final String? errorMessage;

  RegisterResult._({
    required this.isSuccess,
    this.email,
    this.errorMessage,
  });

  factory RegisterResult.success({required String email}) {
    return RegisterResult._(
      isSuccess: true,
      email: email,
    );
  }

  factory RegisterResult.failure(String message) {
    return RegisterResult._(
      isSuccess: false,
      errorMessage: message,
    );
  }
}
