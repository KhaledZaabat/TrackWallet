import 'package:famxpense/data/repos/auth_repository.dart';
import 'package:famxpense/features/Auth/cubit/signup_state.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

class SignupCubit extends Cubit<SignupState> {
  final AuthRepository _authRepository;
  String? _pendingEmail; // Store email for OTP verification

  SignupCubit(this._authRepository) : super(SignupInitial());

  /// Register a new user
  Future<void> register({
    required String email,
    required String password,
    required String username,
    required String fullName,
    required DateTime birthDate,
    required bool isMale,
    String? profileImagePath,
  }) async {
    emit(SignupLoading());

    try {
      final result = await _authRepository.register(
        email: email,
        password: password,
        username: username,
        fullName: fullName,
        birthDate: birthDate,
        isMale: isMale,
        profileImagePath: profileImagePath,
      );

      if (result.isSuccess && result.email != null) {
        _pendingEmail = result.email;
        emit(SignupSuccess(email: result.email!));
      } else {
        emit(SignupError(result.errorMessage ?? 'Registration failed'));
        // Return to initial after showing error
        await Future.delayed(const Duration(milliseconds: 100));
        emit(SignupInitial());
      }
    } catch (e) {
      emit(SignupError('An unexpected error occurred'));
      await Future.delayed(const Duration(milliseconds: 100));
      emit(SignupInitial());
    }
  }

  /// Verify OTP
  Future<void> verifyOtp({
    required String otp,
    String? email,
  }) async {
    final emailToVerify = email ?? _pendingEmail;

    if (emailToVerify == null) {
      emit(OtpVerificationError('Email not found. Please register again.'));
      return;
    }

    emit(OtpVerificationLoading());

    try {
      final result = await _authRepository.confirmAccount(
        email: emailToVerify,
        otp: otp,
      );

      if (result.isSuccess) {
        emit(OtpVerificationSuccess());
      } else {
        emit(OtpVerificationError(result.message ?? 'OTP verification failed'));
        // Return to initial OTP state after showing error
        await Future.delayed(const Duration(milliseconds: 100));
        emit(SignupSuccess(email: emailToVerify));
      }
    } catch (e) {
      emit(OtpVerificationError('An unexpected error occurred'));
      await Future.delayed(const Duration(milliseconds: 100));
      if (emailToVerify != null) {
        emit(SignupSuccess(email: emailToVerify));
      }
    }
  }

  /// Resend OTP
  Future<void> resendOtp({String? email}) async {
    final emailToResend = email ?? _pendingEmail;

    if (emailToResend == null) {
      emit(OtpResendError('Email not found. Please register again.'));
      return;
    }

    emit(OtpResending());

    try {
      final result = await _authRepository.resendConfirmationOtp(
        email: emailToResend,
      );

      if (result.isSuccess) {
        emit(OtpResent(result.message ?? 'OTP sent successfully'));
        // Return to success state to allow entering OTP
        await Future.delayed(const Duration(seconds: 2));
        emit(SignupSuccess(email: emailToResend));
      } else {
        emit(OtpResendError(result.message ?? 'Failed to resend OTP'));
        await Future.delayed(const Duration(milliseconds: 100));
        emit(SignupSuccess(email: emailToResend));
      }
    } catch (e) {
      emit(OtpResendError('An unexpected error occurred'));
      await Future.delayed(const Duration(milliseconds: 100));
      if (emailToResend != null) {
        emit(SignupSuccess(email: emailToResend));
      }
    }
  }

  /// Reset to initial state
  void reset() {
    _pendingEmail = null;
    emit(SignupInitial());
  }

  /// Get pending email
  String? get pendingEmail => _pendingEmail;
}
