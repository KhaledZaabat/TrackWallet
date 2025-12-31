// features/auth/presentation/cubit/auth_cubit.dart
import 'package:famxpense/data/database/repositories/concrete/auth_repository.dart';
import 'package:famxpense/features/auth/presentation/Auth/cubit/auth_state.dart';
import 'package:famxpense/models/Family/FamilyInfo.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

class AuthCubit extends Cubit<AuthState> {
  final AuthRepository _authRepository;

  AuthCubit(this._authRepository) : super(AuthInitial());

  /// Check authentication status on app start
  Future<void> checkAuthStatus() async {
    emit(AuthChecking());

    try {
      final isAuthenticated = await _authRepository.isAuthenticated();

      if (isAuthenticated) {
        // User has refresh token
        final userId = await _authRepository.getCurrentUserId();

        if (userId != null) {
          // You might want to fetch user details here
          // For now, we'll emit authenticated with minimal data
          emit(AuthAuthenticated(
            // fetch the home infromations
            userId: userId,
            email: '',
            fullName: '',
            families: [],
          ));
        } else {
          emit(AuthUnauthenticated());
        }
      } else {
        emit(AuthUnauthenticated());
      }
    } catch (e) {
      emit(AuthUnauthenticated());
    }
  }

  /// Login with email/username and password
  Future<void> login({
    required String identifier,
    required String password,
  }) async {
    emit(AuthLoading());

    try {
      final result = await _authRepository.login(
        identifier: identifier,
        password: password,
      );

      if (result.isSuccess && result.data != null) {
        final data = result.data!;
        emit(AuthAuthenticated(
          userId: data.userId,
          email: data.email,
          fullName: data.fullName,
          profileImageUrl: data.profileImageUrl,
          families: data.families
              .map((f) => FamilyInfo(
                    id: f.id,
                    name: f.name,
                    currentBudget: f.currentBudget,
                    familyBio: f.familyBio,
                  ))
              .toList(),
        ));
      } else {
        emit(AuthError(result.errorMessage ?? 'Login failed'));
        // Return to unauthenticated after showing error
        await Future.delayed(const Duration(milliseconds: 100));
        emit(AuthUnauthenticated());
      }
    } catch (e) {
      emit(AuthError('An unexpected error occurred'));
      await Future.delayed(const Duration(milliseconds: 100));
      emit(AuthUnauthenticated());
    }
  }

  /// Login with Google
  Future<void> loginWithGoogle(String idToken) async {
    emit(AuthLoading());

    try {
      final result = await _authRepository.loginWithGoogle(idToken);

      if (result.isSuccess && result.data != null) {
        final data = result.data!;
        emit(AuthAuthenticated(
          userId: data.userId,
          email: data.email,
          fullName: data.fullName,
          profileImageUrl: data.profileImageUrl,
          families: data.families
              .map((f) => FamilyInfo(
                    id: f.id,
                    name: f.name,
                    currentBudget: f.currentBudget,
                    familyBio: f.familyBio,
                  ))
              .toList(),
        ));
      } else {
        emit(AuthError(result.errorMessage ?? 'Google login failed'));
        await Future.delayed(const Duration(milliseconds: 100));
        emit(AuthUnauthenticated());
      }
    } catch (e) {
      emit(AuthError('An unexpected error occurred'));
      await Future.delayed(const Duration(milliseconds: 100));
      emit(AuthUnauthenticated());
    }
  }

  /// Logout
  Future<void> logout() async {
    emit(AuthLoading());

    try {
      await _authRepository.logout();
      emit(AuthUnauthenticated());
    } catch (e) {
      // Still logout locally even if API fails
      emit(AuthUnauthenticated());
    }
  }
}
