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

  Future<void> loginWithGoogle() async {
    emit(AuthLoading());

    try {
      AppLogger.info(_tag, 'Starting Google Sign-In flow');

      final idToken = await _googleSignInService.signIn();

      if (idToken == null) {
        AppLogger.info(_tag, 'Google Sign-In cancelled by user');
        emit(AuthUnauthenticated());
        return;
      }

      AppLogger.info(_tag, 'Authenticating with backend');
      final result = await _authRepository.loginWithGoogle(idToken);

      if (result.isSuccess && result.data != null) {
        final data = result.data!;
        AppLogger.info(_tag, 'Google login successful for user: ${data.email}');
        _emitAuthenticatedState(data);
      } else {
        AppLogger.error(
            _tag, 'Backend authentication failed: ${result.errorMessage}');
        _emitErrorAndReset(result.errorMessage ?? 'Google login failed');
      }
    } on GoogleSignInException catch (e, stackTrace) {
      AppLogger.error(
        _tag,
        'Google Sign-In exception',
        error: e,
        stackTrace: stackTrace,
      );
      _emitErrorAndReset('Failed to sign in with Google. Please try again.');
    } catch (e, stackTrace) {
      AppLogger.error(
        _tag,
        'Unexpected error during Google login',
        error: e,
        stackTrace: stackTrace,
      );
      _emitErrorAndReset('An unexpected error occurred');
    }
  }

  Future<void> logout() async {
    emit(AuthLoading());

    try {
      await _authRepository.logout();

      if (await _googleSignInService.isSignedIn()) {
        await _googleSignInService.disconnect();
      }

      emit(AuthUnauthenticated());
    } catch (e, stackTrace) {
      AppLogger.error(_tag, 'Logout error', error: e, stackTrace: stackTrace);
      emit(AuthUnauthenticated());
    }
  }

  void _emitAuthenticatedState(dynamic data) {
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

  Future<void> _emitErrorAndReset(String message) async {
    emit(AuthError(message));
    await Future.delayed(const Duration(milliseconds: 100));
    emit(AuthUnauthenticated());
  }
}
