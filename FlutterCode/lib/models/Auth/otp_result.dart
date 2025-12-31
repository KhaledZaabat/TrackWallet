class OtpResult {
  final bool isSuccess;
  final String? message;

  OtpResult._({
    required this.isSuccess,
    this.message,
  });

  factory OtpResult.success({required String message}) {
    return OtpResult._(
      isSuccess: true,
      message: message,
    );
  }

  factory OtpResult.failure(String message) {
    return OtpResult._(
      isSuccess: false,
      message: message,
    );
  }
}
