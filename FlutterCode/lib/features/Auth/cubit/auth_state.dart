import 'package:famxpense/models/Family/FamilyInfo.dart';

sealed class AuthState {}

class AuthInitial extends AuthState {}

class AuthChecking extends AuthState {}

class AuthAuthenticated extends AuthState {
  final String userId;
  final String email;
  final String fullName;
  final String? profileImageUrl;
  final List<FamilyInfo> families;

  AuthAuthenticated({
    required this.userId,
    required this.email,
    required this.fullName,
    this.profileImageUrl,
    required this.families,
  });
}

class AuthUnauthenticated extends AuthState {}

class AuthLoading extends AuthState {}

class AuthError extends AuthState {
  final String message;

  AuthError(this.message);
}
