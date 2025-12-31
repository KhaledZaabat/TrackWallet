import 'package:famxpense/models/Family/FamilyInfo.dart';

sealed class AuthState {}

/// Initial state when app starts
class AuthInitial extends AuthState {}

/// Checking if user is already logged in (checking stored tokens)
class AuthChecking extends AuthState {}

/// User is authenticated
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

/// User is not authenticated
class AuthUnauthenticated extends AuthState {}

/// Loading during login/logout
class AuthLoading extends AuthState {}

/// Login failed
class AuthError extends AuthState {
  final String message;

  AuthError(this.message);
}
