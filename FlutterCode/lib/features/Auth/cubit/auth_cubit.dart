// features/auth/presentation/cubit/auth_cubit.dart
import 'package:famxpense/core/app_logger.dart';
import 'package:famxpense/core/services/google_sign_in_service.dart';
import 'package:famxpense/data/repos/auth_repository.dart';
import 'package:famxpense/features/Auth/cubit/auth_state.dart';
import 'package:famxpense/models/Family/FamilyInfo.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

class AuthCubit extends Cubit<AuthState> {
  static const String _tag = 'AuthCubit';

  final AuthRepository _authRepository;
  final GoogleSignInService _googleSignInService;

  AuthCubit(
    this._authRepository,
    this._googleSignInService,
  ) : super(AuthInitial());

  /// Check if user is already authenticated
  Future<void> checkAuthStatus() async {
    emit(AuthChecking());

    try {
      final isAuthenticated = await _authRepository.isAuthenticated();

      if (isAuthenticated) {
        final userId = await _authRepository.getCurrentUserId();

        if (userId != null) {
          final refreshResult = await _authRepository.refreshToken();

          if (refreshResult.isSuccess && refreshResult.data != null) {
            final data = refreshResult.data!;

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
            return;
          } else {
            AppLogger.info(_tag, 'Token refresh failed');
          }
        }
      }

      emit(AuthUnauthenticated());
    } catch (e, stackTrace) {
      AppLogger.error(
        _tag,
        'Auth status check failed',
        error: e,
        stackTrace: stackTrace,
      );
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
        _emitAuthenticatedState(data);
      } else {
        _emitErrorAndReset(result.errorMessage ?? 'Login failed');
      }
    } catch (e, stackTrace) {
      AppLogger.error(_tag, 'Login failed', error: e, stackTrace: stackTrace);
      _emitErrorAndReset('An unexpected error occurred');
    }
  }

  /// Login with Google
  Future<void> loginWithGoogle() async {
    AppLogger.step(_tag, 1, '🔐 AUTH CUBIT: Starting Google Login Flow');
    emit(AuthLoading());

    try {
      AppLogger.step(_tag, 2, 'Calling GoogleSignInService.signIn()');
      
      // Get ID token from Google
      final idToken = await _googleSignInService.signIn();

      AppLogger.step(_tag, 3, 'GoogleSignInService returned');
      AppLogger.debug(_tag, 'ID Token result:', data: {
        'hasToken': idToken != null,
        'tokenLength': idToken?.length ?? 0,
      });

      if (idToken == null) {
        // User cancelled the sign-in
        AppLogger.warning(_tag, 'Google Sign-In cancelled - returning to unauthenticated state');
        emit(AuthUnauthenticated());
        return;
      }

      // Authenticate with backend
      AppLogger.step(_tag, 4, 'Sending ID token to backend for authentication');
      AppLogger.info(_tag, 'Token preview: ${idToken.substring(0, 30)}...');
      
      final result = await _authRepository.loginWithGoogle(idToken);

      AppLogger.step(_tag, 5, 'Backend response received');
      AppLogger.debug(_tag, 'Backend result:', data: {
        'isSuccess': result.isSuccess,
        'hasData': result.data != null,
        'errorMessage': result.errorMessage,
      });

      if (result.isSuccess && result.data != null) {
        final data = result.data!;
        AppLogger.success(_tag, '🎉 GOOGLE LOGIN SUCCESSFUL');
        AppLogger.debug(_tag, 'User data:', data: {
          'email': data.email,
          'fullName': data.fullName,
          'userId': data.userId,
          'familiesCount': data.families?.length ?? 0,
        });
        _emitAuthenticatedState(data);
      } else {
        AppLogger.error(_tag, '❌ BACKEND AUTHENTICATION FAILED');
        AppLogger.error(_tag, 'Error message: ${result.errorMessage}');
        _emitErrorAndReset(result.errorMessage ?? 'Google login failed');
      }
    } on GoogleSignInException catch (e, stackTrace) {
      AppLogger.error(_tag, '❌ GoogleSignInException caught', error: e, stackTrace: stackTrace);
      _emitErrorAndReset('Failed to sign in with Google. Please try again.');
    } catch (e, stackTrace) {
      AppLogger.error(_tag, '❌ UNEXPECTED ERROR in loginWithGoogle', error: e, stackTrace: stackTrace);
      _emitErrorAndReset('An unexpected error occurred');
    }
  }

  /// Logout
  Future<void> logout() async {
    emit(AuthLoading());

    try {
      // Logout from backend
      await _authRepository.logout();

      // Disconnect Google account if signed in
      if (await _googleSignInService.isSignedIn()) {
        await _googleSignInService.disconnect();
      }

      emit(AuthUnauthenticated());
    } catch (e, stackTrace) {
      AppLogger.error(_tag, 'Logout error', error: e, stackTrace: stackTrace);
      // Still logout locally even if API fails
      emit(AuthUnauthenticated());
    }
  }

  /// Helper: Emit authenticated state
  void _emitAuthenticatedState(dynamic data) {
    // data.families is already List<FamilyInfo> from the repository
    final List<FamilyInfo> families = data.families is List<FamilyInfo>
        ? data.families
        : (data.families as List?)
                ?.map((f) => f is FamilyInfo
                    ? f
                    : FamilyInfo(
                        id: f.id,
                        name: f.name,
                        currentBudget: f.currentBudget,
                        familyBio: f.familyBio,
                      ))
                .toList() ??
            [];

    emit(AuthAuthenticated(
      userId: data.userId,
      email: data.email,
      fullName: data.fullName,
      profileImageUrl: data.profileImageUrl,
      families: families,
    ));
  }

  /// Helper: Emit error and return to unauthenticated
  Future<void> _emitErrorAndReset(String message) async {
    emit(AuthError(message));
    await Future.delayed(const Duration(milliseconds: 100));
    emit(AuthUnauthenticated());
  }
}
