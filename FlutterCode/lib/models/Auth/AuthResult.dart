import 'package:famxpense/models/Auth/auth_data.dart';
import 'package:famxpense/models/Family/FamilyInfo.dart';

class AuthResult {
  final bool isSuccess;
  final String? errorMessage;
  final AuthData? data;

  AuthResult._({
    required this.isSuccess,
    this.errorMessage,
    this.data,
  });

  factory AuthResult.success({
    required String userId,
    required String email,
    required String fullName,
    String? profileImageUrl,
    List<FamilyInfo>? families,
  }) {
    return AuthResult._(
      isSuccess: true,
      data: AuthData(
        userId: userId,
        email: email,
        fullName: fullName,
        profileImageUrl: profileImageUrl,
        families: families ?? [],
      ),
    );
  }

  factory AuthResult.failure(String message) {
    return AuthResult._(
      isSuccess: false,
      errorMessage: message,
    );
  }
}
